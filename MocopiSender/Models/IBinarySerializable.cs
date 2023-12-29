#nullable enable

namespace MocopiSender
{
    public interface IBinarySerializable
    {
        byte[] ToBytes();
    }
}