using System;
using System.Collections.Generic;
using UnityEngine;
using EditorAttributes;

public class Section_Content : MonoBehaviour, IAffectedByRiver
{
    [Header("Data")]
    [SerializeField] private River_Manager riverManager;
    [SerializeField] private GlobalRiverValues globalRiverValues;

    [Serializable]
    public class SectionData
    {
        public float initialDelay = 0f;
        public float postDelay = 0f;

        public int ObjectCount => SectionBuilderDatas.Count;
        public List<ISection_Data> SectionBuilderDatas = new();

        [Line(GUIColor.Cyan)]
        public List<Section_Obstacle_Object> ObstacleDatas = new();
        [Line(GUIColor.Red)]
        public List<Section_Enemy_Object> EnemyDatas = new();
        [Line(GUIColor.Yellow)]
        public List<Section_Collectible_Object> CollectibleDatas = new();

        [Line(GUIColor.White)]
        public List<Section_Gemstone_Gate> GemstoneGateDatas = new();
    }
    public SectionData sectionData = new();

    // [EditorAttributes.Button]
    public void GetSectionObjects()
    {
        sectionData.SectionBuilderDatas.Clear();

        sectionData.ObstacleDatas.Clear();
        sectionData.EnemyDatas.Clear();
        sectionData.CollectibleDatas.Clear();
        sectionData.GemstoneGateDatas.Clear();

        foreach (var sbo in GetComponentsInChildren<Section_Builder_Object>())
        {
            sbo.Register(this);
            if (!globalRiverValues || !riverManager)
            {
                Debug.LogWarning("Missing Global River Values or River Manager");
            }
            sbo.InjectRiverManager(globalRiverValues, riverManager);
        }
    }

    public void InjectRiverManager(River_Manager manager)
    {
        riverManager = manager;
    }

    [Header("Debug")]
    [SerializeField] private bool enableDebug;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        GetSectionObjects();

        if (!enableDebug) return;

        foreach (var item in sectionData.ObstacleDatas) item.DrawGizmos();
        foreach (var item in sectionData.EnemyDatas) item.DrawGizmos();
        foreach (var item in sectionData.CollectibleDatas) item.DrawGizmos();
        foreach (var item in sectionData.GemstoneGateDatas) item.DrawGizmos();
    }
#endif
}
