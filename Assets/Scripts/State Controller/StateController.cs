using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StateController : MonoBehaviour
{
    #region Variables

    

    #endregion

    IState currentState;

    private void Update() => currentState?.UpdateState();
    private void FixedUpdate() => currentState?.FixedUpdateState();

    public void ChangeState(IState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }
}
