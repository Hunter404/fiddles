// -----------------------------------------------------------------------
// <copyright file="CircularBufferStream.cs" company="Fredrik Larsson">
//     Copyright (c) 2023 Fredrik Larsson. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

/// <summary>
/// The CircularBufferStream class is a custom implementation of a Stream using a circular buffer, which is a data structure that uses a single, fixed-size buffer as if it were connected end-to-end.
/// This kind of data structure is very useful for situations where you want a fixed-sized buffer and old data can be automatically overwritten when the buffer is full.
/// </summary>
public class CircularBufferStream : Stream
{
    private readonly byte[] _buffer;
    private int _head;
    private int _tail;
    private int _length;

    public CircularBufferStream(int capacity)
    {
        _buffer = new byte[capacity];
        _head = 0;
        _tail = 0;
        _length = 0;
    }

    public override bool CanRead => true;
    public override bool CanSeek => false;
    public override bool CanWrite => true;
    public override long Length => _length;

    public override long Position
    {
        get => throw new NotSupportedException();
        set => throw new NotSupportedException();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_length == 0) return 0;

        var readCount = Math.Min(count, _length);
        var readToEnd = Math.Min(_buffer.Length - _tail, readCount);

        Array.Copy(_buffer, _tail, buffer, offset, readToEnd);

        if (readToEnd < readCount)
        {
            var wrapAroundRead = readCount - readToEnd;
            Array.Copy(_buffer, 0, buffer, offset + readToEnd, wrapAroundRead);
        }

        _tail = (_tail + readCount) % _buffer.Length;
        _length -= readCount;

        return readCount;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        var remainingSpace = _buffer.Length - _length;
        if (count > remainingSpace)
        {
            throw new InvalidOperationException("Not enough space in buffer");
        }

        var writeToEnd = Math.Min(_buffer.Length - _head, count);

        Array.Copy(buffer, offset, _buffer, _head, writeToEnd);

        if (writeToEnd < count)
        {
            var wrapAroundWrite = count - writeToEnd;
            Array.Copy(buffer, offset + writeToEnd, _buffer, 0, wrapAroundWrite);
        }

        _head = (_head + count) % _buffer.Length;
        _length += count;
    }
}
