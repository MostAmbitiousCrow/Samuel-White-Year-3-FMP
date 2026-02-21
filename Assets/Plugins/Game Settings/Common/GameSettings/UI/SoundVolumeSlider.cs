using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
	[AddComponentMenu("GameSettings/UI/Sound Volume Slider")]
	public class SoundVolumeSlider : MonoBehaviour
	{
		[SerializeField] private Slider targetElement;
		[SerializeField] private SoundVolumeType volumeType = SoundVolumeType.Main;
		[SerializeField] private TextMeshProUGUI volumeTypeText;

        // ------------------------------------------------------------------------------------------------------------

        private void Reset()
		{
			targetElement = GetComponentInChildren<Slider>();
		}

		private void Start()
		{
			if (targetElement == null)
			{
				targetElement = GetComponentInChildren<Slider>();
				if (targetElement == null)
				{
					Debug.Log("[SoundVolumeSlider] Could not find any Slider component on the GameObject.", gameObject);
					return;
				}
			}

			if (volumeTypeText == null)
            {
                volumeTypeText = GetComponentInChildren<TextMeshProUGUI>();
                if (volumeTypeText == null)
                {
                    Debug.Log("[SoundVolumeSlider] Could not find any TextMeshProUGUI component on the GameObject.", gameObject);
                    return;
                }
            }

            targetElement.value = GameSettingsManager.GetSoundVolume(volumeType);
			UpdateText(targetElement.value);
            targetElement.onValueChanged.AddListener(OnValueChange);
		}

		private void OnValueChange(float value)
		{
			GameSettingsManager.SetSoundVolume(volumeType, value);
            UpdateText(value);
        }

		private void UpdateText(float value)
		{
            var newValue = Mathf.Round(value * 1000f) / 10f;
            volumeTypeText.SetText(newValue.ToString() + "%");
        }

		// ------------------------------------------------------------------------------------------------------------
	}

}
