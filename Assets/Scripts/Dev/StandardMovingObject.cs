using UnityEngine;
using EditorAttributes;

public class StandardMovingObject : MonoTimeBehaviour
{
    private Vector2 direction = Vector2.right;
    [SerializeField, MinMaxSlider(-5f, 5f)] Vector2 _directions;
    private float currentLimit;

    private void Awake()
    {
        currentLimit = _directions.x;
    }

    public override void TimeUpdate()
    {
        if (transform.position.x < _directions.x)
        {
            direction = Vector2.right;
        }
        if (transform.position.x > _directions.y)
        {
            direction = Vector2.left;
        }

        transform.Translate(direction * Time.deltaTime);
    }
}
