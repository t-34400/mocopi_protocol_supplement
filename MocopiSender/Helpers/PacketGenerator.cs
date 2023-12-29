#nullable enable

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MocopiSender
{
    static class PacketGenerator
    {
        internal static Packet GenerateSkdfPacket(IEnumerable<ushort> localAddress, ushort localReceiverPort, List<(Vector3 defaultLocalPosition, Quaternion defaultLocalRotation, int parentIndex)> skeletonDefinition)
        {
            var headValues = new List<IBinarySerializable>()
            {
                new PacketField("ftyp", new StringValue("sony motion format")),
                new PacketField("vrsn", new UintValue_1Byte(1)),
            };
            var sndfValues = new List<IBinarySerializable>()
            {
                new PacketField("ipad", new IpAddressValue(localAddress)),
                new PacketField("rcvp", new UintValue_2Byte(localReceiverPort))
            };
            var bonsValues = skeletonDefinition.Select((component, index) => GetBndtField(component.defaultLocalPosition, component.defaultLocalRotation, index, component.parentIndex))
                                        .ToList();

            var packetFields = new List<PacketField>()
            {
                new PacketField("head", headValues),
                new PacketField("sndf", sndfValues),
                new PacketField("skdf", new PacketField("bons", bonsValues)),
            };
            return new(packetFields);
        }

        private static IBinarySerializable GetBndtField(Vector3 defaultLocalPosition, Quaternion defaultLocalRotation, int rigIndex, int parentIndex)
        {
            var bndtValues = new List<IBinarySerializable>()
            {
                new PacketField("bnid", new UintValue_2Byte((ushort)rigIndex)),
                new PacketField("pbid", new UintValue_2Byte((ushort)parentIndex)),
                new PacketField("tran", new FloatArrayValue(ConvertLocalPositionAndRotation(defaultLocalPosition, defaultLocalRotation)))
            };
            return new PacketField("bndt", bndtValues);
        }

        internal static Packet GenerateFramPacket(int currentFrame, float currentTime, double unixTime, IEnumerable<ushort> localAddress, ushort localReceiverPort, List<Transform?> rigTransforms)
        {
            var headValues = new List<IBinarySerializable>()
            {
                new PacketField("ftyp", new StringValue("sony motion format")),
                new PacketField("vrsn", new UintValue_1Byte(1)),
            };
            var sndfValues = new List<IBinarySerializable>()
            {
                new PacketField("ipad", new IpAddressValue(localAddress)),
                new PacketField("rcvp", new UintValue_2Byte(localReceiverPort))
            };
            var btrsValues = rigTransforms.Select((rigTransform, index) => GetBtdtField(rigTransform, index))
                                        .ToList();
            var framValues = new List<IBinarySerializable>()
            {
                new PacketField("fnum", new UintValue_4Byte((uint)currentFrame)),
                new PacketField("time", new FloatValue(currentTime)),
                new PacketField("uttm", new DoubleValue(unixTime)),
                new PacketField("btrs", btrsValues),
            };

            var packetFields = new List<PacketField>()
            {
                new PacketField("head", headValues),
                new PacketField("sndf", sndfValues),
                new PacketField("fram", framValues),
            };
            return new(packetFields);
        }

        private static IBinarySerializable GetBtdtField(Transform? rigTransform, int rigIndex)
        {
            var bndtValues = new List<IBinarySerializable>()
            {
                new PacketField("bnid", new UintValue_2Byte((ushort)rigIndex)),
                new PacketField("tran", new FloatArrayValue(GetLocalPositionAndRotationFromTransform(rigTransform)))
            };
            return new PacketField("btdt", bndtValues);
        }


        private static List<float> GetLocalPositionAndRotationFromTransform(Transform? transform)
        {
            var localPosition = Vector3.zero;
            var localRotation = Quaternion.identity;
#if UNITY_2022_2_OR_NEWER
            transform?.GetLocalPositionAndRotation(out localPosition, out localRotation);
#else
            if(transform != null)
            {
                localPosition = transform.localPosition;
                localRotation = transform.localRotation;
            }            
#endif

            return ConvertLocalPositionAndRotation(localPosition, localRotation);

        }

        private static List<float> ConvertLocalPositionAndRotation(Vector3 localPosition, Quaternion localRotation) => new() 
            {
                -localRotation.x, localRotation.y, localRotation.z, -localRotation.w,
                -localPosition.x, localPosition.y, localPosition.z,
            };
    }
}