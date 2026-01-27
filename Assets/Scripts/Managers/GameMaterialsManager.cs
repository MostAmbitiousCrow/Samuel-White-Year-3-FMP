using System;
using EditorAttributes;
using UnityEngine;

public class GameMaterialsManager : MonoBehaviour
{
    // Cached Shader Property IDs
    private static readonly int NewHighlight = Shader.PropertyToID(HighlightString);
    private static readonly int NewMidtone = Shader.PropertyToID(MidtoneString);
    private static readonly int NewShadow = Shader.PropertyToID(ShadowString);

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
    
    public ObjectMaterialColours detectColours = new ObjectMaterialColours()
    {
        HighlightColour = Color.white,
        MidtoneColour = Color.gray,
        ShadowColour = Color.black
    };

    private void Start()
    {
        ResetColours();
        if (enableRainbowMode) ToggleRainbowMode();
        
        GameManager.SceneManager.onLevelLoaded += ResetColours;
    }

    [Button]
    public void ResetColours()
    {
        UpdateMaterials(defaultColours);
    }

    public void UpdateMaterials(SO_GameColours colours)
    {
        currentColours = colours;

        for (var i = 0; i < colours.MaterialColours.Length; i++)
        {
            UpdateMaterial(i, colours.MaterialColours[i]);
        }

        UpdateSkybox(colours.MaterialColours[5].ShadowColour); // Environment Colours
    }

    public void UpdateMaterial(ObjectTypes objectType, ObjectMaterialColours colour)
    {
        var i = (int)objectType;
        
        materials[i].SetColor(NewHighlight, colour.HighlightColour);
        materials[i].SetColor(NewMidtone, colour.MidtoneColour);
        materials[i].SetColor(NewShadow, colour.ShadowColour);
        
        Debug.Log($"Updated Material {i}");
    }

    public void UpdateMaterial(int id, ObjectMaterialColours colour)
    {
        materials[id].SetColor(NewHighlight, colour.HighlightColour);
        materials[id].SetColor(NewMidtone, colour.MidtoneColour);
        materials[id].SetColor(NewShadow, colour.ShadowColour);
        
        Debug.Log($"Updated Material {id}");
    }

    public void UpdateMaterial(Material mat, ObjectMaterialColours colour)
    {
        mat.SetColor(NewHighlight, colour.HighlightColour);
        mat.SetColor(NewMidtone, colour.MidtoneColour);
        mat.SetColor(NewShadow, colour.ShadowColour);
    }

    public void UpdateSkybox(Color colour)
    {
        if (Camera.main != null) Camera.main.backgroundColor = colour;
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

        if (Camera.main != null) UpdateSkybox(ShiftHue(Camera.main.backgroundColor, hueOffset));
    }
    
    private Color ShiftHue(Color original, float offset)
    {
        Color.RGBToHSV(original, out var h, out var s, out var v);
        // Add the offset and use % 1f to wrap around from 1.0 back to 0.0 // (I did not write this D: )
        return Color.HSVToRGB((h + offset) % 1f, s, v);
    }

    #endregion
    
    #region Colour Transition

    // TODO: Implement a routine to transition one colour scheme to another!

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