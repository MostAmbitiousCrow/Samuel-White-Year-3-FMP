using UnityEngine;
using GameColours;

/// <summary>
/// The scriptable object that references the colours for the game to use.
/// </summary>
[CreateAssetMenu(fileName = "GameColours", menuName = "ScriptableObjects/GameSettings/GameColours", order = 1)]
public class SO_GameColours : ScriptableObject
{
    [SerializeField] public ObjectMaterialColours[] MaterialColours = new ObjectMaterialColours[]
    {
        // Global
        new ObjectMaterialColours()
        {
            Name = "Global",
            HighlightColour = Color.white,
            MidtoneColour = Color.gray,
            ShadowColour = Color.black
        },
        
        // Player
        new ObjectMaterialColours()
        {
            Name = "Player",
            HighlightColour = Color.green,
            MidtoneColour = Color.green,
            ShadowColour = Color.green
        },
        
        // Enemies
        new ObjectMaterialColours()
        {
            Name = "Enemies",
            HighlightColour = Color.red,
            MidtoneColour = Color.red,
            ShadowColour = Color.red,
        },
        
        // Obstacles
        new ObjectMaterialColours()
        {
            Name = "Obstacles",
            HighlightColour = Color.yellow,
            MidtoneColour = Color.yellow,
            ShadowColour = Color.yellow,
        },
        
        // Collectibles
        new ObjectMaterialColours()
        {
            Name = "Collectibles",
            HighlightColour = Color.blue,
            MidtoneColour = Color.blue,
            ShadowColour = Color.blue,
        },
        
        // Environment
        new ObjectMaterialColours()
        {
            Name = "Environment",
            HighlightColour = Color.white,
            MidtoneColour = Color.gray,
            ShadowColour = Color.black
        },
        
        // UI
        new ObjectMaterialColours()
        {
            Name = "UI",
            HighlightColour = Color.white,
            MidtoneColour = Color.gray,
            ShadowColour = Color.black
        }
    };
}