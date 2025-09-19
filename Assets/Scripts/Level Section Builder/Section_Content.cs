using System;
using System.Collections.Generic;
using UnityEngine;

public class Section_Content : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] River_Manager riverManager;
    
    [Serializable]
    public class SectionData
    {
        public float initialDelay = 0f;
        public float postDelay = 0f;

        public int ObjectCount => SectionBuilderDatas.Count;
        public List<ISection_Data> SectionBuilderDatas = new();

        public List<Section_Obstacle_Object> ObstacleDatas = new();
        public List<Section_Enemy_Object> EnemyDatas = new();
        public List<Section_Collectible_Object> CollectibleDatas = new();
    }
    public SectionData sectionData = new();

    [EditorAttributes.Button]
    public void GetSectionObjects()
    {
        sectionData.SectionBuilderDatas.Clear();

        sectionData.ObstacleDatas.Clear();
        sectionData.EnemyDatas.Clear();
        sectionData.CollectibleDatas.Clear();

        foreach (var sbo in GetComponentsInChildren<Section_Builder_Object>())
            sbo.Register(this);
    }
}
