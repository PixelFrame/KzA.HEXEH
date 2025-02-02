﻿using KzA.HEXEH.Base.Output;
using KzA.HEXEH.Base.Parser;
using KzA.HEXEH.Core.Utility;
using Serilog;
using System.Buffers.Binary;
using System.Text.Json;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class ArrayParser : ParserBase
    {
        public override ParserType Type => ParserType.Internal;
        private int objectCount;
        private bool includeSchema = false;
        private IParser? nextParser;
        private ulong terminator = 0;
        private int terminatorLength = 1;

        public override Dictionary<string, Type> GetOptions()
        {
            return new Dictionary<string, Type>()
            {
                { "ObjectCount?", typeof(int) },
                { "Terminator?", typeof(ulong) },
                { "TerminatorLength?", typeof(int) },
                { "ObjectParser", typeof(string) },
                { "IncludeSchema?", typeof(bool) },
                { "ParserOptions?", typeof(Dictionary<string, object>) }
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
            Log.Debug("[ArrayParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                if (nextParser == null) { throw new InvalidOperationException("ObjectType not set"); }

                var head = new DataNode()
                {
                    Label = "Array of objects with length inherited",
                    Index = Offset,
                };
                var start = Offset;

                if (objectCount > 0)
                {
                    for (var i = 0; i < objectCount; i++)
                    {
                        head.Children.Add(nextParser.Parse(Input, Offset, out int currentObjLen, ParseStack));
                        Offset += currentObjLen;
                    }
                }
                if (objectCount < 0)
                {
                    while (true)
                    {
                        head.Children.Add(nextParser.Parse(Input, Offset, out int currentObjLen, ParseStack));
                        Offset += currentObjLen;
                        if (Offset == Input.Length) break;
                        if (ValidateTerminator(Input, Offset)) break;
                        if (Offset > Input.Length) throw new ParseFailureException("Array exceeds data boundry", ParseStack.Dump(), Offset - currentObjLen, null);
                    }
                }
                Read = Offset - start;
                head.Length = Read;
                Log.Debug("[ArrayParser] Parsed {Read} bytes", Read);
                ParseStack!.PopEx();
                return head;
            }
            catch (ParseException e)
            {
                throw new ParseFailureException("Failed to parse inner object", e.ParserStackPrint, Offset, e);
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Failed to parse inner object", ParseStack!.Dump(), Offset, e);
            }
        }

        private bool ValidateTerminator(in ReadOnlySpan<byte> Input, int Offset)
        {
            ulong nextToken = 0;
            switch (terminatorLength)
            {
                case 1: nextToken = Input[Offset]; break;
                case 2: nextToken = BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(Input.Slice(Offset, 2)) : BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                case 4: nextToken = BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(Input.Slice(Offset, 4)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Offset, 4)); break;
                case 8: nextToken = BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(Input.Slice(Offset, 8)) : BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Offset, 8)); break;
            }
            return nextToken == terminator;
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
            if (objectCount < 0)
            {
                Log.Debug("[ArrayParser] Start parsing from {Offset}", Offset);
                ParseStack = PrepareParseStack(ParseStack);
                try
                {
                    if (nextParser == null) { throw new InvalidOperationException("ObjectType not set"); }

                    var head = new DataNode()
                    {
                        Label = "Array of objects with length inherited",
                        Index = Offset,
                    };
                    var end = Offset + Length;
                    while (true)
                    {
                        head.Children.Add(nextParser.Parse(Input, Offset, out int currentObjLen, ParseStack));
                        Offset += currentObjLen;
                        if (Offset == end) break;
                        if (Offset > end) throw new ParseFailureException("Array exceeds data boundry", ParseStack.Dump(), Offset - currentObjLen, null);
                    }
                    head.Length = Length;
                    Log.Debug("[ArrayParser] Parsed {Read} bytes", Length);
                    ParseStack!.PopEx();
                    return head;
                }
                catch (ParseException e)
                {
                    throw new ParseFailureException("Failed to parse inner object", e.ParserStackPrint, Offset, e);
                }
                catch (Exception e)
                {
                    throw new ParseFailureException("Failed to parse inner object", ParseStack!.Dump(), Offset, e);
                }
            }

            var res = Parse(in Input, Offset, out int read, ParseStack);
            if (read < Length)
            {
                var paddingNode = new DataNode()
                {
                    Label = "Padding (Unread Bytes)",
                    Value = BitConverter.ToString(Input.Slice(Offset + read, Length - read).ToArray()),
                    Index = Offset + read,
                    Length = Length - read,
                };
                res.Length = Length;
                res.Children.Add(paddingNode);
            }
            if (read > Length)
            {
                Log.Error("[ArrayParser] Actual object array length exceeding given length");
                ParseStack!.Push(GetType().FullName ?? GetType().Name);
                throw new ParseLengthMismatchException("Actual object array length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("IncludeSchema", out var includeSchemaObj))
            {
                if (includeSchemaObj is bool _includeSchemaSchema)
                {
                    includeSchema = _includeSchemaSchema;
                    Log.Debug("[ArrayParser] Set option IncludeSchema to {includeSchema}", includeSchema);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: IncludeSchema");
                }
            }

            if (Options.TryGetValue("ObjectParser", out var targetTypeNameObj))
            {
                if (targetTypeNameObj is string targetTypeName)
                {
                    nextParser = ParserManager.InstantiateParserByRelativeName(targetTypeName, includeSchema);
                    Log.Debug("[ArrayParser] Set option ObjectParser to {targetTypeName}", targetTypeName);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectParser");
                }
            }
            else
            {
                throw new ArgumentException("ObjectParser not provided");
            }

            if (Options.TryGetValue("ObjectCount", out var objectCountObj))
            {
                if (objectCountObj is int _objectCount)
                {
                    objectCount = _objectCount;
                    Log.Debug("[ArrayParser] Set option ObjectCount to {objectCount}", objectCount);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectCount");
                }
            }
            else
            {
                objectCount = -1;
            }

            if (Options.TryGetValue("Terminator", out var terminatorObj))
            {
                if (terminatorObj is ulong _terminatorObj)
                {
                    terminator = _terminatorObj;
                    Log.Debug("[ArrayParser] Set option Terminator to {terminator}", terminator);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: Terminator");
                }
            }
            else
            {
                terminator = 0;
            }

            if (Options.TryGetValue("TerminatorLength", out var terminatorLenObj))
            {
                if (terminatorLenObj is int _terminatorLenObj)
                {
                    terminatorLength = _terminatorLenObj;
                    Log.Debug("[ArrayParser] Set option TerminatorLength to {terminatorLength}", terminatorLength);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: TerminatorLength");
                }
            }
            else
            {
                terminatorLength = 1;
            }

            if (Options.TryGetValue("ParserOptions", out var nextParserOptionsObj))
            {
                if (nextParserOptionsObj is Dictionary<string, object> nextParserOptions)
                {
                    nextParser.SetOptions(nextParserOptions);
                    Log.Debug("[ArrayParser] Set option ParserOptions to {nextParserOptions}", nextParserOptions);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ParserOptions");
                }
            }
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("IncludeSchema", out var isSchemaStr))
            {
                if (isSchemaStr.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    includeSchema = true;
                    Log.Debug("[ArrayParser] Set option IncludeSchema to {includeSchema}", includeSchema);
                }
            }

            if (Options.TryGetValue("ObjectParser", out var targetTypeName))
            {
                nextParser = ParserManager.InstantiateParserByRelativeName(targetTypeName, includeSchema);
                Log.Debug("[ArrayParser] Set option ObjectParser to {targetTypeName}", targetTypeName);
            }
            else
            {
                throw new ArgumentException("ObjectParser not provided");
            }

            if (Options.TryGetValue("ObjectCount", out var objectCountStr))
            {
                if (int.TryParse(objectCountStr, out var _objectCount))
                {
                    objectCount = _objectCount;
                    Log.Debug("[ArrayParser] Set option ObjectCount to {objectCount}", objectCount);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ObjectCount");
                }
            }
            else
            {
                objectCount = -1;
            }

            if (Options.TryGetValue("Terminator", out var terminatorObj))
            {
                if (ulong.TryParse(terminatorObj, out var _terminatorObj))
                {
                    terminator = _terminatorObj;
                    Log.Debug("[ArrayParser] Set option Terminator to {terminator}", terminator);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: Terminator");
                }
            }
            else
            {
                terminator = 0;
            }

            if (Options.TryGetValue("TerminatorLength", out var terminatorLenObj))
            {
                if (int.TryParse(terminatorLenObj, out var _terminatorLenObj))
                {
                    terminatorLength = _terminatorLenObj;
                    Log.Debug("[ArrayParser] Set option TerminatorLength to {terminatorLength}", terminatorLength);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: TerminatorLength");
                }
            }
            else
            {
                terminatorLength = 1;
            }

            if (Options.TryGetValue("ParserOptions", out var nextParserOptionsStr))
            {
                var nextParserOptions = JsonSerializer.Deserialize<Dictionary<string, string>>(nextParserOptionsStr);
                if (nextParserOptions != null)
                {
                    nextParser.SetOptionsFromSchema(nextParserOptions);
                    Log.Debug("[ArrayParser] Set option ParserOptions to {nextParserOptions}", nextParserOptions);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: ParserOptions");
                }
            }
        }
    }
}
