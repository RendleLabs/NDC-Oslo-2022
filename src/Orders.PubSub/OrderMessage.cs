using System;
using MessagePack;

namespace Orders.PubSub;

[MessagePackObject]
public class OrderMessage
{
    [Key(0)] public string CrustId { get; set; } = null!;
    [Key(1)] public string[] ToppingIds { get; set; } = null!;
    [Key(2)] public DateTimeOffset Time { get; set; }

    public byte[] ToBytes() =>
        MessagePackSerializer.Serialize(this);

    public static OrderMessage FromBytes(byte[] bytes) =>
        MessagePackSerializer.Deserialize<OrderMessage>(bytes);
}