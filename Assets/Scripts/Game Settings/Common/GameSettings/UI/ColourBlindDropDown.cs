using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using EditorAttributes;
using GameColours;

namespace Game
{
	[AddComponentMenu("GameSettings/UI/Colourblind DropDown")]
	public class ColourBlindDropDown : MonoBehaviour
	{
		[Title("Colourblind options is applied once the game starts", 15)]
		[SerializeField] private TMP_Dropdown targetElement;

		// ------------------------------------------------------------------------------------------------------------

		private void Reset()
		{
			targetElement = GetComponentInChildren<TMP_Dropdown>();
		}

		private void Awake()
		{
			if (targetElement) return;
			
			targetElement = GetComponentInChildren<TMP_Dropdown>();
			if (targetElement) return;
			Debug.Log("[ColourBlindDropDown] Could not find any TextMeshPro Dropdown component on the GameObject.", gameObject);
		}
		
		private void OnEnable()
		{
			RefreshControl();
		}

		private void RefreshControl()
		{
			if (!targetElement) return;
			
			targetElement.ClearOptions();
			targetElement.onValueChanged.RemoveAllListeners();

			List<TMP_Dropdown.OptionData> opts = new List<TMP_Dropdown.OptionData>();
			var types = Enum.GetNames(typeof(GameSettingsManager.ColourblindType));
			foreach (string s in types)
			{
				opts.Add(new TMP_Dropdown.OptionData(s));
			}

			targetElement.AddOptions(opts);
			targetElement.value = (int)GameSettingsManager.CurrentColourblindMode;
			targetElement.onValueChanged.AddListener(OnValueChange);
		}

		private void OnValueChange(int idx)
		{
			GameSettingsManager.CurrentColourblindMode = (GameSettingsManager.ColourblindType)idx;
			GameColoursManager.ResetColours();
			Debug.Log($"Set Colourblind mode to {GameSettingsManager.CurrentColourblindMode}");
		}

		// ------------------------------------------------------------------------------------------------------------
	}

}
