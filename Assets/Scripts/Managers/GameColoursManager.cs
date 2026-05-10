using System;
using System.Collections.Generic;
using EditorAttributes;
using Game;
using UnityEngine;

namespace GameColours
{
    public class GameColoursManager : MonoBehaviour
    {
        // Cached Shader Property IDs
        public static readonly int NewHighlight = Shader.PropertyToID(HighlightString);
        private static readonly int NewMidtone = Shader.PropertyToID(MidtoneString);
        private static readonly int NewShadow = Shader.PropertyToID(ShadowString);

        [Header("Colours")]
        [SerializeField] private MaterialType[] materialTypes;
        public static MaterialType[] MaterialTypes;
        
        [Serializable]
        public class MaterialType
        {
            public ObjectTypes type;
            public Material[] materials;
        }
        
        private static Material[] _materials;
        private static Dictionary<int, Material[]> _objectColours;
        [Space]
        public static SO_GameColours CurrentColours;
        [SerializeField] private SO_GameColours defaultColours;
        private static SO_GameColours _defaultColours;
        
        [Header("Colour Blindness")]
        [SerializeField] private SO_GameColours[] colourBlindColours = new  SO_GameColours[3];
        private static SO_GameColours[] _colourBlindness;
    
        // Cached parameter names
        private const string HighlightString = "_NewHighlight";
        private const string MidtoneString = "_NewMidtone";
        private const string ShadowString = "_NewShadow";

        public static bool IsRainbowModeActive;
    
        public enum ObjectTypes
        {
            Global, Player, Enemy, Obstacle, Collectible, Environment, River, UI
        }

        public static event Action OnGameColoursChanged;
    
        private void Awake()
        {
            if (!CurrentColours)
            {
                _colourBlindness = colourBlindColours;
                _defaultColours = defaultColours;
                CurrentColours = defaultColours;
            }

            _objectColours = new Dictionary<int, Material[]>();
            var count = Enum.GetNames(typeof(ObjectTypes)).Length;
            for (int i = 0; i < count; i++)
            {
                var matType = materialTypes[i];
                _objectColours.Add((int)matType.type, matType.materials);
            }
            MaterialTypes = materialTypes;
            print($"Counted Types = {count}. Actual Count = {_objectColours.Count}");
            
            UpdateColours();

            // GameManager.SceneManager.onLevelLoaded += ResetColours;
        }
    
        private void OnEnable()
        {
            GameSettingsManager.GameplayChanged += UpdateColours;
        }
    
        private void OnDestroy()
        {
            GameSettingsManager.GameplayChanged -= UpdateColours;
        }
    
        public static void UpdateColours()
        {
            if (GameSettingsManager.CurrentColourblindMode
                ==
                GameSettingsManager.ColourblindType.None)
            {
                AssignColours(CurrentColours);
                SetRainbowMode(GameSettingsManager.DoRainbowMode);
            }
            else
            {
                UpdateColourBlindColours();
            }
            
            OnGameColoursChanged?.Invoke();
            
            Debug.Log($"Updated Colours. New Colours = {CurrentColours}");
        }

        [Button]
        public void ResetColours()
        {
            _defaultColours = defaultColours;
            UpdateColours();
        }

        /// <summary> Sets the colour blindness to 'none' and updates the game colours </summary>
        [Button]
        public void ResetColourBlindness()
        {
            GameSettingsManager.SetColourBlindness(0);
            UpdateColours();
        }

        private static void UpdateColourBlindColours()
        {
            AssignColours(_colourBlindness[(int)GameSettingsManager.CurrentColourblindMode]);
        }
    
        private static void AssignColours(SO_GameColours colours)
        {
            CurrentColours = colours;
    
            for (var i = 0; i < colours.MaterialColours.Length; i++)
            {
                // print($"Length = {colours.MaterialColours.Length}. ({i})");
                UpdateMaterials(_objectColours[i], colours.MaterialColours[i]);
            }
    
            // UpdateSkybox(colours.MaterialColours[5].ShadowColour); // Environment Colours
        }
    
        private static void UpdateMaterials(Material[] mat, ObjectMaterialColours colour)
        {
            foreach (var item in mat)
            {
                item.SetColor(NewHighlight, colour.HighlightColour);
                item.SetColor(NewMidtone, colour.MidtoneColour);
                item.SetColor(NewShadow, colour.ShadowColour);
            }
        }
    
        private static void UpdateSkybox(Color colour)
        {
            if (Camera.main) Camera.main.backgroundColor = colour;
        }
    
        #region Rainbow Mode
        public static void SetRainbowMode(bool state)
        {
            IsRainbowModeActive = state;
        }
            
        private void Update()
        {
            // Fix: Only run if Rainbow Mode is ON
            if (!IsRainbowModeActive) return;
        
            CycleRainbow();
        }
    
        private void CycleRainbow()
        {
            // Use Time.time to get a continuously increasing value for the hue
            // Modular to always loop
            var hueOffset = (Time.realtimeSinceStartup * 0.25f) % 1f;
    
            for (var i = 0; i < _objectColours.Count; i++)
            {
                var mats = _objectColours[i];
            
                // Reference colour from offset
                var baseCol = CurrentColours.MaterialColours[i];
            
                // Note: Change the colour values INSIDE the class, otherwise it won't update the colours
                var rainbowCol = new ObjectMaterialColours
                {
                    HighlightColour = ShiftHue(baseCol.HighlightColour, hueOffset),
                    MidtoneColour = ShiftHue(baseCol.MidtoneColour, hueOffset),
                    ShadowColour = ShiftHue(baseCol.ShadowColour, hueOffset)
                };
            
                UpdateMaterials(mats, rainbowCol);
            }
            OnGameColoursChanged?.Invoke();
    
            if (Camera.main) UpdateSkybox(ShiftHue(Camera.main.backgroundColor, hueOffset));
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
        public string Name = "Material Name";
        public Color ShadowColour = Color.black;
        public Color MidtoneColour = Color.gray;
        public Color HighlightColour = Color.white;
    }
}
