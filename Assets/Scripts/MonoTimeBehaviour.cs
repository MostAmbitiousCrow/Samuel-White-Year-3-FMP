using System.Collections;
using UnityEngine;

public abstract class MonoTimeBehaviour : MonoBehaviour
{
    public bool IsHitStopped { get; private set; } = false;
    /// <summary> Coroutine suspension supplied with the check for if the game is paused </summary>
    /// TODO: PauseWait should include HitStop
    public WaitUntil PauseWait { get; } = new(() => !GameManager.GameLogic.GamePaused); // Variable should be in the game manager
    private void Update()
    {
        if(!GameManager.GameLogic.GamePaused || IsHitStopped) TimeUpdate();
    }
    private void FixedUpdate()
    {
        if (!GameManager.GameLogic.GamePaused || IsHitStopped) FixedTimeUpdate();
    }

    /// <summary>
    /// Update function that will only trigger if the game is not paused
    /// </summary>
    protected virtual void TimeUpdate() { }

    /// <summary>
    /// Fixed Update function that will only trigger if the game is not paused
    /// </summary>
    protected virtual void FixedTimeUpdate() { }

    #region HitStop
    public void TriggerHitStop(float stopDuration)
    {
        IsHitStopped = true;
        if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
        _hitStopRoutine = StartCoroutine(HitStopRoutine(stopDuration));

        OnHitStop();
    }

    public void CancelHitStop()
    {
        IsHitStopped = false;
        StopCoroutine(_hitStopRoutine);
    }

    private Coroutine _hitStopRoutine;
    private IEnumerator HitStopRoutine(float stopDuration)
    {
        var t = 0f;
        while (t < stopDuration)
        {
            t += Time.deltaTime;
            yield return PauseWait;
        }
        IsHitStopped = false;
        _hitStopRoutine = null;

        OnHitStopEnded();
    }
    
    /// <summary> Is called whenever this characters Hit Stop is triggered </summary>
    protected virtual void OnHitStop() { }
    
    protected virtual void OnHitStopEnded() { }
    #endregion
}
