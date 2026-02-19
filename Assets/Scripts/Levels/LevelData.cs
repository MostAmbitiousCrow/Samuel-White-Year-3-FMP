using System.Collections.Generic;
using EditorAttributes;
using UnityEngine;
using UnityEngine.Splines;

public class LevelData : MonoBehaviour
{
    private Game_Section_Manager _sectionManager;
    [SerializeField] private List<Section_Content.SectionData> sections;
    public List<Section_Content.SectionData> Sections => sections;
    [SerializeField] private SplineContainer levelSplineContainer;
    public SplineContainer LevelSplineContainer => levelSplineContainer;
    
    [Button]
    public void GetSections()
    {
        sections.Clear();
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out Section_Content content))
            {
                sections.Add(content.sectionData);
            }
        }
    }

    private void Start()
    {
        LocateGameSectionManager();
    }

    private void LocateGameSectionManager()
    {
        _sectionManager = FindFirstObjectByType<Game_Section_Manager>();
        
        if (_sectionManager == null) Debug.LogError("No Game Section Manager was found!");
        
        _sectionManager.AssignNewLevelData(this);
    }
}
