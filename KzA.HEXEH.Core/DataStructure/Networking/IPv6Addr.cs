using System.Runtime.InteropServices;
using System.Text;

namespace KzA.HEXEH.Core.DataStructure.Networking
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IPv6Addr
    {
        // UINT64 *2
        [FieldOffset(0)]
        public ulong qwAddress0;

        [FieldOffset(8)]
        public ulong qwAddress1;

        // UINT32 *4
        [FieldOffset(0)]
        public uint dwAddress0;

        [FieldOffset(4)]
        public uint dwAddress1;

        [FieldOffset(8)]
        public uint dwAddress2;

        [FieldOffset(12)]
        public uint dwAddress3;

        // UINT16 *8
        [FieldOffset(0)]
        public ushort wAddress0;

        [FieldOffset(2)]
        public ushort wAddress1;

        [FieldOffset(4)]
        public ushort wAddress2;

        [FieldOffset(6)]
        public ushort wAddress3;

        [FieldOffset(8)]
        public ushort wAddress4;

        [FieldOffset(10)]
        public ushort wAddress5;

        [FieldOffset(12)]
        public ushort wAddress6;

        [FieldOffset(14)]
        public ushort wAddress7;

        public ushort[] Words
        {
            get
            {
                return new ushort[] { wAddress0, wAddress1, wAddress2, wAddress3, wAddress4, wAddress5, wAddress6, wAddress7 };
            }
            set
            {
                wAddress0 = value[0];
                wAddress1 = value[1];
                wAddress2 = value[2];
                wAddress3 = value[3];
                wAddress4 = value[4];
                wAddress5 = value[5];
                wAddress6 = value[6];
                wAddress7 = value[7];
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new();
            int longestLen = 0;
            int longestStartIdx = -1;
            int len = 0;
            int startIdx = -1;
            bool cnting = false;
            for (int i = 0; i < Words.Length; i++)
            {
                if (Words[i] == 0)
                {
                    if (cnting)
                    {
                        len++;
                        if (i == Words.Length - 1 && len > 1 && len > longestLen)
                        {
                            longestLen = len;
                            longestStartIdx = startIdx;
                        }
                    }
                    else
                    {
                        cnting = true;
                        len = 1;
                        startIdx = i;
                    }
                }
                else
                {
                    if (cnting)
                    {
                        cnting = false;
                        if (len > 1 && len > longestLen)
                        {
                            longestLen = len;
                            longestStartIdx = startIdx;
                        }
                    }
                }
            }
            for (int i = 0; i < Words.Length; i++)
            {
                if (i == longestStartIdx) sb.Append("::");
                else if (i > longestStartIdx && i < longestStartIdx + longestLen) continue;
                else { sb.Append(Words[i].ToString("X")); if (i != Words.Length - 1 && i != longestStartIdx - 1) sb.Append(':'); }
            }
            return sb.ToString();
        }
    }
}
