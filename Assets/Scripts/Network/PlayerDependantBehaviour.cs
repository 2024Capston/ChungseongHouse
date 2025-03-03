using System;
using Unity.Netcode;

/// <summary>
/// 로컬 플레이어의 생성을 전제로 하는 Class
/// </summary>
//public class PlayerDependantBehaviour : NetworkBehaviour
//{
//    public void SubscribeToPlayerCreation(Action action)
//    {
//        if (PlayerController.LocalPlayer)
//        {
//            action.Invoke();
//        }
//        else
//        {
//            PlayerController.LocalPlayerCreated += action;
//        }
//    }
//}
