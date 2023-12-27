using KzA.HEXEH.Core.Output;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KzA.HEXEH.Core.Parser.Windows
{
    internal class FILETIMEParser : IParser
    {
        public ParserType Type => ParserType.Hardcoded;

        public Dictionary<string, Type> GetOptions()
        {
            return [];
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input)
        {
            return Parse(Input, 0);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, out int Read)
        {
            Read = 8;
            return Parse(Input, 0);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset)
        {
            var filetime = BinaryPrimitives.ReadInt64LittleEndian(Input.Slice(Offset,8));
            var datetime = DateTime.FromFileTime(filetime);
            return new DataNode()
            {
                Label = "FILETIME",
                Value = datetime.ToString()
            };
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, out int Read)
        {
            Read = 8;
            return Parse(Input, Offset);
        }

        public DataNode Parse(in ReadOnlySpan<byte> Input, int Offset, int Length)
        {
            if (Length != 8) throw new ArgumentException("FILETIME length must be 8");
            return Parse(Input, Offset);
        }

        public void SetOptions(Dictionary<string, object> Options)
        {
            throw new NotImplementedException();
        }

        public void SetOptionsFromSchema(Dictionary<string, string> Options)
        {
            throw new NotImplementedException();
        }
    }
}
