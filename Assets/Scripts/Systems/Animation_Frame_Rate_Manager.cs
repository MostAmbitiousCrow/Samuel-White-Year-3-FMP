using System;
using UnityEngine;

public class Animation_Frame_Rate_Manager : MonoTimeBehaviour
{
    public static int AnimationFramerate = 16;
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

    /// <summary>
    /// Custom Yield Instruction
    /// </summary>
    public class WaitForTick : CustomYieldInstruction
    {
        private bool ticked = false;

        public WaitForTick()
        {
            OnTick += OnTickHandler;
        }

        private void OnTickHandler(object sender, OnTickEvent e)
        {
            ticked = true;
            OnTick -= OnTickHandler;
        }

        public override bool keepWaiting
        {
            get { return !ticked; }
        }
    }
}
