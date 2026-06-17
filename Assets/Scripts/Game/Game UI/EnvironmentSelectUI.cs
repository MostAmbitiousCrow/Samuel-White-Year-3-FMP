using System;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Environment_Select
{
    public class EnvironmentSelectUI : MonoBehaviour
    {
        [SerializeField] private Image environmentIcon, checkmark;
        [SerializeField] private Button button;
        public Button Button => button;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField, ReadOnly] private Environments environment;

        private void Awake()
        {
            button.onClick.AddListener(SelectEnvironment);
            checkmark.gameObject.SetActive(false);
        }

        public void UpdateSelectDetails(SO_EnvironmentPaths.EnvironmentPath data)
        {
            buttonText.text = data.root.ToString();
            if (data.photo) environmentIcon.sprite = data.photo;
            environment = data.root;
            
            if (GameLevelManager.CheckEnvironmentCompleted(data.root)) checkmark.gameObject.SetActive(true);
        }

        public void SelectEnvironment()
        {
            GameLevelManager.Instance.LoadEnvironmentAndLevel(environment);
            Game_UI.Instance.CloseEnvironmentSelect();
        }
    }
}
