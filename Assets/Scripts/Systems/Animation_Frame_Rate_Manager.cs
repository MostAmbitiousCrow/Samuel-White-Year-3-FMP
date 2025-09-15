using System;
using UnityEngine;

public class Animation_Frame_Rate_Manager : MonoBehaviour
{
    public static int AnimationFramerate = 24;
    private const int DefaultAnimationFrameRate = 24;

    /// <summary>
    /// Sets the target frame rate for animations.
    /// </summary>
    /// <param name="frameRate">The target frame rate.</param>
    public static void SetAnimationFrameRate(int frameRate) => AnimationFramerate = frameRate;

    public static void ResetAnimationFrameRate() => AnimationFramerate = DefaultAnimationFrameRate;

    public static float GetDeltaAnimationFrameRate() { return 1f / AnimationFramerate; }

    #region Timer
    private float timer;
    private int tick;

    public class OnTickEvent : EventArgs
    {
        public int tick;
    }

    public static event EventHandler<OnTickEvent> OnTick;


    void Awake()
    {
        tick = 0;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= GetDeltaAnimationFrameRate())
        {
            timer -= GetDeltaAnimationFrameRate(); // reset but keep excess value (setting to zero will cause an offset)
            tick++;
            OnTick?.Invoke(this, new OnTickEvent { tick = tick });
        }
    }
    #endregion
}
