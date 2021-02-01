using System;
using System.Buffers;
using System.Globalization;
using System.Text;

namespace SliceScript
{
    public ref struct UnicodeGroupEnumerator
    {
        private int _offset;

        public ReadOnlySpan<byte> Span { get; }

        public Range Current { get; private set; }
        public UnicodeCategory Category { get; private set; }

        public UnicodeGroupEnumerator(ReadOnlySpan<byte> span) : this()
        {
            Span = span;
        }

        public bool MoveNext()
        {
            int start = _offset;
            
            UnicodeCategory? lastCategory = default;
            OperationStatus status;
            while ((status = Rune.DecodeFromUtf8(Span[_offset..], out Rune rune, out int consumed)) == OperationStatus.Done)
            {
                UnicodeCategory category = Rune.GetUnicodeCategory(rune);
                if (lastCategory.HasValue && category != lastCategory)
                {
                    Current = new Range(start, _offset);
                    Category = lastCategory.GetValueOrDefault();
                    return true;
                }

                lastCategory = category;
                _offset += consumed;
            }

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

        public UnicodeGroupEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
