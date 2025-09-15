using UnityEngine;

public class AnimationFrameRateTestScript : MonoBehaviour
{
    public void SetFrameRate(int frameRate)
    {
        Animation_Frame_Rate_Manager.SetAnimationFrameRate(frameRate);
    }
}
