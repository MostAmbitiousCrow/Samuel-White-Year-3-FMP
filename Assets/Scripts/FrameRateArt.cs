using UnityEngine;
using static Animation_Frame_Rate_Manager;

public class FrameRateArt : MonoBehaviour
{
    [SerializeField] Transform _artRoot;

    private void OnEnable()
    {
        OnTick += UpdateArtRoot;

        transform.parent = null;
        transform.position = _artRoot.position;
    }

    private void OnDisable()
    {
        OnTick -= UpdateArtRoot;
    }

    void UpdateArtRoot(object sender, OnTickEvent e)
    {
        transform.position = _artRoot.position;
    }
}
