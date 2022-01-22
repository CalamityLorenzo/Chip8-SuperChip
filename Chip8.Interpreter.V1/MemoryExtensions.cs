using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chip8.Interpreter.V1
{
    internal static class MemoryExtensions
    {
        // Memory is a byte array
        // instructions are 2 bytes...(16 bit);
        public static ushort ReadOpcode(this byte[] @this, int Startposition)
        {
            return (ushort)(@this[Startposition] << 8 | @this[Startposition + 1]);
        }

        public static byte[] Deconstruct(this ushort @this)
        {
            throw new NotFiniteNumberException();
        }

        /// <summary>
        /// The top nibble of the instruction
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static byte GetOperation(this ushort @this)
        {
            return (byte)(@this >> 12);
        }

        public static ushort Get12BitNumber(this ushort @this)
        {
            // All three nibbles accounted for
            return (ushort)(@this & 0x0FFF);
        }
        public static byte GetTop8BitNumber(this ushort @this)
        {
            // All three nibbles accounted for
            return (byte)(@this & 0x0FF0);
        }

        public static byte GetBottom8BitNumber(this ushort @this)
        {
            // All three nibbles accounted for
            return (byte)(@this & 0x00FF);
        }

        public static byte GetXRegister(this ushort @this)
        {
            // All three nibbles accounted for
            return (byte)((@this & 0x0F00) >>8);
        }

        public static byte GetYRegister(this ushort @this)
        {
            // All three nibbles accounted for
            return (byte)((@this & 0x00F0) >> 4);
        }

        public static byte GetLastNibble(this ushort @this)
        {
            return (byte)(@this & 0xF);
        }
    }
}
