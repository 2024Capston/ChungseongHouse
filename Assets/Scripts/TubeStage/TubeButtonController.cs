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
            ClickButtonServerRpc();
            return false;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void ClickButtonServerRpc()
        {
            if (_isPressed)
            {
                UnpressButton();
                PlayPressAnimation(false);
                TubeStageMapper.Instance.OnClickButton(_color, IsHost, false);
            }
            else
            {
                PressButton();
                PlayPressAnimation(true);
                TubeStageMapper.Instance.OnClickButton(_color, IsHost, true);
            }
        }
    }
}

