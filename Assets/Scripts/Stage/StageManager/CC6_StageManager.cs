using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace ColorChanger
{
    public class CC6_StageManager : StageManager
    {
        public override void EndGame()
        {
        }

        public override void RestartGame()
        {
            foreach(PlayerController playerController in FindObjectsOfType<PlayerController>())
            {
                playerController.RespawnPlayer();
                playerController.ForceStopInteraction();
            }

            foreach(NetworkObjectSpawner networkObjectSpawner in FindObjectsOfType<NetworkObjectSpawner>())
            {
                networkObjectSpawner.SpawnObject();
            }
        }

        public override void StartGame()
        {
        }
    }
}