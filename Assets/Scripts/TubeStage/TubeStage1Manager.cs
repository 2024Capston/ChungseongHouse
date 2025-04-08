using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace TubeStage
{
    public class TubeStage1Manager : StageManager
    {
        private Coroutine _currentGame;

        private TubeStage1Controller _tubeStage1Controller;
        
        protected override void Init()
        {
            _isDestroyOnLoad = true;
            base.Init();
            _tubeStage1Controller = GetComponent<TubeStage1Controller>();
        }
        
        public override void StartGame()
        {
            _tubeStage1Controller.SetStartButton(true);
            TubeStage1Mapper.Instance.ResetAll();
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventA, OnClickStartButton);
            _tubeStage1Controller.StartDoorOpen();
        }

        public override void RestartGame()
        {
            if (_currentGame != null)
            {
                StopCoroutine(_currentGame);
            }
            _currentGame = null;
            TubeStage1Mapper.Instance.SetSourceTubeLight(0);
            _tubeStage1Controller.SetStartButton(true);
            TubeStage1Mapper.Instance.ResetAll();
        }

        public override void EndGame()
        {
            EventBus.Instance.UnsubscribeEvent<UnityAction>(EventType.EventA, OnClickStartButton);
            InGameManager.Instance.EndGameServerRpc();
        }

        private void OnClickStartButton()
        {
            _tubeStage1Controller.SetStartButton(false);
            _currentGame = StartCoroutine(CoProgressGame());
        }

        private IEnumerator CoProgressGame()
        {
            TubeStage1Mapper.Instance.InitTube(); 
            TubeStage1Mapper.Instance.ClearState();

            bool isSuccess = true;
            
            for (int i = 0; i < 8; i++)
            {
                int lightMap = Random.Range(0, 15);
                TubeStage1Mapper.Instance.SetButtonGroupColor();
                TubeStage1Mapper.Instance.SetSourceTubeLight(lightMap);
                
                
                yield return new WaitForSeconds(15f);
                TubeStage1Mapper.Instance.ApplyButtonState();

                isSuccess &= lightMap == TubeStage1Mapper.Instance.ButtonState;
                Logger.Log($"{lightMap} {TubeStage1Mapper.Instance.ButtonState}");
                
                TubeStage1Mapper.Instance.ClearState();
                
            }

            if (isSuccess)
            {
                _tubeStage1Controller.EndDoorOpen();
                TubeStage1Mapper.Instance.ClearAll();
            }
            else
            {
                RestartGame();
            }
        }
    }
}

