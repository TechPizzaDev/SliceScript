using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;

namespace SliceScript
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding encoding = new UTF8Encoding(false, false);
            string source16 = File.ReadAllText("Script1.sscript");
            byte[] source8 = encoding.GetBytes(source16);
            ParseScript(source8);
        }

        private static Dictionary<ScriptScope, ImmutableArray<Utf8Memory>> ScopeKeywords;
        private static ImmutableArray<string> AccessKeywords;
        private static ImmutableArray<string> ModifierKeywords;

        static Program()
        {
            ScopeKeywords = new Dictionary<ScriptScope, ImmutableArray<Utf8Memory>>();
            ScopeKeywords.Add(ScriptScope.Namespace, ImmutableArray.Create(new Utf8Memory[] { new("using"), new("class"), new("struct") }));
            ScopeKeywords.Add(ScriptScope.Type, ImmutableArray.Create(new Utf8Memory[] { new() }));
            ScopeKeywords.Add(ScriptScope.Method, ImmutableArray.Create(new Utf8Memory[] { new() }));

            AccessKeywords = ImmutableArray.Create("public", "protected", "private", "internal");
            ModifierKeywords = ImmutableArray.Create("virtual", "override", "readonly");
        }

        public enum ScriptScope
        {
            Namespace,
            Type,
            Member,
            Method,
        }

        class ParseScope
        {
            public List<SyntaxNode> Nodes { get; }
            public ReadOnlyMemory<char> Name { get; set; }
            public List<SyntaxToken> Tokens { get; }

            public ParseScope()
            {
                Nodes = new List<SyntaxNode>();
                Tokens = new List<SyntaxToken>();
            }
        }

        delegate bool GetPredicate<T>(ref UnicodeGroupIterator iterator, ref T state);
        delegate bool GetPredicate(ref UnicodeGroupIterator iterator);

        static bool Get<T>(
            ref UnicodeGroupIterator iterator,
            GetPredicate<T> startPredicate,
            GetPredicate<T> restPredicate,
            ref T state,
            out Range range)
        {
            if (!iterator.MoveNext(out int consumed) ||
                !startPredicate.Invoke(ref iterator, ref state))
            {
                range = default;
                return false;
            }

            iterator.Move(consumed);
            range = iterator.Current;

            while (iterator.MoveNext(out consumed))
            {
                if (!restPredicate(ref iterator, ref state))
                    break;

                iterator.Move(consumed);
                range = new Range(range.Start, iterator.Current.End);
            }

            return true;
        }

        static bool Get(
            ref UnicodeGroupIterator iterator,
            GetPredicate startPredicate,
            GetPredicate restPredicate,
            out Range range)
        {
            if (!iterator.MoveNext(out int consumed) ||
                !startPredicate.Invoke(ref iterator))
            {
                range = default;
                return false;
            }

            iterator.Move(consumed);
            range = iterator.Current;

            while (iterator.MoveNext(out consumed))
            {
                if (!restPredicate(ref iterator))
                    break;

                iterator.Move(consumed);
                range = new Range(range.Start, iterator.Current.End);
            }

            return true;
        }

        static bool GetWord(ref UnicodeGroupIterator iterator, out Range range)
        {
            static bool predicate(ref UnicodeGroupIterator iterator) => iterator.Category.IsLetter();
            return Get(ref iterator, predicate, predicate, out range);
        }

        static bool GetWordWithNumbers(ref UnicodeGroupIterator iterator, bool canStartWithNumber, out Range range)
        {
            static bool startPredicate(ref UnicodeGroupIterator iterator, ref bool canStartWithNumber)
            {
                return !iterator.Category.IsLetterOrNumber()
                    || (!canStartWithNumber && iterator.Category.IsNumber());
            }
            static bool restPredicate(ref UnicodeGroupIterator iterator, ref bool canStartWithNumber)
            {
                return iterator.Category.IsLetter();
            }
            return Get(ref iterator, startPredicate, restPredicate, ref canStartWithNumber, out range);
        }

        static bool GetNumber(ref UnicodeGroupIterator iterator, out Range range)
        {
            static bool predicate(ref UnicodeGroupIterator iterator) => iterator.Category.IsNumber();
            return Get(ref iterator, predicate, predicate, out range);
        }

        static bool GetSpaceSeparator(ref UnicodeGroupIterator iterator, out Range range)
        {
            static bool predicate(ref UnicodeGroupIterator iterator) => iterator.Category.IsWhiteSpace();
            return Get(ref iterator, predicate, predicate, out range);
        }

        static bool GetOtherPunctuation(ref UnicodeGroupIterator iterator, out Range range)
        {
            static bool predicate(ref UnicodeGroupIterator iterator) => iterator.Category.IsOtherPunctuation();
            return Get(ref iterator, predicate, predicate, out range);
        }

        static Range? GetWord(ref UnicodeGroupIterator iterator)
        {
            if (GetWord(ref iterator, out Range range))
                return range;
            return null;
        }

        static void ParseScript(ReadOnlyMemory<byte> source)
        {
            UnicodeGroupIterator rootIterator = new(source.Span);

            UnicodeGroupIterator iterator = rootIterator;

            GetSpaceSeparator(ref iterator, out Range firstSpace);
            if (GetWord(ref iterator, out Range firstWord))
            {
                if (GetSpaceSeparator(ref iterator, out Range secondSpace))
                {
                    if (GetWord(ref iterator, out Range secondWord))
                    {
                        GetSpaceSeparator(ref iterator, out Range thirdSpace);
                        GetOtherPunctuation(ref iterator, out Range firstPunctuation);

                        Console.WriteLine("\"" + Encoding.UTF8.GetString(source[firstSpace].Span) + "\"");
                        Console.WriteLine("\"" + Encoding.UTF8.GetString(source[firstWord].Span) + "\"");
                        Console.WriteLine("\"" + Encoding.UTF8.GetString(source[secondSpace].Span) + "\"");
                        Console.WriteLine("\"" + Encoding.UTF8.GetString(source[secondWord].Span) + "\"");
                        Console.WriteLine("\"" + Encoding.UTF8.GetString(source[thirdSpace].Span) + "\"");
                        Console.WriteLine("\"" + Encoding.UTF8.GetString(source[firstPunctuation].Span) + "\"");
                    }
                }
            }

            //Stack<ParseScope> scopeStack = new();
            //
            //ParseScope rootScope = new();
            //scopeStack.Push(rootScope);
            //
            //ScriptScope scope = ScriptScope.Namespace;
            //
            //EnterScope:
            //ParseScope parseScope = scopeStack.Peek();
            //ImmutableArray<Utf8String> scopeKeywords = ScopeKeywords[scope];
            //ReadOnlySpan<byte> sourceSpan = source.Span;
            //
            //switch (scope)
            //{
            //    case ScriptScope.Namespace:
            //        foreach (Utf8String scopeKeyword in scopeKeywords)
            //        {
            //            if (sourceSpan.StartsWith(scopeKeyword))
            //            {
            //                scopeStack.Push(new ParseScope());
            //                goto EnterScope;
            //            }
            //        }
            //        break;
            //
            //    case ScriptScope.Type:
            //
            //        break;
            //
            //    case ScriptScope.Member:
            //
            //        break;
            //
            //    case ScriptScope.Method:
            //
            //        break;
            //}
        }

        public readonly struct SyntaxNode
        {
            public Utf8Memory Name { get; }
            public SyntaxNode[] Children { get; }
            public SyntaxToken[] Tokens { get; }

            public SyntaxNode(Utf8Memory name, SyntaxNode[] children, SyntaxToken[] tokens)
            {
                Name = name;
                Children = children ?? throw new ArgumentNullException(nameof(children));
                Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            }
        }

        public readonly struct SyntaxToken
        {
            public Utf8Memory Value { get; }

            public SyntaxToken(Utf8Memory value)
            {
                Value = value;
            }
        }

        public readonly struct SyntaxTrivia
        {

        }
    }
}
