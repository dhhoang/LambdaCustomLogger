using System;
using System.Buffers;
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
    private const int BufferSizeNoBuffer = 1;

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

    private static readonly byte[] LineBreakBytes = Encoding.UTF8.GetBytes(Environment.NewLine);

    private readonly SafeFileHandle _fd;
    private readonly FileStream _fileStream;

    public TelemetryFdLogFowarder(int fileDescriptor)
    {
        _fd = new SafeFileHandle(new IntPtr(fileDescriptor), false);
        _fileStream = new FileStream(_fd, FileAccess.Write, BufferSizeNoBuffer, isAsync: false);
    }

    public void Forward(string entry)
    {
        var utf8Size = Encoding.UTF8.GetByteCount(entry);

        var bufferSize = utf8Size + 8;
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        var bufferSpan = buffer.AsSpan();

        try
        {
            BinaryPrimitives.WriteUInt32BigEndian(bufferSpan[..4], LambdaTelemetryLogHeaderFrameType);
            BinaryPrimitives.WriteInt32BigEndian(bufferSpan.Slice(4, 4), utf8Size + LineBreakBytes.Length);

            Encoding.UTF8.GetBytes(entry, bufferSpan[8..]);

            lock (_fileStream)
            {
                _fileStream.Write(bufferSpan[..bufferSize]);
                _fileStream.Write(LineBreakBytes);
                _fileStream.Flush();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    public void Forward(ReadOnlySpan<byte> data)
    {
        Span<byte> frameHeader = stackalloc byte[8];
        BinaryPrimitives.WriteUInt32BigEndian(frameHeader[..4], LambdaTelemetryLogHeaderFrameType);
        BinaryPrimitives.WriteInt32BigEndian(frameHeader.Slice(4, 4), data.Length);

        lock (_fileStream)
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
