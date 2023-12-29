#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MocopiSender
{
    class MocopiSender : MonoBehaviour
    {
        private const string JOINT_OBJECT_NAME_PREFIX = "human_low:_";
        private const float SKDF_PACKET_SENDING_INTERVAL = 1.0f;
        private const float FRAM_PACKET_SENDING_INTERVAL = 0.1f;

        [SerializeField] private string remoteAddress = "127.0.0.1";
        [SerializeField] private string localAddress = "0.0.0.0";
        [SerializeField] private int localReceiverPort = 12351;
        [SerializeField] private Transform humanRigRoot = default!;

        private UdpSender? udpSender;

        private Packet? skdfPacket;
        private List<Transform?> rigTransforms = new();
        private List<ushort> localAddressUshorts = new();

        private int sendFrameCount = 0;

        private float latestSendSkdfPacketTime = float.NegativeInfinity;
        private float latestSendFramPacketTime = float.NegativeInfinity;

        private void Awake()
        {
            rigTransforms = jointNameList.Select(jointName => ObjectFinder.FindObjectByName(humanRigRoot, JOINT_OBJECT_NAME_PREFIX + jointName))
                                            .ToList();
            var skeletonDefinition = rigTransforms.Select(rigTransform => GetSkeletonDefinitionComponent(rigTransform, rigTransforms))
                                        .ToList();
            try
            {
                localAddressUshorts = localAddress.Split('.')
                                                    .Select(part => Convert.ToUInt16(part))
                                                    .ToList();
                skdfPacket = PacketGenerator.GenerateSkdfPacket(localAddressUshorts, (ushort)localReceiverPort, skeletonDefinition);
            }
            catch(Exception e)
            {
                Debug.LogError("Invalid Local IP address: " + e.Message);
            }

        }

        private (Vector3, Quaternion, int) GetSkeletonDefinitionComponent(Transform? rigTransform, List<Transform?> rigTransforms)
        {
            var defaultLocalPosition = rigTransform?.localPosition ?? Vector3.zero;
            var defaultLocalRotation = rigTransform?.localRotation ?? Quaternion.identity;
            return (defaultLocalPosition, defaultLocalRotation, rigTransforms.IndexOf(rigTransform?.parent));
        }

        private void OnEnable()
        {
            try
            {
                udpSender = new UdpSender(remoteAddress);
            }
            catch(Exception e)
            {
                Debug.LogError("Error initializing socket: " + e.Message);
                enabled = false;
            }
        }

        private void OnDisable()
        {
            udpSender?.Close();
        }

        private void Update()
        {
            var currentTime = Time.time;
            if(skdfPacket != null && currentTime - latestSendSkdfPacketTime > SKDF_PACKET_SENDING_INTERVAL)
            {
                try
                {
                    udpSender?.SendData(skdfPacket.ToBytes());
                }
                catch(Exception e)
                {
                    Debug.LogError("Error sending SKDF packet: " + e.Message);
                }
                latestSendSkdfPacketTime = currentTime;
            }
            if(currentTime - latestSendFramPacketTime > FRAM_PACKET_SENDING_INTERVAL)
            {
                ++sendFrameCount;
                try
                {
                    double unixTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeSeconds();
                    var framPacket = PacketGenerator.GenerateFramPacket(sendFrameCount, currentTime, unixTime, localAddressUshorts, (ushort)localReceiverPort, rigTransforms);
                    udpSender?.SendData(framPacket.ToBytes());
                }
                catch(Exception e)
                {
                    Debug.LogError("Error sending FRAM packet: " + e.Message);
                }
                latestSendFramPacketTime = currentTime;
            }
        }

        private static readonly List<string> jointNameList = new()
        {
            "root",
            "torso_1",
            "torso_2",
            "torso_3",
            "torso_4",
            "torso_5",
            "torso_6",
            "torso_7",
            "neck_1",
            "neck_2",
            "head",
            "l_shoulder",
            "l_up_arm",
            "l_low_arm",
            "l_hand",
            "r_shoulder",
            "r_up_arm",
            "r_low_arm",
            "r_hand",
            "l_up_leg",
            "l_low_leg",
            "l_foot",
            "l_toes",
            "r_up_leg",
            "r_low_leg",
            "r_foot",
            "r_toes",
       };
    }
}