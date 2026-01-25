using System;
using EditorAttributes;
using UnityEngine;

public class GameMaterialsManager : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material[] materials = new Material[8];
    [Space]
    [SerializeField] private SO_GameColours currentColours;
    public SO_GameColours CurrentColours => currentColours;
    [SerializeField] private SO_GameColours defaultColours;
    
    [Header("Special")]
    [SerializeField] private bool enableRainbowMode;
    public bool EnableRainbowMode => enableRainbowMode;

    // Cached parameter names
    private const string HighlightString = "_NewHighlight";
    private const string MidtoneString = "_NewMidtone";
    private const string ShadowString = "_NewShadow";

    public enum ObjectTypes
    {
        Global, Player, Enemy, Obstacle, Collectible, Boat, Environment, UI
    }
    
    [Header("Detection Colours")]
    public ObjectMaterialColours DetectColours = new ObjectMaterialColours()
    {
        HighlightColour = Color.white,
        MidtoneColour = Color.gray,
        ShadowColour = Color.black
    };

    [Button]
    public void ResetColours()
    {
        UpdateMaterials(defaultColours);
    }

    public void UpdateMaterials(SO_GameColours colours)
    {
        currentColours = colours;
        ObjectMaterialColours col;

        for (var i = 0; i < colours.MaterialColours.Length; i++)
        {
            UpdateMaterial(i, colours.MaterialColours[i]);
        }
        print(colours.MaterialColours.Length);
    }

    public void UpdateMaterial(ObjectTypes objectType, ObjectMaterialColours colour)
    {
        var i = (int)objectType;
        
        materials[i].SetColor(HighlightString, colour.HighlightColour);
        materials[i].SetColor(MidtoneString, colour.MidtoneColour);
        materials[i].SetColor(ShadowString, colour.ShadowColour);
        
        Debug.Log($"Updated Material {i}");
    }

    public void UpdateMaterial(int id, ObjectMaterialColours colour)
    {
        materials[id].SetColor(HighlightString, colour.HighlightColour);
        materials[id].SetColor(MidtoneString, colour.MidtoneColour);
        materials[id].SetColor(ShadowString, colour.ShadowColour);
        
        Debug.Log($"Updated Material {id}");
    }

    public void UpdateMaterial(Material mat, ObjectMaterialColours colour)
    {
        mat.SetColor(HighlightString, colour.HighlightColour);
        mat.SetColor(MidtoneString, colour.MidtoneColour);
        mat.SetColor(ShadowString, colour.ShadowColour);
    }

    #region Rainbow Mode

    [Button]
    public void ToggleRainbowMode()
    {
        enableRainbowMode = !enableRainbowMode;
        ResetColours();
    }
        
    private void Update()
    {
        // Fix: Only run if the game is ACTIVE and Rainbow Mode is ON
        if (!enableRainbowMode) return;
    
        CycleRainbow();
    }

    private void CycleRainbow()
    {
        // Use Time.time to get a continuously increasing value for the hue
        var hueOffset = (Time.time * 0.25f) % 1f;

        for (var i = 0; i < materials.Length; i++)
        {
            var mat = materials[i];
        
            // Reference colour from offset
            var baseCol = defaultColours.MaterialColours[i];
        
            // Must change the colour values INSIDE the class. ffs.
            var rainbowCol = new ObjectMaterialColours
            {
                HighlightColour = ShiftHue(baseCol.HighlightColour, hueOffset),
                MidtoneColour = ShiftHue(baseCol.MidtoneColour, hueOffset),
                ShadowColour = ShiftHue(baseCol.ShadowColour, hueOffset)
            };
        
            UpdateMaterial(mat, rainbowCol);
        }
    }
    
    private Color ShiftHue(Color original, float offset)
    {
        Color.RGBToHSV(original, out var h, out var s, out var v);
        // Add the offset and use % 1f to wrap around from 1.0 back to 0.0 // (I did not write this D: )
        return Color.HSVToRGB((h + offset) % 1f, s, v);
    }

    #endregion


}

[Serializable]
public class ObjectMaterialColours
{
    [ReadOnly] public string Name = "Material Name";
    public Color ShadowColour = Color.black;
    public Color MidtoneColour = Color.gray;
    public Color HighlightColour = Color.white;
}