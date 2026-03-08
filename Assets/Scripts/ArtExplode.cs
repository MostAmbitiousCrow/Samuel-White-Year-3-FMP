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
    [SerializeField] private float force = 10f;
    [SerializeField, MinMaxSlider(-5f, 5f, true)] private Vector2 minMaxAngularVelocity = new Vector2(-5f, 5f);
    [Space]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (art.Length <= 0) return;
        foreach (var r in art) artPositions.Add(r.transform.localPosition);
    }

    private void OnEnable()
    {
        if(animator) animator.enabled = true;

        for (int i = 0; i < art.Length; i++)
        {
            art[i].isKinematic = true;
            art[i].transform.localPosition = artPositions[i];
            art[i].transform.localRotation = Quaternion.identity;
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

    // private Vector3 CalculateCenter()
    // {
    //     var totalX = 0f;
    //     var totalY = 0f;
    //     var totalZ = 0f;
    //     
    //     foreach (var item in art)
    //     {
    //         totalX += item.position.x;
    //         totalY += item.position.y;
    //         totalZ += item.position.z;
    //     }
    //     
    //     var centerX = totalX / art.Length;
    //     var centerY = totalY / art.Length;
    //     var centerZ = totalZ / art.Length;
    //     
    //     var center = new Vector3(centerX, centerY, centerZ);
    //     
    //     return  center;
    // }

    private Vector3 GetRandomRotation()
    {
        var x = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        var y = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        var z = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        
        return  new Vector3(x, y, z);
    }
}
