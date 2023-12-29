#nullable enable

using System;

namespace MocopiSender
{
    public static class EndianConverter
    {
        public static byte[] ConvertToLittleEndianBytes(byte[] bytes)
        {
            if(BitConverter.IsLittleEndian)
            {
                return bytes;
            }

            Array.Reverse(bytes);
            return bytes;
        }
    }
}