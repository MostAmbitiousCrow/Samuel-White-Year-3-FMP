using UnityEngine;

public class EnvironmentBlock : MonoBehaviour
{
    public Environments environmentType;
    
    [Header("Connection Anchors")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    
    public Transform StartAnchor => startPoint;
    public Transform EndAnchor => endPoint;
    
    public void OnSpawned()
    {
        gameObject.SetActive(true);
    }
    
    public void OnReturned()
    {
        gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (startPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 0.5f);
        }
        
        if (endPoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint.position, 0.5f);
        }
        
        if (startPoint && endPoint)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
        }
    }
#endif
}
