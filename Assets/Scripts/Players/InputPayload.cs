using System.Runtime.InteropServices.WindowsRuntime;
using Unity.Netcode;
using UnityEngine;

public interface IInputPayload : INetworkSerializable
{
    public int Tick { get; set; }
}