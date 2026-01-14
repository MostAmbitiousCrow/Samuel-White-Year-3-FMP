using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class PooledObject : MonoTimeBehaviour
{

    // TODO: Look into kPooling you added to your package and implement it here!
    public virtual void OnSpawned()
    {

    }

    public virtual void OnRecycled()
    {

    }
}
