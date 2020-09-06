// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System;
using System.Diagnostics;
using System.IO;

namespace MDSDK.JPEG2000.Utils
{
    class GenericStreamReader<TStream> : ByteReader where TStream : Stream
    {
        private readonly TStream _stream;

        public GenericStreamReader(TStream stream, int bufferSize = 4096)
            : base(stream.CanSeek ? stream.Length - stream.Position : long.MaxValue)
        {
            _stream = stream;
            _buffer = new byte[bufferSize];
        }

        protected sealed override void FillBuffer(long bytesLeftInWindow)
        {
            Debug.Assert(_bufferReadPos == _bufferReadEnd);

            var maxLength = (int)Math.Min(_buffer.Length, bytesLeftInWindow);
            var n = _stream.Read(_buffer, 0, maxLength);
            if (n == 0)
            {
                throw new IOException("Unexpected end of stream");
            }
            _bufferReadPos = 0;
            _bufferReadEnd = n;
        }

        protected sealed override void InputDirect(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                var n = _stream.Read(buffer, offset, count);
                if (n == 0)
                {
                    throw new IOException("Unexpected end of stream");
                }
                offset += n;
                count -= n;
            }
        }

        protected sealed override void Skip(long count)
        {
            if (_stream.CanSeek)
            {
                var newPosition = _stream.Position + count;
                if (_stream.Seek(count, SeekOrigin.Current) != newPosition)
                {
                    throw new IOException("Seek failed");
                }
            }
            else
            {
                while (count > _buffer.Length)
                {
                    InputDirect(_buffer, 0, _buffer.Length);
                    count -= _buffer.Length;
                }
                InputDirect(_buffer, 0, (int)count);
            }
        }
    }
}
