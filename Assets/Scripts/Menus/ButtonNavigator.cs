using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonNavigator : MonoBehaviour
{
    private Button _button;
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material unselectedMaterial;
    
    private void Start()
    {
        _button = GetComponent<Button>();
        
        _button.image.material = unselectedMaterial;
        
        // Debug.Log($"Down = {_button.navigation.selectOnLeft}, Up = {_button.navigation.selectOnUp}, " +
        //           $"Left = {_button.navigation.selectOnLeft}, right = {_button.navigation.selectOnRight}");
        //
        // // _button.navigation = Navigation.Mode.Explicit;
    }

    public void OnSelect(BaseEventData eventData)
    {
        
    }

    private void Update()
    {
        _button.image.material = EventSystem.current.currentSelectedGameObject == _button.gameObject ?
            selectedMaterial : unselectedMaterial;
    }
}
