using System;
using EditorAttributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Environment_Select
{
    public class EnvironmentSelectUI : MonoBehaviour
    {
        private static readonly int Selected = Animator.StringToHash("Selected");
        private static readonly int Normal = Animator.StringToHash("Normal");
        
        [SerializeField] private Image directionalIcon, checkmark;
        public Image DirectionalIcon => directionalIcon;
        [SerializeField] private Animator animator;
        [SerializeField] private TextMeshProUGUI buttonText;
        [SerializeField, ReadOnly] private Environments environment;

        private void Awake()
        {
            // button.onClick.AddListener(SelectEnvironment);
            checkmark.gameObject.SetActive(false);
        }

        public void UpdateSelectDetails(SO_EnvironmentPaths.EnvironmentPath data)
        {
            buttonText.text = data.root.ToString();
            environment = data.root;
            
            if (GameLevelManager.CheckEnvironmentCompleted(data.root)) checkmark.gameObject.SetActive(true);
        }
        
        private bool _isSelected;

        public void SelectEnvironment()
        {
            // GameLevelManager.Instance.LoadEnvironmentAndLevel(environment);
            // Game_UI.Instance.CloseEnvironmentSelect();
            if (_isSelected) return;
            animator.ResetTrigger(Normal);
            animator.SetTrigger(Selected);

            _isSelected = true;
        }

        public void DeSelectEnvironment()
        {
            animator.ResetTrigger(Selected);
            animator.SetTrigger(Normal);
            
            _isSelected = false;
        }
    }
}
