// Copyright (c) Robin Boerdijk - All rights reserved - See LICENSE file for license terms

using System.Runtime.CompilerServices;
using static MDSDK.JPEG2000.Utils.StaticInclude;

namespace MDSDK.JPEG2000.Utils
{
    class Signal1D<T>
    {
        public Range Range { get; }
        
        private readonly T[] _samples;

        public Signal1D(Range range, T[] samples)
        {
            Range = range;
            _samples = samples;
        }

        public Signal1D(Range range)
            : this(range, new T[range.Length])
        {
        }

        public T this[int i]
        {
            [MethodImpl(HotspotMethodImplOptions)]
            get
            {
                if (!Range.Contains(i, out int offsetInRange))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(i));
                }
                return _samples[offsetInRange];
            }
            [MethodImpl(HotspotMethodImplOptions)]
            set
            {
                if (!Range.Contains(i, out int offsetInRange))
                {
                    throw new System.ArgumentOutOfRangeException(nameof(i));
                }
                _samples[offsetInRange] = value;
            }
        }

        public override string ToString()
        {
            return $"{typeof(T).Name}[{Range.Start}..{Range.End}) {{ {string.Join(", ", _samples)} }}";
        }

        [MethodImpl(HotspotMethodImplOptions)]
        public static void ExtendPeriodicSymmetrically(Range range, Signal1D<T> yExt)
        {
            range.GetBounds(out int i0, out int i1);
            
            yExt.Range.GetBounds(out int j0, out int j1);

            int i = i0;

            for (var j = i0 - 1; j >= j0; j--)
            {
                i++;
                yExt[j] = yExt[i];
            }

            i = i1 - 1;

            for (var j = i1; j < j1; j++)
            {
                i--;
                yExt[j] = yExt[i];
            }
        }
    }
}
