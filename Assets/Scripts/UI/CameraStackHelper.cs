using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Camera))]
public class CameraStackHelper : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(CoSetCamera());
    }

    private IEnumerator CoSetCamera()
    {
        yield return new WaitUntil(() => Camera.main != null);
        
        var mainCamera = Camera.main;
        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        mainCameraData.renderType = CameraRenderType.Base;

        /*var camera = GetComponent<Camera>();
        var cameraData = camera.GetUniversalAdditionalCameraData();
        cameraData.renderType = CameraRenderType.Overlay;
        mainCameraData.cameraStack.Add(camera);

        var uiManagerCameara = UIManager.Instance.UICamara;
        var uiManagerCameraData = uiManagerCameara.GetUniversalAdditionalCameraData();
        uiManagerCameraData.renderType = CameraRenderType.Overlay;
        mainCameraData.cameraStack.Add(uiManagerCameara);*/
    }
}
