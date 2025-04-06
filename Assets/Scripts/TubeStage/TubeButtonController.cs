using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace TubeStage
{
    public class TubeButtonController : ButtonController
    {
        public override bool OnInteractableCheck(PlayerController player)
        {
            return _isEnabled;
        }

        public override bool OnStartInteraction(PlayerController player)
        {
            ClickButton();
            return false;
        }
        
        private void ClickButton()
        {
            if (_isPressed)
            {
                UnpressButton();
                PlayPressAnimation(false);
                TubeStageMapper.Instance.OnClickButtonServerRpc(_color, IsHost, false);
            }
            else
            {
                PressButton();
                PlayPressAnimation(true);
                TubeStageMapper.Instance.OnClickButtonServerRpc(_color, IsHost, true);
            }
        }
    }
}

