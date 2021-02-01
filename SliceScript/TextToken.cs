using System.Diagnostics;
using System.Globalization;

namespace SliceScript
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct TextToken
    {
        public Utf8Memory Value { get; }
        public UnicodeCategory Category { get; }

        public TextToken(Utf8Memory value, UnicodeCategory category)
        {
            Value = value;
            Category = category;
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public override string ToString()
        {
            return "\"" + Value + "\" " + Category;
        }
    }
}
