#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MocopiSender
{
    public record MultipleValues(IEnumerable<IBinarySerializable> values) : IBinarySerializable
    {
        public byte[] ToBytes() => values.SelectMany(value => value.ToBytes())
                                        .ToArray();
    }

    public record UintValue_1Byte(byte value) : IBinarySerializable
    {
        public byte[] ToBytes() => new [] { value };
    }

    public record UintValue_2Byte(ushort value) : IBinarySerializable
    {
        public byte[] ToBytes() => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(value));
    }

    public record UintValue_4Byte(uint value) : IBinarySerializable
    {
        public byte[] ToBytes() => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(value));
    }

    public record UintValue_8Byte(ulong value) : IBinarySerializable
    {
        public byte[] ToBytes() => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(value));
    }

    public record IpAddressValue(IEnumerable<ushort> ipAddress) : IBinarySerializable
    {
        public byte[] ToBytes() => ipAddress.SelectMany(octet => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(octet)))
                                            .ToArray();
    }

    public record StringValue(string value) : IBinarySerializable
    {
        public byte[] ToBytes() => Encoding.ASCII.GetBytes(value);
    }

    public record FloatArrayValue(IEnumerable<float> values) : IBinarySerializable
    {
        public byte[] ToBytes() => values.SelectMany(value => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(value)))
                                            .ToArray();
    }

    public record FloatValue(float value) : IBinarySerializable
    {
        public byte[] ToBytes() => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(value));
    }

    public record DoubleValue(double value) : IBinarySerializable
    {
        public byte[] ToBytes() => EndianConverter.ConvertToLittleEndianBytes(BitConverter.GetBytes(value));
    }
}