using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 스테이지 목표 지점을 관리하는 Class
/// </summary>
public class GoalController : MonoBehaviour
{
    private int _playersCount = 0;  // 목표 지점에 들어온 플레이어 수

    private void OnTriggerEnter(Collider other)
    {
        if (NetworkManager.Singleton.IsServer && other.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            // !! 플레이어 둘이 모두 들어온 경우
            if (++_playersCount == 1)   //test로 1로 바꿈
            {
                // !! CLEAR
                StageManager.Instance.EndGame();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (NetworkManager.Singleton.IsServer && other.TryGetComponent<PlayerController>(out PlayerController playerController))
        {
            _playersCount--;
        }
    }
}
