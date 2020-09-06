// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace MDSDK.JPEG2000.Utils
{
    abstract class ByteReader
    {
        protected byte[] _buffer;

        protected int _bufferReadPos;

        protected int _bufferReadEnd;

        private long _position;

        private long _endPosition;

        protected ByteReader(long streamLength)
        {
            _position = 0;
            _endPosition = streamLength;
        }

        protected abstract void FillBuffer(long bytesLeftInWindow);
        
        protected abstract void InputDirect(byte[] buffer, int offset, int count);

        protected abstract void Skip(long count);

        public long Position
        {
            get { return _position; }
            private set
            {
                if (value > _endPosition)
                {
                    throw new IOException($"Attempt to consume {value - _endPosition} more bytes than available");
                }
                _position = value;
            }
        }

        public IDisposable Window(long length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }

            var newEndPosition = Position + length;
            if (newEndPosition > _endPosition)
            {
                throw new IOException($"New window exceeds current window by {newEndPosition - _endPosition} bytes");
            }

            var originalEndPosition = _endPosition;

            _endPosition = newEndPosition;

            return new GenericDisposable(() => _endPosition = originalEndPosition);
        }

        private void EnsureBufferContainsAtLeastOneByte()
        {
            if (_bufferReadPos == _bufferReadEnd)
            {
                var bytesLeftInWindow = _endPosition - Position;
                if (bytesLeftInWindow < 1)
                {
                    throw new IOException("End of window reached");
                }
                FillBuffer(bytesLeftInWindow);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByte()
        {
            EnsureBufferContainsAtLeastOneByte();
            Position++;
            return _buffer[_bufferReadPos++];
        }

        public void ReadBytes(byte[] bytes, int offset, int count)
        {
            Position += count;

            var bytesInBuffer = _bufferReadEnd - _bufferReadPos;
            if ((bytesInBuffer == 0) && (count > 0) && (count < 4096))
            {
                EnsureBufferContainsAtLeastOneByte();
                bytesInBuffer = _bufferReadEnd - _bufferReadPos;
            }

            if (count <= bytesInBuffer)
            {
                Buffer.BlockCopy(_buffer, _bufferReadPos, bytes, offset, count);
                _bufferReadPos += count;
            }
            else
            {
                Buffer.BlockCopy(_buffer, _bufferReadPos, bytes, offset, bytesInBuffer);
                _bufferReadPos = _bufferReadEnd;

                offset += bytesInBuffer;
                count -= bytesInBuffer;

                InputDirect(bytes, offset, count);
            }
        }

        public void ReadBytes(byte[] bytes)
        {
            ReadBytes(bytes, 0, bytes.Length);
        }

        public byte[] ReadBytes(int count)
        {
            var bytes = new byte[count];
            ReadBytes(bytes);
            return bytes;
        }

        public void SkipBytes(long count)
        {
            Position += count;

            var bytesInBuffer = _bufferReadEnd - _bufferReadPos;
            if (count <= bytesInBuffer)
            {
                _bufferReadPos += (int)count;
            }
            else
            {
                _bufferReadPos = _bufferReadEnd;
                count -= bytesInBuffer;
                Skip(count);
            }
        }

        public void CopyBytesTo(int count, Stream stream)
        {
            Position += count;

            var bytesInBuffer = _bufferReadEnd - _bufferReadPos;
            if (count <= bytesInBuffer)
            {
                stream.Write(_buffer, _bufferReadPos, count);
                _bufferReadPos += count;
            }
            else
            {
                stream.Write(_buffer, _bufferReadPos, bytesInBuffer);
                _bufferReadPos = _bufferReadEnd;

                count -= bytesInBuffer;

                while (count > _buffer.Length)
                {
                    InputDirect(_buffer, 0, _buffer.Length);
                    stream.Write(_buffer, 0, _buffer.Length);
                    count -= _buffer.Length;
                }

                InputDirect(_buffer, 0, count);
                stream.Write(_buffer, 0, count);
            }
        }

        public bool AtEnd => Position == _endPosition; 

        public long BytesRemaining => _endPosition - Position;

        public void SkipRemainingBytes()
        {
            SkipBytes(BytesRemaining);
        }

        public byte[] ReadRemainingBytes()
        {
            return ReadBytes(checked((int)BytesRemaining));
        }
    }
}

