using System;
using Unity.Netcode;

public interface IInputPayload : INetworkSerializable, IEquatable<IInputPayload>
{
    public int Tick { get; set; }
}

public interface IInputPayloadArray<InputPayload> : INetworkSerializable, IEquatable<IInputPayloadArray<InputPayload>>
{
    public InputPayload[] Array { get; set; }
}