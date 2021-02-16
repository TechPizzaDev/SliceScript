using System;
using System.Buffers;
using System.Globalization;
using System.Text;

namespace SliceScript
{
    public ref struct UnicodeGroupIterator
    {
        public ReadOnlySpan<byte> Span { get; }

        public Range Current { get; private set; }
        public int Offset { get; private set; }
        public UnicodeCategory Category { get; private set; }
        
        public UnicodeGroupIterator(ReadOnlySpan<byte> span) : this()
        {
            Span = span;
        }

        public void Move(int count)
        {
            Offset += count;
        }

        public bool MoveNext(out int consumed)
        {
            int index = Offset;

            UnicodeCategory? lastCategory = default;
            OperationStatus status;
            while ((status = Rune.DecodeFromUtf8(Span[index..], out Rune rune, out int consumedUtf8)) == OperationStatus.Done)
            {
                UnicodeCategory category = Rune.GetUnicodeCategory(rune);
                if (lastCategory.HasValue && category != lastCategory)
                {
                    Current = new Range(Offset, index);
                    Category = lastCategory.GetValueOrDefault();
                    consumed = index - Offset;
                    return true;
                }

                lastCategory = category;
                index += consumedUtf8;
            }

            consumed = 0;
            return false;

            //int next;
            //if ((next = _span.IndexOfAny(_triviaChars)) != -1)
            //{
            //    _span = _span[next..];
            //
            //    if (next > 0)
            //    {
            //        Current = _memory.Slice(0, next);
            //        _memory = _memory[next..];
            //        return true;
            //    }
            //
            //    int triviaLength = 0;
            //    while (_span.IndexOfAny(_triviaChars) == 0)
            //    {
            //        triviaLength++;
            //        _span = _span[1..];
            //    }
            //
            //    Current = _memory.Slice(next, triviaLength);
            //    _memory = _memory[(next + triviaLength)..];
            //    return true;
            //}
            //
            //if (_memory.Length > 0)
            //{
            //    Current = _memory;
            //    _memory = _memory[_memory.Length..];
            //    return true;
            //}
            //
            //return false;
        }

        public UnicodeGroupIterator GetEnumerator()
        {
            return this;
        }
    }
}
