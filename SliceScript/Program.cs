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

        static bool GetWord(ref UnicodeGroupEnumerator enumerator, out Range range)
        {
            if (!enumerator.MoveNext() ||
                !enumerator.Category.IsLetter())
            {
                range = default;
                return false;
            }

            range = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (!enumerator.Category.IsLetter())
                    break;

                range = new Range(range.Start, enumerator.Current.End);
            }

            return true;
        }

        static bool GetWordWithNumbers(ref UnicodeGroupEnumerator enumerator, bool canStartWithNumber, out Range range)
        {
            if (!enumerator.MoveNext() ||
                !(enumerator.Category.IsLetter() || enumerator.Category.IsNumber()) ||
                (!canStartWithNumber && enumerator.Category.IsNumber()))
            {
                range = default;
                return false;
            }

            range = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (!enumerator.Category.IsLetterOrNumber())
                    break;

                range = new Range(range.Start, enumerator.Current.End);
            }

            return true;
        }

        static bool GetNumber(ref UnicodeGroupEnumerator enumerator, out Range range)
        {
            if (!enumerator.MoveNext() ||
                !enumerator.Category.IsNumber())
            {
                range = default;
                return false;
            }

            range = enumerator.Current;

            while (enumerator.MoveNext())
            {
                if (!enumerator.Category.IsNumber())
                    break;

                range = new Range(range.Start, enumerator.Current.End);
            }

            return true;
        }

        static void ParseScript(ReadOnlyMemory<byte> source)
        {
            UnicodeGroupEnumerator rootEnumerator = new(source.Span);

            UnicodeGroupEnumerator enumerator = rootEnumerator;



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
