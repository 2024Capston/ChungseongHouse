using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{ 
    public class CC4_StageManager : StageManager
    {
        public override void EndGame()
        {
            InGameManager.Instance.EndGameServerRpc();
        }

        public override void RestartGame()
        {
        }

        public override void StartGame()
        {
            EventBus.Instance.SubscribeEvent<UnityAction<GameObject>>(EventType.EventA, OnRoomEntered);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventC, OnPlatePressed);
        }

        public void OnRoomEntered(GameObject other)
        {
            if (other.GetComponent<CubeController>())
            {
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
        }

        public void OnPlatePressed(PlateController plateController, GameObject objectOnPlate)
        {
            if (objectOnPlate.GetComponent<CubeController>())
            {
                EventBus.Instance.InvokeEvent(EventType.EventD);
            }
        }
    }
}