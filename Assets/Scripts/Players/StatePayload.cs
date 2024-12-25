using System;
using Unity.Netcode;
using UnityEngine;

public interface IStatePayload : INetworkSerializable
{
    public int Tick { get; set; }
}

public interface IStatePayloadArray<StatePayload> : INetworkSerializable, IEquatable<IStatePayloadArray<StatePayload>>
{
    public StatePayload[] Array { get; set; }
}