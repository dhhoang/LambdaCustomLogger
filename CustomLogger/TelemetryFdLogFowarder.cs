using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace CustomLogger;

internal class TelemetryFdLogFowarder : ILambdaLogForwarder, IDisposable
{
    /// <summary>
    /// This will prevent <see cref="FileStream"/> from creating user-space buffer and use kernel buffer.
    /// </summary>
    private const int DefaultFdBufferSize = 4096;

    /// <summary>
    /// Write log message to the file descriptor which will make sure the message is recorded as a single CloudWatch Log record.
    /// The format of the message must be:
    /// +----------------------+------------------------+-----------------------+
    /// | Frame Type - 4 bytes | Length (len) - 4 bytes | Message - 'len' bytes |
    /// +----------------------+------------------------+-----------------------+
    /// The first 4 bytes are the frame type. For logs this is always 0xa55a0001.
    /// The second 4 bytes are the length of the message.
    /// The remaining bytes are the message itself. Byte order is big-endian.
    /// </summary>
    private const uint LambdaTelemetryLogHeaderFrameType = 0xa55a0001;

    private static readonly byte[] LineBreakBytes = LoggerHelper.Utf8NoBomNoThrow.GetBytes(Environment.NewLine);

    private readonly SafeFileHandle _fd;
    private readonly FileStream _fileStream;
    private readonly object _lock = new object();

    public TelemetryFdLogFowarder(int fileDescriptor)
    {
        // the handle should not own the fd because it's not opened by us
        _fd = new SafeFileHandle(new IntPtr(fileDescriptor), false);
        _fileStream = new FileStream(_fd, FileAccess.Write, DefaultFdBufferSize, isAsync: false);
    }

    public void Forward(string entry)
    {
        var maxBufSize = LoggerHelper.Utf8NoBomNoThrow.GetMaxByteCount(entry.Length) + 8;
        var bufferSpan = maxBufSize > 1024
            ? new byte[maxBufSize]
            : stackalloc byte[1024];

        BinaryPrimitives.WriteUInt32BigEndian(bufferSpan[..4], LambdaTelemetryLogHeaderFrameType);
        var utf8Size = Encoding.UTF8.GetBytes(entry, bufferSpan[8..]);
        BinaryPrimitives.WriteInt32BigEndian(bufferSpan.Slice(4, 4), utf8Size);

        var writtenSpan = utf8Size + 8;
        lock (_lock)
        {
            _fileStream.Write(bufferSpan[..writtenSpan]);
            _fileStream.Flush();
        }
    }

    public void Forward(ReadOnlySpan<byte> data)
    {
        Span<byte> frameHeader = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(frameHeader[..4], LambdaTelemetryLogHeaderFrameType);
        BinaryPrimitives.WriteInt32BigEndian(frameHeader.Slice(4, 4), data.Length);

        lock (_lock)
        {
            _fileStream.Write(frameHeader);
            _fileStream.Write(data);
            _fileStream.Flush();
        }
    }

    public void Dispose()
    {
        _fileStream.Dispose();
        _fd.Dispose();
    }
}
