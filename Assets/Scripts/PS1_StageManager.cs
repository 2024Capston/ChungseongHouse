using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PS1_StageManager : StageManager
{
    private int _buttonCount = 0;

    private bool[] _clear = new bool[2];

    public override void EndGame()
    {
        InGameManager.Instance.EndGameServerRpc();
    }

    public override void RestartGame()
    {
    }

    public override void StartGame()
    {
        EventBus.Instance.SubscribeEvent<UnityAction>(EventType.EventE, OnPressButton);
        EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventA, OnPressPlateA);
        EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventB, OnPressPlateB);
        EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventG, OnPressPlateG);
        EventBus.Instance.SubscribeEvent<UnityAction<PlateController, GameObject>>(EventType.EventH, OnPressPlateH);
    }

    public void OnPressButton()
    {
        if (++_buttonCount == 2)
        {
            EventBus.Instance.InvokeEvent(EventType.EventF);
        }
    }

    public void OnPressPlateA(PlateController plate, GameObject objectOnPlate)
    {
        if (plate.ObjectsOnPlate.Count > 0)
        {
            EventBus.Instance.InvokeEvent(EventType.EventC);
        }
        else
        {
            EventBus.Instance.InvokeEvent(EventType.EventD);
        }
    }

    public void OnPressPlateB(PlateController plate, GameObject objectOnPlate)
    {
        if (plate.ObjectsOnPlate.Count > 0)
        {
            EventBus.Instance.InvokeEvent(EventType.EventI);
        }
        else
        {
            EventBus.Instance.InvokeEvent(EventType.EventJ);
        }
    }

    public void OnPressPlateG(PlateController plate, GameObject objectOnPlate)
    {
        _clear[0] = plate.ObjectsOnPlate.Count > 0;

        if (_clear[0] && _clear[1])
        {
            EndGame();
        }
    }

    public void OnPressPlateH(PlateController plate, GameObject objectOnPlate)
    {
        _clear[1] = plate.ObjectsOnPlate.Count > 0;

        if (_clear[0] && _clear[1])
        {
            EndGame();
        }
    }
}
