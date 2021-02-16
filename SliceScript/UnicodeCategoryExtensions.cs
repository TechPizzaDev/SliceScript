using System.Globalization;

namespace SliceScript
{
    public static class UnicodeCategoryExtensions
    {
        public static bool IsLetter(this UnicodeCategory category)
        {
            return category == UnicodeCategory.LowercaseLetter
                || category == UnicodeCategory.UppercaseLetter;
        }

        public static bool IsNumber(this UnicodeCategory category)
        {
            return category == UnicodeCategory.DecimalDigitNumber;
        }

        public static bool IsLetterOrNumber(this UnicodeCategory category)
        {
            return category == UnicodeCategory.LowercaseLetter
                || category == UnicodeCategory.UppercaseLetter 
                || category == UnicodeCategory.DecimalDigitNumber;
        }

        public static bool IsWhiteSpace(this UnicodeCategory category)
        {
            return category == UnicodeCategory.SpaceSeparator;
        }

        public static bool IsOtherPunctuation(this UnicodeCategory category)
        {
            return category == UnicodeCategory.OtherPunctuation;
        }
    }
}
