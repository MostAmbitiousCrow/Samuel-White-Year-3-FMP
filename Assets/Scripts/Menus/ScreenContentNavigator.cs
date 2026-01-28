using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EditorAttributes;

public class ScreenContentNavigator : MonoBehaviour
{
    [Line(GUIColor.Gray)]
    [SerializeField] protected PageContent[] pages;
    [Serializable]
    public class PageContent
    {
        public GameObject page;
        public GameObject enterObject;
    }
    [SerializeField] protected int currentPage = 0;

    public void OpenPage(int page)
    {
        pages[currentPage].page.SetActive(false);
        
        currentPage = page;
        pages[currentPage].page.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(pages[currentPage].enterObject.gameObject);
    }
}
