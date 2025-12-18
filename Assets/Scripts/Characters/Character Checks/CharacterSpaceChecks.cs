using System.Collections.Generic;
using UnityEngine;

public static class CharacterSpaceChecks
{
    /// <summary> Scans an area for a character based on a given position and box shape </summary>
    public static CharacterStateController ScanAreaForDamageableCharacter(Vector3 checkPosition, Vector3 boxSize, Quaternion checkRotation, LayerMask layerMask, bool returnClosest = false, bool log = false)
    {
        Collider[] hits = Physics.OverlapBox(checkPosition, boxSize * 0.5f, checkRotation, layerMask);

        if (hits.Length > 0)
        {
            if (returnClosest)
            {
                float distance = Mathf.Infinity;
                Collider closestItem = null;

                foreach (var item in hits)
                {
                    float d = Vector3.Distance(item.transform.position, checkPosition);
                    if (d < distance) // update if this hit item is closer
                    {
                        closestItem = item;
                        distance = d;
                    }
                }

                if (closestItem != null)
                {
                    if (log) Debug.Log($"Hit {closestItem.name} at {checkPosition}");
                    return closestItem.GetComponent<CharacterStateController>();
                }

                return null;
            }
            else
            {
                if (log) Debug.Log($"Hit {hits[0].name} at {checkPosition}");
                return hits[0].GetComponent<CharacterStateController>();
            }
        }
        else
        {
            if (log) Debug.Log($"No hit at: {checkPosition}");
            return null;
        }
    }

    /// <summary> Scans an area for a charactera based on a given position and box shape </summary>
    public static Boat_Character[] ScanAreaForDamageableCharacters(Vector3 checkPosition, Vector3 boxSize, Quaternion checkRotation, LayerMask layerMask, bool log = false)
    {
        Collider[] hits = Physics.OverlapBox(checkPosition, boxSize * 0.5f, checkRotation, layerMask);

        if (hits.Length > 0)
        {
            List<Boat_Character> d = new();

            foreach (var item in hits)
            {
                d.Add(item.GetComponent<Boat_Character>());
            }

            if (log) Debug.Log($"Hit {hits.Length} characters at {checkPosition}");
            return d.ToArray();
        }
        else
        {
            if (log) Debug.Log($"No hits at: {checkPosition}");
            return null;
        }
    }

    //public static void DebugArea(Vector3 checkPosition, Vector3 boxSize, Quaternion checkRotation, float duration = 1f)
    //{
    //    StartCoroutine(ScanAreaRoutine(checkPosition, boxSize, checkRotation, duration));
    //}

    //static IEnumerator ScanAreaRoutine(Vector3 checkPosition, Vector3 boxSize, Quaternion checkRotation, float duration)
    //{
    //    float t = duration;
    //    WaitUntil wait = new(() => t > duration);

    //    while (t > duration)
    //    {

    //    }
    //    yield break;
    //}
}
