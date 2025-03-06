using System;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 아직 Scene에 배치되지 않은 Delegate를 저장하기 위한 Class
/// </summary>
[Serializable]
public class DelegateWrapper
{
    [SerializeField] GameObject _gameObject;    // Delegate가 포함된 게임 오브젝트. 없을 경우 Scene 전체를 탐색해 찾는다
    [SerializeField] string _scriptName;        // Delegate가 포함된 스크립트 이름
    [SerializeField] string _functionName;      // Delegate의 함수 이름

    /// <summary>
    /// 게임 오브젝트, 스크립트 이름, 함수 이름을 바탕으로 Delegate를 반환한다.
    /// </summary>
    /// <returns>Delegate</returns>
    public Delegate GetDelegate()
    {
        // 게임 오브젝트가 주어진 경우
        if (_gameObject)
        {
            Type type = Type.GetType(_scriptName);
            Component component = null;

            // 해당 오브젝트가 NetworkObjectSpawner인 경우 스폰된 오브젝트를 사용
            if (_gameObject.TryGetComponent<NetworkObjectSpawner>(out NetworkObjectSpawner networkObjectSpawner))
            {
                component = networkObjectSpawner.SpawnedObject.GetComponentInChildren(type);
                type = component.GetType();
            }
            else
            {
                component = _gameObject.GetComponent(type);
                type = component.GetType();
            }

            MethodInfo methodInfo = type.GetMethod(_functionName);

            return Delegate.CreateDelegate(GetReturnType(methodInfo), component, methodInfo);
        }
        // 게임 오브젝트가 주어지지 않은 경우
        else
        {
            Type type = Type.GetType(_scriptName);

            // Scene 전체를 탐색
            Component component = GameObject.FindAnyObjectByType(type).GetComponent(type);
            type = component.GetType();

            MethodInfo methodInfo = type.GetMethod(_functionName);

            return Delegate.CreateDelegate(GetReturnType(methodInfo), component, methodInfo);
        }
    }

    /// <summary>
    /// 메소드 정보를 바탕으로 매개변수 개수에 맞는 UnityAction 자료형을 반환한다.
    /// </summary>
    /// <param name="methodInfo">메소드 정보</param>
    /// <returns>UnityAction 자료형</returns>
    private Type GetReturnType(MethodInfo methodInfo)
    {
        ParameterInfo[] parameters = methodInfo.GetParameters();

        if (parameters.Length == 0)
        {
            return typeof(UnityAction);
        }
        else if (parameters.Length == 1)
        {
            return typeof(UnityAction<>).MakeGenericType(parameters[0].ParameterType);
        }
        else if (parameters.Length == 2)
        {
            return typeof(UnityAction<,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType);
        }
        else
        {
            return typeof(UnityAction<,,>).MakeGenericType(parameters[0].ParameterType, parameters[1].ParameterType, parameters[2].ParameterType);
        }
    }
}