using UnityEngine;

public class Audio_Manager : MonoBehaviour
{
    [SerializeField] AudioListener audioListener;

    public static void MuteAudio(bool state)
    {
        AudioListener.pause = state;
    }
    public static void ToggleMuteAudio()
    {
        AudioListener.pause = !AudioListener.pause;
    }
}
