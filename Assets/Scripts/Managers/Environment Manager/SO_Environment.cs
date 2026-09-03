using System;
using UnityEngine;

[CreateAssetMenu(fileName = "SO_Environment", menuName = "Scriptable Objects/Environment")]
public class SO_Environment : ScriptableObject
{
    public Environments environmentType;
    public GameObject[] blocks;

    [Serializable]
    public struct BlockData
    {
        public EnvironmentBlock environmentBlock;
        [Min(0f)] public float appearanceWeight;
    }
}
