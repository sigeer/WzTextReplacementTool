using System;
using System.Linq;
using System.Text;

namespace MapleLib.Helpers
{
    public static class HexTool
    {
        private static char[] HEX = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

        /// <summary>
        /// Converts a byte value to readable hex representation
        /// </summary>
        /// <param name="byteValue"></param>
        /// <returns></returns>
        public static String ToString(byte byteValue)
        {
            int tmp = byteValue << 8;
            char[] retstr = new char[] { HEX[(tmp >> 12) & 0x0F], HEX[(tmp >> 8) & 0x0F] };
            return new string(retstr);
        }

        /// <summary>
        /// Converts an array of bytes to readable hex representation
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static String ToString(byte[] bytes)
        {
            StringBuilder hexed = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                hexed.Append(ToString(bytes[i]));
                hexed.Append(' ');
            }
            return hexed.ToString();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 3);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2} ", b);
            return hex.ToString();
        }
    }
}