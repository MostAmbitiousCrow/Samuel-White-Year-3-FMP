using System;
using System.Collections.Generic;
using CameraShake;
using CarterGames.Assets.AudioManager;
using UnityEngine;
using EditorAttributes;
using Random = UnityEngine.Random;

public class ArtExplode : MonoBehaviour
{
    [Title("Art Explode")]
    [SerializeField] private Rigidbody[] art;

    [SerializeField] private List<Vector3> artPositions = new List<Vector3>();
    [SerializeField] private List<Quaternion> artRotations = new List<Quaternion>();
    [SerializeField] private float force = 10f;
    [SerializeField, MinMaxSlider(-5f, 5f, true)] private Vector2 minMaxAngularVelocity = new Vector2(-5f, 5f);
    [Space]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (art.Length <= 0) return;
        
        foreach (var r in art) 
        {
            artPositions.Add(r.transform.localPosition);
            artRotations.Add(r.transform.localRotation);
        }
    }

    private void OnEnable()
    {
        ResetArtPositions();
        if(animator) animator.enabled = true;
    }

    public void ResetArtPositions()
    {
        for (int i = 0; i < art.Length; i++)
        {
            var artObj = art[i]; 
            artObj.isKinematic = true;
            artObj.transform.localPosition = artPositions[i];
            artObj.transform.localRotation = artRotations[i];
            
            // Debug.Log($"{artObj.name} local pos = {artObj.transform.localPosition}. Original = {artPositions[i]}");
        }


        Debug.Log($"{gameObject} Art Reset");
    }

    public void ExplodeArt()
    {
        if(animator) animator.enabled = false;
        // var center = CalculateCenter();
        foreach (var r in art)
        {
            r.isKinematic = false;
            r.linearVelocity = Vector3.zero;
            r.angularVelocity = GetRandomRotation();
        
            r.AddExplosionForce(force, transform.position, 8f, force, ForceMode.Impulse);
        }

        AudioManager.PlayGroup(Group.ExplodeCombined);
        CameraShaker.Presets.Explosion3D(); // TODO: Add a preset for art explosions
        
        // Set timer to freeze the art so it doesn't keep falling out of the world
        Invoke(nameof(FreezeArt), 3f);
    }

    private void FreezeArt()
    {
        foreach (var r in art) r.isKinematic = true;
    }

    private Vector3 GetRandomRotation()
    {
        var x = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        var y = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        var z = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        
        return  new Vector3(x, y, z);
    }
}
