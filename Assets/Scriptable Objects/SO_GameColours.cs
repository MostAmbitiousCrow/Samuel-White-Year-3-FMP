using UnityEngine;

/// <summary>
/// The scriptable object that references the colours for the game to use.
/// </summary>
[CreateAssetMenu(fileName = "GameColours", menuName = "ScriptableObjects/GameSettings/GameColours", order = 1)]
public class SO_GameColours : ScriptableObject
{
    [Header("Character/Object Specific Colours")]
    public Color PlayerColour = Color.green;
    public Color EnemyColour = Color.red;
    public Color ObstacleColour = Color.yellow;
    public Color GemstoneColour = Color.blue;

    [Header("Game Colours")]
    public Color ShadowColour = Color.black;
    public Color MidtoneColour = Color.gray;
    public Color HighlightColour = Color.white;
}