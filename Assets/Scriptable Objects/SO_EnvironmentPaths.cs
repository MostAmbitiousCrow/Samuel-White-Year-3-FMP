using System;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "EnvironmentPaths", menuName = "ScriptableObjects/EnvironmentPaths")]
public class SO_EnvironmentPaths : ScriptableObject
{
    [FormerlySerializedAs("environmentPaths")] public EnvironmentPath[] paths;
    
    [Serializable]
    public struct EnvironmentPath
    {
        /// <summary> The root environment to branch from </summary>
        [Tooltip("The root environment to branch from")]
        public Environments root;
        
        /// <summary> An image of the environment </summary>
        [AssetPreview]
        public Sprite photo;
        
        /// <summary> The environments the root environment connect to </summary>
        [Tooltip("The environments the root environment connect to")]
        public Environments[] branches;
    }
}
