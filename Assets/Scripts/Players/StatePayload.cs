using Unity.Netcode;
using UnityEngine;

public interface IStatePayload : INetworkSerializable
{
    public int Tick { get; set; }
}