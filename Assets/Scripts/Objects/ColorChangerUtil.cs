using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace ColorChanger
{
    /// <summary>
    /// 색깔 변환기로 변환된 물체에 추가되는 Class
    /// </summary>
    public class ColorChangerUtil : MonoBehaviour
    {
        private const float TRANSITION_TIME = 2f;
        private CubeController _cubeController;
        private SpriteRenderer _spriteRenderer;

        private bool _timerStarted;
        private bool _timerEnded;
        private float _changeCooldown = 0f;
        private float _timer;

        private void Update()
        {
            if (Camera.main)
            {
                transform.LookAt(Camera.main.transform.position);
            }

            if (!_timerStarted || _timerEnded)
            {
                return;
            }

            _timer += Time.deltaTime;
            _spriteRenderer.material.SetFloat("_Arc2", (1f - _timer / _changeCooldown) * 360f);

            // 일정 시간이 지나면 색깔을 되돌린다.
            if (_timer > _changeCooldown)
            {
                _cubeController.ForceStopInteraction();
                _cubeController.GetComponent<CubeRenderer>().PlayTransitionAnimation(TRANSITION_TIME);

                _spriteRenderer.enabled = false;
                _timerEnded = true;

                if (NetworkManager.Singleton.IsServer)
                {
                    StartCoroutine("CoChangeCubeColor", _cubeController);
                }
            }
        }

        private IEnumerator CoChangeCubeColor(CubeController cubeController)
        {
            cubeController.SetActive(false);

            yield return new WaitForSeconds(TRANSITION_TIME);

            cubeController.SetActive(true);

            // 3 - (ColorType enum): 색깔 교체
            cubeController.ChangeColor(3 - cubeController.Color);

            Destroy(gameObject);
        }

        /// <summary>
        /// 색깔 변환 메커니즘을 초기화한다.
        /// </summary>
        /// <param name="changeTime">색깔 변환 지속시간</param>
        public void Initialize(CubeController cubeController, float changeTime)
        {
            _changeCooldown = changeTime;

            _cubeController = cubeController;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _spriteRenderer.enabled = false;
        }

        /// <summary>
        /// 색깔 변환 메커니즘을 시작한다.
        /// </summary>
        public void StartTimer()
        {
            _timerStarted = true;
            _spriteRenderer.enabled = true;
        }
    }
}