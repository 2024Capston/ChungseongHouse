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
            EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventA, OnClickStartButton);
            _tubeStage1Controller.StartDoorOpen();
        }

        public override void RestartGame()
        {
            StopCoroutine(_currentGame);
            _tubeStage1Controller.SetStartButton(true);
        }

        public override void EndGame()
        {
            EventBus.Instance.UnsubscribeEvent<UnityAction>(EventType.EventA, OnClickStartButton);
        }

        private void OnClickStartButton()
        {
            _tubeStage1Controller.SetStartButton(false);
            _currentGame = StartCoroutine(CoProgressGame());
        }

        private IEnumerator CoProgressGame()
        {
            TubeStage1Mapper.Instance.InitTube(); 
            TubeStage1Mapper.Instance.ResetState();

            bool isSuccess = true;
            
            for (int i = 0; i < 8; i++)
            {
                int lightMap = Random.Range(0, 15);
                TubeStage1Mapper.Instance.SetButtonGroupColor();
                TubeStage1Mapper.Instance.SetSourceTubeLight(lightMap);
                
                
                yield return new WaitForSeconds(15f);
                TubeStage1Mapper.Instance.ApplyState();

                isSuccess &= lightMap == TubeStage1Mapper.Instance.ButtonState;
                Logger.Log($"{lightMap} {TubeStage1Mapper.Instance.ButtonState}");
                
                TubeStage1Mapper.Instance.ResetState();
                
            }
        }
    }
}

