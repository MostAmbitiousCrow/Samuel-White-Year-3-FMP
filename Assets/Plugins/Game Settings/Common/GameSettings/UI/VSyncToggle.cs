using UnityEngine;
using UnityEngine.UI;

namespace Game
{
	[AddComponentMenu("GameSettings/UI/Vsync Toggle")]
	public class VSyncActiveToggle : MonoBehaviour
	{
		[SerializeField] private Toggle targetElement;

		// ------------------------------------------------------------------------------------------------------------

		private void Reset()
		{
			targetElement = GetComponentInChildren<Toggle>();
		}

		private void Start()
		{
			if (targetElement == null)
			{
				targetElement = GetComponentInChildren<Toggle>();
				if (targetElement == null)
				{
					Debug.Log("[VSync Toggle] Could not find any Toggle component on the GameObject.", gameObject);
					return;
				}
			}

			targetElement.isOn = GameSettingsManager.VSyncActive;
			targetElement.onValueChanged.AddListener(OnValueChange);
		}

		private void OnValueChange(bool isOn)
		{
			GameSettingsManager.SetVSync(isOn);
		}

		// ------------------------------------------------------------------------------------------------------------
	}

}
