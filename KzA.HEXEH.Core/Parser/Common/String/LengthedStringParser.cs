﻿using KzA.HEXEH.Core.Output;
using Serilog;
using System.Text;

namespace KzA.HEXEH.Core.Parser.Common.String
{
    public class LengthedStringParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;
        private static readonly int[] validLen = { 1, 2, 4, 8 };
        private int _lenOfLen;
        private int lenOfLen
        {
            get => _lenOfLen;
            set
            {
                if (!validLen.Contains(value)) throw new ArgumentException("Invalid Option: LenOfLen should be one of the following values {1,2,4,8}");
                _lenOfLen = value;
            }
        }
        private Encoding encoding = Encoding.UTF8;

        public override Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "LenOfLen", typeof(int) },
                { "Encoding", typeof(Encoding) },
            };
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, Input.Length, ParseStack);
        }
        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, Offset, Input.Length - Offset, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[LengthedStringParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                var innerParser = new LengthedObjectParser();
                var stringParserOpt = new Dictionary<string, object>()
                { {"Encoding", encoding } };
                innerParser.SetOptions(new Dictionary<string, object>()
                {
                    {"LenOfLen", lenOfLen },
                    {"ObjectParser", "Common.String.String" },
                    {"ParserOptions", stringParserOpt },
                });
                var innerResult = innerParser.Parse(Input, Offset, out Read, ParseStack);
                innerResult.Label = "String with length specified";
                innerResult.Value = innerResult.Children[1].Value;
                Log.Debug("[LengthedStringParser] Parsed {Read} bytes", Read);
                ParseStack!.PopEx();
                return innerResult;
            }
            catch (ParseException e)
            {
                throw new ParseFailureException("Failed to parse", e.ParserStackPrint, Offset, e);
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            Log.Debug("[LengthedStringParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                var innerParser = new LengthedObjectParser();
                var stringParserOpt = new Dictionary<string, object>()
                { {"Encoding", encoding } };
                innerParser.SetOptions(new Dictionary<string, object>()
                {
                    {"LenOfLen", lenOfLen },
                    {"ObjectParser", "Common.String.String" },
                    {"ParserOptions", stringParserOpt },
                });
                var innerResult = innerParser.Parse(Input, Offset, Length, ParseStack);
                innerResult.Label = "String with length specified";
                innerResult.Value = innerResult.Children[1].Value;
                Log.Debug("[LengthedStringParser] Parsed {Length} bytes", Length);
                ParseStack!.PopEx();
                return innerResult;
            }
            catch (ParseException e)
            {
                throw new ParseFailureException("Failed to parse", e.ParserStackPrint, Offset, e);
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse", ParseStack!.Dump(), Offset, e);
            }
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("LenOfLen", out var lenOfLenObj))
            {
                if (lenOfLenObj is int _lenOfLen) { lenOfLen = _lenOfLen; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfLen");
                }
            }
            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                if (encodingObj is Encoding _encoding) { encoding = _encoding; }
                else
                {
                    throw new ArgumentException("Invalid Option: Encoding");
                }
            }
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("LenOfLen", out var lenOfLenObj))
            {
                if (int.TryParse(lenOfLenObj, out var _lenOfLen)) { lenOfLen = _lenOfLen; }
                else
                {
                    throw new ArgumentException("Invalid Option: LenOfLen");
                }
            }

            if (Options.TryGetValue("Encoding", out var encodingObj))
            {
                encoding = Encoding.GetEncoding(encodingObj);
            }
            else
            {
                throw new ArgumentException("Encoding not provided");
            }
        }
    }
}