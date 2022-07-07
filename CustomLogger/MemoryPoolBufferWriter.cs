using System;
using System.Buffers;
using System.Diagnostics;

namespace CustomLogger;

internal sealed class MemoryPoolBufferWriter : IBufferWriter<byte>, IDisposable
{
    private readonly MemoryPool<byte> _memoryPool;
    private IMemoryOwner<byte> _rentedMemory;
    private int _idx;

    public MemoryPoolBufferWriter(MemoryPool<byte> memoryPool, int initialCapacity)
    {
        Debug.Assert(initialCapacity > 0);

        _memoryPool = memoryPool;
        _rentedMemory = _memoryPool.Rent(initialCapacity);
    }

    public ReadOnlyMemory<byte> WrittenMemory
    {
        get
        {
            Debug.Assert(_idx <= _rentedMemory.Memory.Length);
            return _rentedMemory.Memory[.._idx];
        }
    }

    /// <inheritdoc />
    public void Advance(int count)
    {
        if (count < 0)
        {
            throw new ArgumentException("Parameter 'count' must be non-negative", nameof(count));
        }

        if (_idx > _rentedMemory.Memory.Length - count)
        {
            throw new InvalidOperationException($"Cannot advance by ${count} bytes, active buffer is {GetSpan().Length} bytes");
        }

        _idx += count;
    }

    /// <inheritdoc />
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentException(null, nameof(sizeHint));
        }

        EnsureBuffer(sizeHint + _idx);
        return _rentedMemory.Memory[_idx..];
    }

    /// <inheritdoc />
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentException(null, nameof(sizeHint));
        }

        EnsureBuffer(sizeHint + _idx);
        return _rentedMemory.Memory.Span[_idx..];
    }

    private void EnsureBuffer(int minimumSize)
    {
        Debug.Assert(minimumSize >= 0);

        var wholeBufSize = _rentedMemory.Memory.Length;
        if (minimumSize >= wholeBufSize)
        {
            return;
        }

        var oldMem = _rentedMemory;
        while (wholeBufSize < minimumSize)
        {
            wholeBufSize *= 2;
        }

        var newMem = _memoryPool.Rent(wholeBufSize);

        Debug.Assert(newMem.Memory.Length >= minimumSize);
        oldMem.Memory[.._idx].CopyTo(newMem.Memory);

        _rentedMemory = newMem;
        oldMem.Dispose();
    }

    public void Dispose()
    {
        var idx = _idx;
        if (idx < 0)
        {
            return;
        }

        _idx = -1;

        _memoryPool.Dispose();
    }
}
