using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    public class CC3_StageManager : StageManager
    {
        private PlateController[] _plateControllers;
        private DoorController _exitDoorController;

        public override void EndGame()
        {
            EventBus.Instance.UnsubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnPlatePressed);
            InGameManager.Instance.EndGameServerRpc();
        }

        public override void RestartGame()
        {
        }

        public override void StartGame()
        {
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnPlatePressed);
        }

        private bool CheckClearCondition()
        {
            return true;
            foreach (PlateController plateController in _plateControllers)
            {
                bool hasCube = false;

                foreach (GameObject objectOnPlate in plateController.ObjectsOnPlate)
                {
                    if (objectOnPlate.GetComponent<CubeController>() != null)
                    {
                        hasCube = true;
                        break;
                    }
                }

                if (!hasCube)
                {
                    return false;
                }
            }

            return true;
        }

        public void OnPlatePressed(PlateController plate, GameObject cubeOnPlate)
        {
            if (CheckClearCondition())
            {
                EventBus.Instance.InvokeEvent(EventType.EventB);
            }
        }
    }
}