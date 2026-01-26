using System;
using System.Collections;
using UnityEngine;

public abstract class MonoTimeBehaviour : MonoBehaviour
{
    /// <summary> Coroutine suspension supplied with the check for if the game is paused </summary>
    /// TODO: PauseWait should include HitStop
    public WaitUntil PauseWait { get; } = new(() => !GameManager.GameLogic.GamePaused); // Variable should be in the game manager
    private void Update()
    {
        if(!GameManager.GameLogic.GamePaused) TimeUpdate();
    }
    private void FixedUpdate()
    {
        if (!GameManager.GameLogic.GamePaused) FixedTimeUpdate();
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
    public void TriggerHitStop(float stopDuration = .2f)
    {
        // Debug.Log("HitStop Routine Started");
        if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
        _hitStopRoutine = StartCoroutine(HitStopRoutine(stopDuration));
    }

    public void CancelHitStop()
    {
        if (_hitStopRoutine != null) StopCoroutine(_hitStopRoutine);
    }

    private Coroutine _hitStopRoutine;
    private IEnumerator HitStopRoutine(float stopDuration)
    {
        OnHitStop();
        
        if (stopDuration > 0f)
        {
            var t = 0f;
            while (t < stopDuration)
            {
                t += Time.unscaledDeltaTime;
                yield return PauseWait;
            }
        }

        _hitStopRoutine = null;
        
        OnHitStopEnded();
    }

    /// <summary> Is called whenever this characters Hit Stop is triggered </summary>
    protected virtual void OnHitStop()
    {
        // Debug.Log("HitStop Started");
        Time.timeScale = 0f;
    }

    protected virtual void OnHitStopEnded()
    {
        // Debug.Log("HitStop Stopped");
        Time.timeScale = 1f;
    }
    #endregion

    private void OnDestroy()
    {
        OnHitStopEnded();
    }
}
