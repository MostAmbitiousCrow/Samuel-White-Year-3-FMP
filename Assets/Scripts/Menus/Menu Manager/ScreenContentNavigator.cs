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

    [SerializeField] protected int startingPage;
    [SerializeField, ReadOnly] protected int currentPage;

    private void Start()
    {
        foreach (PageContent page in pages) page.page.SetActive(false);
        OpenPage(startingPage);
    }

    public void OpenPage(int page)
    {
        pages[currentPage].page.SetActive(false);
        
        currentPage = page;
        pages[currentPage].page.SetActive(true);
        
        EventSystem.current.SetSelectedGameObject(pages[currentPage].enterObject.gameObject);
    }

    private void OnEnable()
    {
        OpenPage(currentPage);
    }
}
