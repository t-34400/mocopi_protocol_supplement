#nullable enable

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace MocopiSender
{
    public class PacketField : IBinarySerializable
    {
        private const int NAME_LENGTH_BINARY_DATA_LENGTH = 8;
        private readonly string fieldName;
        private readonly IBinarySerializable value;

        public PacketField(string fieldName, IBinarySerializable value)
        {
            this.fieldName = fieldName;
            this.value = value;
        }
        public PacketField(string fieldName, IEnumerable<IBinarySerializable> values) : this(fieldName, new MultipleValues(values)) {}

        public byte[] ToBytes()
        {
            var fieldNameBytes = Encoding.ASCII.GetBytes(fieldName);
            var valueBytes = value.ToBytes();

            var valueLength = valueBytes.Length;
            var lengthBytes = BitConverter.GetBytes((uint)valueLength);
            lengthBytes = EndianConverter.ConvertToLittleEndianBytes(lengthBytes);

            var dataLength = valueLength + NAME_LENGTH_BINARY_DATA_LENGTH;
            var returnBytes = new byte[dataLength];
            Buffer.BlockCopy(lengthBytes, 0, returnBytes, 0, lengthBytes.Length);
            Buffer.BlockCopy(fieldNameBytes, 0, returnBytes, lengthBytes.Length, fieldNameBytes.Length);
            Buffer.BlockCopy(valueBytes, 0, returnBytes, lengthBytes.Length + fieldNameBytes.Length, valueLength);

            return returnBytes;
        }
    }

    public class Packet : IBinarySerializable
    {
        private IEnumerable<PacketField> packetFields;
        public Packet(IEnumerable<PacketField> packetFields) => this.packetFields = packetFields;

        public byte[] ToBytes()
        {
            var fieldBytes = packetFields.Select(packetField => packetField.ToBytes());
            var dataLength = fieldBytes.Select(bytes => bytes.Length).Sum();

            var returnBytes = new byte[dataLength];
            var currentPosition = 0;
            foreach(var bytes in fieldBytes)
            {
                Buffer.BlockCopy(bytes, 0, returnBytes, currentPosition, bytes.Length);
                currentPosition += bytes.Length;
            }

            return returnBytes;           
        }
    }
}
