using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class PlayerConfig : NetworkBehaviour
{
    public bool IsBlue { get { return IsHost; } }
    public PlayerController MyPlayer { get; set; }
    public string PlayerName => SteamClient.Name;
}
