using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class NetworkSyncObject<InputPayload, StatePayload> : NetworkBehaviour where InputPayload : struct, IInputPayload where StatePayload : struct, IStatePayload
{
    protected const int BUFFER_SIZE = 1024;

    protected InputPayload[] _inputBuffer = new InputPayload[BUFFER_SIZE];
    protected StatePayload[] _stateBuffer = new StatePayload[BUFFER_SIZE];

    protected Queue<InputPayload> _inputQueue = new Queue<InputPayload>();
    protected Queue<StatePayload> _stateQueue = new Queue<StatePayload>();

    protected InputPayload _processingInput;
    protected StatePayload _reconcileTarget;
    protected int _processingTick = 0;

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
            if (IsServer)
            {
                SendInputClientRpc(_processingInput);
            }
            else
            {
                SendInputServerRpc(_processingInput);
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
        while (_inputQueue.Count > 0)
        {
            _processingInput = _inputQueue.Dequeue();
            ApplyInput(_processingInput);
        }
    }

    protected void GetServerState()
    {
        StatePayload statePayload = GetState();

        statePayload.Tick = _processingInput.Tick;

        SendStateClientRpc(statePayload);
    }

    public virtual bool GetReconcilePredicate(StatePayload oldState, StatePayload newState)
    {
        return false;
    }

    protected void GetReconcileCondition()
    {
        while (_stateQueue.Count > 0)
        {
            StatePayload statePayload = _stateQueue.Dequeue();

            if (statePayload.Tick < NetworkSyncManager.Instance.LastReconciledTick)
            {
                continue;
            }

            int bufferIndex = statePayload.Tick % BUFFER_SIZE;

            if (_stateBuffer[bufferIndex].Tick != statePayload.Tick)
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
                    _reconcileTarget = statePayload; // ?
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
    private void SendInputServerRpc(InputPayload inputPayload) {
        _inputQueue.Enqueue(inputPayload);
        SendInputClientRpc(inputPayload);
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendInputClientRpc(InputPayload inputPayload)
    {
        int bufferIndex = inputPayload.Tick % BUFFER_SIZE;

        _inputBuffer[bufferIndex] = inputPayload;
    }

    [ClientRpc(RequireOwnership = false)]
    private void SendStateClientRpc(StatePayload statePayload)
    {
        _stateQueue.Enqueue(statePayload);
    }
}
