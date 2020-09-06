// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Runtime.CompilerServices;

namespace MDSDK.JPEG2000.Utils
{
    abstract class Signal2D
    {
        public Range URange { get; }

        public Range VRange { get; }
        
        protected Signal2D(Range uRange, Range vRange)
        {
            URange = uRange;
            VRange = vRange;
        }

        public abstract Signal2D Create(Range uRange, Range vRange);
    }

    class Signal2D<T> : Signal2D
    {
        private readonly T[] _samples;

        public Signal2D(Range uRange, Range vRange, T[] samples)
            : base(uRange, vRange)
        {
            _samples = samples;
        }

        public Signal2D(Range uRange, Range vRange)
            : this(uRange, vRange, new T[uRange.Length * vRange.Length])
        {
        }

        public override Signal2D Create(Range uRange, Range vRange)
        {
            return new Signal2D<T>(uRange, vRange);
        }

        public T this[int u, int v]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (!URange.Contains(u, out int uOffset))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(u));
                }
                if (!VRange.Contains(v, out int vOffset))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(v));
                }
                return _samples[vOffset * URange.Length + uOffset];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                if (!URange.Contains(u, out int uOffset))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(u));
                }
                if (!VRange.Contains(v, out int vOffset))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(v));
                }
                _samples[vOffset * URange.Length + uOffset] = value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetColumn(int u, Signal1D<T> column)
        {
            VRange.GetBounds(out int v0, out int v1);

            for (var v = v0; v < v1; v++)
            {
                column[v] = this[u, v];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetColumn(int u, Signal1D<T> column)
        {
            VRange.GetBounds(out int v0, out int v1);

            for (var v = v0; v < v1; v++)
            {
                this[u, v] = column[v];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void GetRow(int v, Signal1D<T> row)
        {
            URange.GetBounds(out int u0, out int u1);

            for (var u = u0; u < u1; u++)
            {
                row[u] = this[u, v];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetRow(int v, Signal1D<T> row)
        {
            URange.GetBounds(out int u0, out int u1);

            for (var u = u0; u < u1; u++)
            {
                this[u, v] = row[u];
            }
        }

        public void Iterate(System.Action<int, int, T> callback)
        {
            URange.GetBounds(out int u0, out int u1);
            VRange.GetBounds(out int v0, out int v1);

            for (var v = v0; v < v1; v++)
            {
                for (var u = u0; u < u1; u++)
                {
                    callback(u, v, this[u, v]);
                }
            }
        }
    }
}
