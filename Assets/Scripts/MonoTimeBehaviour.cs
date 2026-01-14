using UnityEngine;

public abstract class MonoTimeBehaviour : MonoBehaviour
{
    /// <summary>
    /// Coroutine suspension supplied with the check for if the game is paused
    /// </summary>
    public WaitUntil PauseWait { get; } = new(() => !GameManager.GameLogic.GamePaused);
    void Update()
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
}
