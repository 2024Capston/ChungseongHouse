using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSyncObject<InputPayload, InputPayloadArray, StatePayload, StatePayloadArray> : NetworkBehaviour where InputPayload : struct, IInputPayload where InputPayloadArray : struct, IInputPayloadArray<InputPayload> where StatePayload : struct, IStatePayload where StatePayloadArray : struct, IStatePayloadArray<StatePayload>
{
    protected const int BUFFER_SIZE = 1024;

    protected InputPayload[] _inputBuffer = new InputPayload[BUFFER_SIZE];
    protected StatePayload[] _stateBuffer = new StatePayload[BUFFER_SIZE];

    protected Queue<InputPayload[]> _inputQueue = new Queue<InputPayload[]>();
    protected Queue<StatePayload[]> _stateQueue = new Queue<StatePayload[]>();

    protected InputPayload _processingInput;
    protected StatePayload _reconcileTarget;
    protected int _processingTick = 0;

    protected int _lastReceivedInputTick = 0;
    protected int _lastReceivedStateTick = 0;

    public override void OnNetworkSpawn()
    {
        NetworkSyncManager.Instance.GetClientInput += GetClientInput;
        NetworkSyncManager.Instance.GetClientState += GetClientState;

        if (IsServer)
        {
            if (!IsOwner) NetworkSyncManager.Instance.GetServerInput += GetServerInput;
            NetworkSyncManager.Instance.GetServerState += GetServerState;
        }
        else
        {
            NetworkSyncManager.Instance.GetReconcileCondition += GetReconcileCondition;
            NetworkSyncManager.Instance.PreReconcile += PreReconcile;
            NetworkSyncManager.Instance.GetReconcileInput += GetReconcileInput;
            NetworkSyncManager.Instance.GetReconcileState += GetReconcileState;
        }
    }

    public override void OnNetworkDespawn()
    {
        NetworkSyncManager.Instance.GetClientInput -= GetClientInput;
        NetworkSyncManager.Instance.GetClientState -= GetClientState;

        if (IsServer)
        {
            NetworkSyncManager.Instance.GetServerInput -= GetServerInput;
            NetworkSyncManager.Instance.GetServerState -= GetServerState;
        }
        else
        {
            NetworkSyncManager.Instance.GetReconcileCondition -= GetReconcileCondition;
            NetworkSyncManager.Instance.PreReconcile -= PreReconcile;
            NetworkSyncManager.Instance.GetReconcileInput -= GetReconcileInput;
            NetworkSyncManager.Instance.GetReconcileState -= GetReconcileState;
        }
    }

    public virtual bool GetInput()
    {
        return false;
    }

    public virtual void ApplyInput(InputPayload inputPayload)
    {
        return;
    }

    protected void GetClientInput()
    {
        if (GetInput())
        {
            ApplyInput(_processingInput);

            List<InputPayload> inputs = new List<InputPayload>();
            inputs.Add(_processingInput);

            for (int i = 1; i < 10; i++)
            {
                int bufferIndex = (_processingInput.Tick - i) % BUFFER_SIZE;

                if (bufferIndex >= 0 && _inputBuffer[bufferIndex].Tick == _processingInput.Tick - i)
                {
                    inputs.Insert(0, _inputBuffer[bufferIndex]);
                }
            }

            InputPayloadArray inputPayloadArray = new InputPayloadArray();
            inputPayloadArray.Array = inputs.ToArray();

            if (IsServer)
            {
                SendInputClientRpc(inputPayloadArray);
            }
            else
            {
                SendInputServerRpc(inputPayloadArray);
            }
        }
    }

    public virtual StatePayload GetState()
    {
        return new StatePayload();
    }

    protected void GetClientState()
    {
        StatePayload statePayload = GetState();

        statePayload.Tick = NetworkSyncManager.Instance.CurrentTick;

        _stateBuffer[statePayload.Tick % BUFFER_SIZE] = statePayload;
    }

    protected void GetServerInput()
    {
        if (_inputQueue.Count > 0)
        {
            while (_inputQueue.Count > 0)
            {
                InputPayload[] inputs = _inputQueue.Dequeue();

                for (int i = 0; i < inputs.Length; i++)
                {
                    if (inputs[i].Tick > _lastReceivedInputTick)
                    {
                        _lastReceivedInputTick = inputs[i].Tick;
                        _processingInput = inputs[i];
                        ApplyInput(_processingInput);
                        break;
                    }
                }
            }
        }
        else
        {
            for (int i = 1; i < 10; i++)
            {
                int bufferIndex = (NetworkSyncManager.Instance.CurrentTick - i) % BUFFER_SIZE;

                if (bufferIndex < 0)
                {
                    break;
                }
                else if (_inputBuffer[bufferIndex].Tick == NetworkSyncManager.Instance.CurrentTick - i)
                {
                    _processingInput = _inputBuffer[bufferIndex];
                    ApplyInput(_processingInput);
                }
            }
        }
    }

    protected void GetServerState()
    {
        StatePayload statePayload = GetState();

        statePayload.Tick = _processingInput.Tick;

        List<StatePayload> states = new List<StatePayload>();
        states.Add(statePayload);

        for (int i = 1; i < 10; i++)
        {
            int bufferIndex = (statePayload.Tick - i) % BUFFER_SIZE;

            if (bufferIndex >= 0 && _stateBuffer[bufferIndex].Tick == statePayload.Tick - i)
            {
                states.Insert(0, _stateBuffer[bufferIndex]);
            }
        }

        StatePayloadArray statePayloadArray = new StatePayloadArray();
        statePayloadArray.Array = states.ToArray();

        SendStateClientRpc(statePayloadArray);
    }

    public virtual bool GetReconcilePredicate(StatePayload oldState, StatePayload newState)
    {
        return false;
    }

    protected void GetReconcileCondition()
    {
        while (_stateQueue.Count > 0)
        {
            StatePayload[] states = _stateQueue.Dequeue();

            StatePayload statePayload = new StatePayload();
            statePayload.Tick = -1;

            for (int i = 0; i < states.Length; i++)
            {
                if (states[i].Tick > _lastReceivedStateTick)
                {
                    _lastReceivedStateTick = states[i].Tick;
                    statePayload = states[i];
                }
            }

            int bufferIndex = statePayload.Tick % BUFFER_SIZE;

            if (bufferIndex == -1 || _stateBuffer[bufferIndex].Tick != statePayload.Tick)
            {
                continue;
            }

            if (GetReconcilePredicate(statePayload, _stateBuffer[bufferIndex]))
            {
                if (!NetworkSyncManager.Instance.NeedReconcile)
                {
                    NetworkSyncManager.Instance.NeedReconcile = true;
                    NetworkSyncManager.Instance.ReconcileTick = statePayload.Tick;

                    _reconcileTarget = statePayload;
                }
                else if (NetworkSyncManager.Instance.ReconcileTick > statePayload.Tick)
                {
                    NetworkSyncManager.Instance.ReconcileTick = statePayload.Tick;

                    _reconcileTarget = statePayload;
                }
                else
                {
                    _reconcileTarget = statePayload;
                }
            }
        }
    }

    public virtual void ApplyPreReconcile(StatePayload newState)
    {
        return;
    }

    protected void PreReconcile(int reconcileTick)
    {
        int bufferIndex = reconcileTick % BUFFER_SIZE;

        if (_reconcileTarget.Tick == reconcileTick)
        {
            ApplyPreReconcile(_reconcileTarget);
        }
        else if (_stateBuffer[bufferIndex].Tick == reconcileTick)
        {
            ApplyPreReconcile(_stateBuffer[bufferIndex]);
        }
    }

    public virtual void ApplyReconcileInput(InputPayload inputPayload)
    {
        return;
    }

    protected void GetReconcileInput(int reconcileTick)
    {
        if (reconcileTick == _reconcileTarget.Tick)
        {
            ApplyPreReconcile(_reconcileTarget);
            return;
        }

        int bufferIndex = reconcileTick % BUFFER_SIZE;

        if (reconcileTick == _inputBuffer[bufferIndex].Tick)
        {
            _processingInput = _inputBuffer[bufferIndex];

            ApplyReconcileInput(_processingInput);
        }
    }

    protected void GetReconcileState()
    {
        StatePayload statePayload = GetState();

        statePayload.Tick = _processingInput.Tick;

        _stateBuffer[statePayload.Tick % BUFFER_SIZE] = statePayload;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendInputServerRpc(InputPayloadArray inputPayload) {
        _inputQueue.Enqueue(inputPayload.Array);
        SendInputClientRpc(inputPayload);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendInputClientRpc(InputPayloadArray inputPayload)
    {
        for (int i = 0; i < inputPayload.Array.Length; i++)
        {
            int bufferIndex = inputPayload.Array[i].Tick % BUFFER_SIZE;

            _inputBuffer[bufferIndex] = inputPayload.Array[i];
        }
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendStateClientRpc(StatePayloadArray statePayload)
    {
        _stateQueue.Enqueue(statePayload.Array);
    }
}
