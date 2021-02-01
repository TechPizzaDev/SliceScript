using System;
using System.Diagnostics;
using System.Text;

namespace SliceScript
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct Utf8Memory
    {
        public static Encoding Utf8 { get; } = new UTF8Encoding(false, false);

        public ReadOnlyMemory<byte> Value { get; }

        public ReadOnlySpan<byte> Span => Value.Span;

        public Utf8Memory(ReadOnlyMemory<byte> value)
        {
            Value = value;
        }

        public Utf8Memory(string value)
        {
            Value = value != null ? Utf8.GetBytes(value) : default;
        }

        public override string ToString()
        {
            return Utf8.GetString(Span);
        }

        public static implicit operator ReadOnlySpan<byte>(Utf8Memory value)
        {
            return value.Span;
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
