using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TubeStage
{
    public class TubeStage1Controller : NetworkBehaviour
    {
        [SerializeField] private DoorController _startDoorController;
        [SerializeField] private DoorController _endDoorController;
        [SerializeField] private GenericButtonController _startButton;
        
        public void StartDoorOpen()
        {
            _startDoorController.IsOpened = true;
            _startDoorController.OpenDoorServerRpc();
        }

        public void EndDoorOpen()
        {
            _endDoorController.IsOpened = true;
            _endDoorController.OpenDoorServerRpc();
        }

        public void SetStartButton(bool isClear)
        {
            if (isClear)
            {
                _startButton.SetButtonColor(ColorType.Purple);
                _startButton.UnpressButton();
                _startButton.PlayPressAnimation(false);
                _startButton.enabled = true;
            }
            else
            {
                _startButton.SetButtonColor(ColorType.None);
                _startButton.enabled = false;
            }
        }
    }

}

