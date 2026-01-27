using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;
using Random = UnityEngine.Random;

public class ArtExplode : MonoBehaviour
{
    [Title("Art Explode")]
    [SerializeField] private Rigidbody[] art;

    private readonly List<Vector3> _artPositions = new List<Vector3>();
    [SerializeField] private float force = 10f;
    [SerializeField, MinMaxSlider(-5f, 5f, true)] private Vector2 minMaxAngularVelocity = new Vector2(-5f, 5f);
    [Space]
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (art.Length <= 0) return;
        foreach (var r in art) _artPositions.Add(r.transform.localPosition);
    }

    private void OnEnable()
    {
        if(animator) animator.enabled = true;

        for (int i = 0; i < art.Length; i++)
        {
            art[i].isKinematic = true;
            art[i].transform.localPosition = _artPositions[i];
        }
    }

    public void ExplodeArt()
    {
        if(animator) animator.enabled = false;
        var pos = CalculateCenter();
        foreach (var r in art)
        {
            r.isKinematic = false;
            r.angularVelocity = GetRandomRotation();
            r.AddExplosionForce(force, pos, 2f, force, ForceMode.Impulse);
        }
    }

    private Vector3 CalculateCenter()
    {
        var totalX = 0f;
        var totalY = 0f;
        var totalZ = 0f;
        
        foreach (var item in art)
        {
            totalX += item.position.x;
            totalY += item.position.y;
            totalZ += item.position.z;
        }
        
        var centerX = totalX / art.Length;
        var centerY = totalY / art.Length;
        var centerZ = totalZ / art.Length;
        
        var center = new Vector3(centerX, centerY, centerZ);
        // Debug.Log($"{name} exploded artwork at: {center}");
        
        return  center;
    }

    private Vector3 GetRandomRotation()
    {
        var x = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        var y = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        var z = Random.Range(minMaxAngularVelocity.x, minMaxAngularVelocity.y);
        
        return  new Vector3(x, y, z);
    }
}
