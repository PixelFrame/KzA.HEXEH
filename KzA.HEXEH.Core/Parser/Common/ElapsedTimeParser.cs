using KzA.HEXEH.Core.Output;
using Serilog;
using System.Buffers.Binary;

namespace KzA.HEXEH.Core.Parser.Common
{
    public class ElapsedTimeParser : ParserBase
    {
        public override ParserType Type => ParserType.Hardcoded;

        private DateTime startTime = DateTime.UnixEpoch;
        private readonly string[] VALID_UNITS = ["d", "h", "m", "s", "us", "ms", "t"];
        private string unit = "t";
        private int _length;
        private int length
        {
            get => _length;
            set
            {
                if (!Global.ValidNumberLen.Contains(value)) throw new ArgumentException("Invalid Option: Length should be one of the following values {1,2,4,8}");
                _length = value;
            }
        }

        public override Dictionary<string, Type> GetOptions()
        {
            return new()
            {
                { "StartTime", typeof(DateTime) },
                { "Unit", typeof(string) },
                { "Length", typeof(int) },
            };
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, out int Read, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out Read, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, Stack<string>? ParseStack = null)
        {
            return Parse(Input, 0, out _, ParseStack);
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read, Stack<string>? ParseStack = null)
        {
            Log.Debug("[ElapsedTimeParser] Start parsing from {Offset}", Offset);
            ParseStack = PrepareParseStack(ParseStack);
            try
            {
                long value = 0;
                switch (length)
                {
                    case 1: value = Input[Offset]; break;
                    case 2: value = BinaryPrimitives.ReadUInt16LittleEndian(Input.Slice(Offset, 2)); break;
                    case 4: value = BinaryPrimitives.ReadUInt32LittleEndian(Input.Slice(Offset, 4)); break;
                    case 8: value = BinaryPrimitives.ReadInt64LittleEndian(Input.Slice(Offset, 8)); break;
                }

                TimeSpan ts = TimeSpan.Zero;
                switch(unit)
                {
                    case "t": ts = TimeSpan.FromTicks(value); break;
                    case "us": ts = TimeSpan.FromMicroseconds(value); break;
                    case "ms": ts = TimeSpan.FromMilliseconds(value); break;
                    case "s": ts = TimeSpan.FromSeconds(value); break;
                    case "m": ts = TimeSpan.FromMinutes(value); break;
                    case "h": ts = TimeSpan.FromHours(value); break;
                    case "d": ts = TimeSpan.FromDays(value); break;
                }

                var t = startTime.Add(ts);

                Log.Debug($"[ElapsedTimeParser] Parsed {length} bytes");
                Read = length;
                ParseStack!.PopEx();
                return new DataNode()
                {
                    Label = "Time",
                    Value = t.ToString(),
                    Index = Offset,
                    Length = length,
                };
            }
            catch (Exception e)
            {
                throw new ParseFailureException("Unable to parse the data to time", ParseStack!.Dump(), Offset, e);
            }
        }

        public override DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length, Stack<string>? ParseStack = null)
        {
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
                Log.Error("[ElapsedTimeParser] Actual object length exceeding given length");
                ParseStack!.Push(GetType().FullName ?? GetType().Name);
                throw new ParseLengthMismatchException("Actual object length exceeding given length", ParseStack!.Dump(), Offset, null);
            }
            return res;
        }

        public override void SetOptions(Dictionary<string, object> Options)
        {
            if (Options.TryGetValue("StartTime", out var startTimeObj))
            {
                if (startTimeObj is DateTime _starTime)
                {
                    startTime = _starTime;
                    Log.Debug("[ElapsedTimeParser] Set option StartTime to {startTime}", startTime);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: StartTime");
                }
            }

            if (Options.TryGetValue("Unit", out var unitObj))
            {
                if (unitObj is string _unitStr)
                {
                    if (VALID_UNITS.Contains(_unitStr))
                    {
                        unit = _unitStr;
                        Log.Debug("[ElapsedTimeParser] Set option Unit to {unit}", unit);
                    }
                    else
                    {
                        throw new ArgumentException($"{_unitStr} is not a valid unit");
                    }
                }
                else
                {
                    throw new ArgumentException("Invalid Option: Unit");
                }
            }

            if (Options.TryGetValue("Length", out var lengthObj))
            {
                if (lengthObj is int _length)
                {
                    length = _length;
                    Log.Debug("[ElapsedTimeParser] Set option Length to {length}", length);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: Length");
                }
            }
            else
            {
                throw new ArgumentException("Length not provided");
            }
        }

        public override void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            if (Options.TryGetValue("StartTime", out var startTimeStr))
            {
                if (DateTime.TryParse(startTimeStr, out startTime))
                {
                    Log.Debug("[ElapsedTimeParser] Set option StartTime to {startTime}", startTime);
                }
                else
                {
                    throw new ArgumentException($"{startTimeStr} cannot be parsed as a DateTime");
                }
            }

            if (Options.TryGetValue("Unit", out var _unitStr))
            {
                if (VALID_UNITS.Contains(_unitStr))
                {
                    unit = _unitStr;
                    Log.Debug("[ElapsedTimeParser] Set option Unit to {unit}", unit);
                }
                else
                {
                    throw new ArgumentException($"{_unitStr} is not a valid unit");
                }
            }

            if (Options.TryGetValue("Length", out var lengthStr))
            {
                if (int.TryParse(lengthStr, out var _length))
                {
                    length = _length;
                    Log.Debug("[ElapsedTimeParser] Set option Length to {length}", length);
                }
                else
                {
                    throw new ArgumentException("Invalid Option: Length");
                }
            }
            else
            {
                throw new ArgumentException("Length not provided");
            }
        }
    }
}
