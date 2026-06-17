using System;
using UnityEngine;

public class River_SlipStream : River_Object
{
    public SlipStreamData data; //TODO can be private
    public bool IsHit { get; private set; }
    
    [Header("Components")]
    [SerializeField] private ParticleSystem[] particles;
    
    public void OverrideData(SlipStreamData overridedData)
    {
        data = overridedData;
        // print($"{name} stats were overrided");
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Boat")) return;
        
        // Speed up the river (aka the players boat) when they enter the slipstream
        riverManager.SetRiverSpeed(data.speedIncreaseAmount);
        IsHit = true;
    }
    
    public override void OnSpawned()
    {
        base.OnSpawned();
        IsHit = false;

        foreach (var item in particles)
        {
            var main = item.main;
            var speed = 1f * (data.speedIncreaseAmount / 100f);
            var minMax = River_Manager.Instance.minMaxSpeed / 10f;
            var iLerp = Mathf.InverseLerp(minMax.x, minMax.y, speed);
            var lerp = Mathf.Lerp(1f, 2.5f, iLerp);
            main.simulationSpeed = lerp; //TODO: Reset the players health upon level load
        }
    }

    [Serializable]
    public class SlipStreamData
    {
        public int speedIncreaseAmount = 1;
    }
}
