using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlatformInputPayload : IInputPayload
{
    private int _tick;
    public int Tick
    {
        get => _tick;
        set => _tick = value;
    }

    private float _y;
    public float Y
    {
        get => _y;
        set => _y = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _tick);
        serializer.SerializeValue(ref _y);
    }

    public bool Equals(IInputPayload other)
    {
        return _tick == other.Tick;
    }
}

public struct PlatformInputPayloadArray : IInputPayloadArray<PlatformInputPayload>
{
    private PlatformInputPayload[] _array;
    PlatformInputPayload[] IInputPayloadArray<PlatformInputPayload>.Array
    {
        get => _array;
        set => _array = value;
    }

    public bool Equals(IInputPayloadArray<PlatformInputPayload> other)
    {
        if (_array.Length != other.Array.Length)
        {
            return false;
        }

        for (int i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other.Array[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = 0;

        if (!serializer.IsReader)
        {
            length = _array.Length;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            _array = new PlatformInputPayload[length];
        }

        for (int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref _array[i]);
        }
    }
}

public struct PlatformStatePayload : IStatePayload
{
    private int _tick;
    public int Tick
    {
        get => _tick;
        set => _tick = value;
    }

    private float _y;
    public float Y
    {
        get => _y;
        set => _y = value;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _tick);
        serializer.SerializeValue(ref _y);
    }
}

public struct PlatformStatePayloadArray : IStatePayloadArray<PlatformStatePayload>
{
    private PlatformStatePayload[] _array;
    PlatformStatePayload[] IStatePayloadArray<PlatformStatePayload>.Array
    {
        get => _array;
        set => _array = value;
    }

    public bool Equals(IStatePayloadArray<PlatformStatePayload> other)
    {
        if (_array.Length != other.Array.Length)
        {
            return false;
        }

        for (int i = 0; i < _array.Length; i++)
        {
            if (!_array[i].Equals(other.Array[i]))
            {
                return false;
            }
        }

        return true;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        int length = 0;

        if (!serializer.IsReader)
        {
            length = _array.Length;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            _array = new PlatformStatePayload[length];
        }

        for (int i = 0; i < length; i++)
        {
            serializer.SerializeValue(ref _array[i]);
        }
    }
}

[GenerateSerializationForTypeAttribute(typeof(PlatformInputPayloadArray))]
[GenerateSerializationForTypeAttribute(typeof(PlatformStatePayloadArray))]
public class PlatformMover : NetworkSyncObject<PlatformInputPayload, PlatformInputPayloadArray, PlatformStatePayload, PlatformStatePayloadArray>
{
    float acc_time = 0f;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        GameObject.Find("Elevator").GetComponent<NetworkSyncInterpolator>().Target = gameObject                             ;
    }

    void FixedUpdate()
    {
        if (!IsServer)
        {
            return;
        }

        acc_time += Time.fixedDeltaTime;

        if (acc_time > 2 * Mathf.PI)
        {
            acc_time = 0;
        }
    }

    public override bool GetInput()
    {
        if (IsServer)
        {
            _processingInput.Tick = NetworkSyncManager.Instance.CurrentTick;

            _processingInput.Y = (Mathf.Sin(acc_time) + 1) * 5;

            return true;
        }
        else
        {
            return false;
        }
    }

    public override void ApplyInput(PlatformInputPayload inputPayload)
    {
        Vector3 pos = GetComponent<Rigidbody>().position;
        pos.y = inputPayload.Y;
        GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(GetComponent<Rigidbody>().position, pos, Time.fixedDeltaTime * 5f));
    }

    public override void ApplyReconcileInput(PlatformInputPayload inputPayload)
    {
        Vector3 pos = GetComponent<Rigidbody>().position;
        pos.y = inputPayload.Y;
        GetComponent<Rigidbody>().MovePosition(Vector3.MoveTowards(GetComponent<Rigidbody>().position, pos, Time.fixedDeltaTime * 5f));
    }

    public override void ApplyPreReconcile(PlatformStatePayload newState)
    {
        Vector3 pos = GetComponent<Rigidbody>().position;
        pos.y = newState.Y;
        GetComponent<Rigidbody>().position = pos;
    }

    public override PlatformStatePayload GetState()
    {
        PlatformStatePayload statePayload = new PlatformStatePayload();

        statePayload.Y = GetComponent<Rigidbody>().position.y;

        return statePayload;
    }

    public override bool GetReconcilePredicate(PlatformStatePayload oldState, PlatformStatePayload newState)
    {
        float posDif = Mathf.Abs(oldState.Y - newState.Y);

        return posDif > 0.0000001f;
    }

    void OnGUI()
    {
        Color originalColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.black;

        GUIStyle customLabelStyle = new GUIStyle(GUI.skin.label);
        customLabelStyle.padding = new RectOffset(2, 2, 2, 2);
        customLabelStyle.margin = new RectOffset(0, 0, 0, 0);

        if (gameObject.TryGetComponent<PlatformMover>(out PlatformMover pm))
        {
            GUI.Box(new Rect(585, 55, 260, 300), GUIContent.none);
            GUILayout.BeginArea(new Rect(590, 60, 250, 290));

            GUILayout.Label("Platform:", customLabelStyle);
            GUILayout.Label($"Processing Tick: {_processingTick}");
            GUILayout.Label($"Reconcile Target: {_reconcileTarget.Tick}");

            GUILayout.Label($"Input: \t\t State:");

            if (NetworkSyncManager.Instance.CurrentTick >= 10)
            {
                for (int j = NetworkSyncManager.Instance.CurrentTick - 10; j < NetworkSyncManager.Instance.CurrentTick; j++)
                {
                    int i = j % 1024;
                    GUILayout.Label($"{_inputBuffer[i].Tick % 1000}: 0.0 {_inputBuffer[i].Y:0.0} 0.0 \t {_stateBuffer[i].Tick % 1000}: 0.0 {_stateBuffer[i].Y:0.0} 0.0", customLabelStyle);
                }
            }
            GUILayout.EndArea();
        }

        GUI.backgroundColor = originalColor;
    }
}
