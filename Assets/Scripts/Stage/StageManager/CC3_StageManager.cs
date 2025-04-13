using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace ColorChanger
{
    public class CC3_StageManager : StageManager
    {
        private bool[] _plateStates;

        public override void EndGame()
        {
            InGameManager.Instance.EndGameServerRpc();
        }

        public override void RestartGame()
        {
            EventBus.Instance.ClearEventBus();

            foreach (PlayerController playerController in FindObjectsOfType<PlayerController>())
            {
                playerController.RespawnPlayer();
                playerController.ForceStopInteraction();
            }

            foreach (NetworkObjectSpawner networkObjectSpawner in FindObjectsOfType<NetworkObjectSpawner>())
            {
                networkObjectSpawner.SpawnObject();
            }

            StartGame();
        }

        public override void StartGame()
        {
            _plateStates = new bool[4];

            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnPlateUpdateA);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventB, OnPlateUpdateB);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventC, OnPlateUpdateC);
            EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventD, OnPlateUpdateD);
        }

        private void OnPlateUpdate(EventType callerEvent, PlateController plate)
        {
            bool hasCube = false;

            foreach (GameObject objectOnPlate in plate.ObjectsOnPlate)
            {
                if (objectOnPlate.GetComponent<CubeController>() != null)
                {
                    hasCube = true;
                    break;
                }
            }

            _plateStates[(int)callerEvent] = hasCube;

            if (_plateStates[(int)callerEvent])
            {
                EventBus.Instance.InvokeEvent(callerEvent + 4, MonitorType.CheckMark);
            }
            else
            {
                EventBus.Instance.InvokeEvent(callerEvent + 4, MonitorType.CubeMark);
            }

            bool isClear = true;

            foreach (bool plateState in _plateStates)
            {
                if (!plateState)
                {
                    isClear = false;
                    break;
                }
            }

            if (isClear)
            {
                EventBus.Instance.InvokeEvent(EventType.EventI);
            }
        }

        public void OnPlateUpdateA(PlateController plate, GameObject cubeOnPlate)
        {
            OnPlateUpdate(EventType.EventA, plate);
        }

        public void OnPlateUpdateB(PlateController plate, GameObject cubeOnPlate)
        {
            OnPlateUpdate(EventType.EventB, plate);
        }

        public void OnPlateUpdateC(PlateController plate, GameObject cubeOnPlate)
        {
            OnPlateUpdate(EventType.EventC, plate);
        }

        public void OnPlateUpdateD(PlateController plate, GameObject cubeOnPlate)
        {
            OnPlateUpdate(EventType.EventD, plate);
        }
    }
}