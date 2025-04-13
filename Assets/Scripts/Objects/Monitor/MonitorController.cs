using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class MonitorController : NetworkBehaviour
{
    MeshRenderer _screenMeshRenderer;

    [ClientRpc(RequireOwnership = false)]
    private void UpdateMonitorTypeClientRpc(MonitorType newType)
    {
        _screenMeshRenderer.material.SetFloat("_MonitorType", (float)newType);
    }

    public void UpdateMonitorType(MonitorType newType)
    {
        UpdateMonitorTypeClientRpc(newType);
    }

    [ClientRpc(RequireOwnership = false)]
    private void InitializeClientRpc(Vector3 position, Quaternion rotation, Vector3 scale, Quaternion frontRotation, MonitorType defaultMonitorType, EventType[] subscribeForScreenChange)
    {
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = scale;

        _screenMeshRenderer = transform.Find("Monitor_Front").transform.Find("Monitor_Screen").GetComponent<MeshRenderer>();

        transform.Find("Monitor_Front").transform.rotation = frontRotation;
        _screenMeshRenderer.material.SetFloat("_MonitorType", (float)defaultMonitorType);
    }

    public void Initialize(Quaternion frontRotation, MonitorType defaultMonitorType, EventType[] subscribeForMonitorUpdate)
    {
        InitializeClientRpc(transform.position, transform.rotation, transform.localScale, frontRotation, defaultMonitorType, subscribeForMonitorUpdate);

        foreach (EventType evn in subscribeForMonitorUpdate)
        {
            EventBus.Instance.SubscribeEvent<UnityAction<MonitorType>>(evn, UpdateMonitorType);
        }
    }
}