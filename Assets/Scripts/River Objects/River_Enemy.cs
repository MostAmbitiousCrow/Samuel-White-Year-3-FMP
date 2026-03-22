using EditorAttributes;
using UnityEngine;

public class River_Enemy : River_Object
{
    [Line(GUIColor.Red, 1, 3)]
    [Header("Enemy Stats")]
    [SerializeField] float emergeTriggerDetectRadius = 3f;
    public BoatEnemy_Data EnemyData { get; private set; }
    
    [SerializeField] BoatEnemyStateController enemyController;

    private Transform _targetSide;

    [Header("Components")] 
    [SerializeField] private GameObject art;
    [SerializeField] private ParticleSystem emergingPs, emergedPs;

    public void OverrideData(BoatEnemy_Data data)
    {
        EnemyData = data;

        enemyController.InitialiseEnemy(data);

        // TODO Override Health!
        // print($"{name} stats were overrided");
    }

    private void OnEnable()
    {
        if (!enemyController) return;
        enemyController.SetDirection(enemyController.boatEnterData.startFacingDirection, false);
        enemyController.gameObject.SetActive(false); //TODO: Adjust this for bat enemies who will spawn with their enemy active
        
        // Set the target side for the Enemy Silhouette to follow
        var space = Boat_Space_Manager.Instance.GetSideSpace
            (enemyController.boatEnterData.targetSideSpace, enemyController.boatEnterData.targetLeftSide);
        _targetSide = space.t;
        
        art.SetActive(true);
        emergingPs.Play();
    }

    protected override void TimeUpdate()
    {
        base.TimeUpdate();

        if (!isMoving) return;
        // Debug.Log($"Enemy is Moving");
        
        if (!(GetDistanceToBoat() < emergeTriggerDetectRadius)) return;
        // Debug.Log($"Enemy has detected boat");
        
        // if (_targetSide) transform.position = _targetSide.position;
        isMoving = false;
        enemyController.gameObject.SetActive(true);
        enemyController.EmergeFromRiver();
            
        art.SetActive(false);
        emergingPs.Stop();
        emergedPs.Play();
    }

    #region Pooling Methods

    public override void OnSpawned()
    {
        base.OnSpawned();
    }

    #endregion


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, emergeTriggerDetectRadius);
    }
}