using UnityEngine;
using ProtoBuf;
using System.IO;
using System.Buffers;
using System;

public class Packets : MonoBehaviour
{
    public enum PacketType { Ping, Normal, Location = 3 , Emoticon = 4,Shot = 5 ,BulletMove = 6}
    public enum HandlerIds {
        Init = 0,
        GET_GAME=  1,
        CREATE_GAME=2,
        JOIN_GAME= 3,
        LOCATION_UPDATE = 10,
        SHOT_UPDATE = 11,
        EMOTICON_UPDATE = 20,
    }

    public static void Serialize<T>(IBufferWriter<byte> writer, T data)
    {
        Serializer.Serialize(writer, data);
    }

    public static T Deserialize<T>(byte[] data) {
        try {
            using (var stream = new MemoryStream(data)) {
                return ProtoBuf.Serializer.Deserialize<T>(stream);
            }
        } catch (Exception ex) {
            Debug.LogError($"Deserialize: Failed to deserialize data. Exception: {ex}");
            throw;
        }
    }
}

[ProtoContract]
public class InitialPayload
{
    [ProtoMember(1, IsRequired = true)]
    public string deviceId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public uint playerId { get; set; }

    [ProtoMember(3, IsRequired = true)]
    public float latency { get; set; }
}

[ProtoContract]
public class CreateGamePayload
{
    [ProtoMember(1, IsRequired = true)]
    public Int64 timestamp { get; set; }

}
[ProtoContract]
public class JoinGamePayload
{
    [ProtoMember(1, IsRequired = true)]
    public string gameId { get; set; }

}
[ProtoContract]
public class PingPayload
{
    [ProtoMember(1, IsRequired = true)]
    public Int64 timestamp { get; set; }

}

[ProtoContract]
public class CommonPacket
{
    [ProtoMember(1)]
    public uint handlerId { get; set; }

    [ProtoMember(2)]
    public string userId { get; set; }

    [ProtoMember(3)]
    public string clientVersion { get; set; }

    [ProtoMember(4)]
    public uint sequence { get; set; }

    [ProtoMember(5)]
    public byte[] payload { get; set; }
}

[ProtoContract]
public class LocationUpdatePayload {

    [ProtoMember(1, IsRequired = true)]
    public string gameId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public float x { get; set; }
    [ProtoMember(3, IsRequired = true)]
    public float y { get; set; }
    [ProtoMember(4, IsRequired = true)]
    public long timestamp { get; set; }
}

[ProtoContract]
public class LocationUpdate
{  
    [ProtoMember(1)]
    public string id { get; set; }

    [ProtoMember(2)]
    public float x { get; set; }

    [ProtoMember(3)]
    public float y { get; set; }
    [ProtoMember(4)]
    public float lockX { get; set; }

    [ProtoMember(5)]
    public float lockY { get; set; }
}
[ProtoContract]
public class ShotUpdate
{
    [ProtoMember(1)]
    public byte[] data { get; set; }
    
}

[ProtoContract]
public class RotationUpdatePayload
{

    [ProtoMember(1, IsRequired = true)]
    public string gameId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public float z { get; set; }

    [ProtoMember(3, IsRequired = true)]
    public long timestamp { get; set; }
}

[ProtoContract]
public class EmoticonUpdate
{
    [ProtoMember(1)]
    public string userId { get; set; }

    [ProtoMember(2)]
    public int emoticonId { get; set; }
   
}
[ProtoContract]
public class EmoticonPayload
{
    [ProtoMember(1 , IsRequired = true)]
    public int emoticonId { get; set; }

    [ProtoMember(2, IsRequired = true)]
    public string gameId { get; set; }
}


[ProtoContract]
public class Response
{
    [ProtoMember(1)]
    public uint handlerId { get; set; }

    [ProtoMember(2)]
    public uint responseCode { get; set; }

    [ProtoMember(3)]
    public long timestamp { get; set; }

    [ProtoMember(4)]
    public byte[] data { get; set; }

    [ProtoMember(5)]
    public uint sequence { get; set; }
}
