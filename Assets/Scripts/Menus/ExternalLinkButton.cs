using Game;
using UnityEngine;

public class ExternalLinkButton : MonoBehaviour
{
    [SerializeField] private string linkURL;

    public void OpenLink()
    {
        if (GameSettingsManager.AllowExternalLinks) Application.OpenURL(linkURL);
        else Debug.LogWarning("Tried to load Link URL but external links are disabled in the Games Settings Manager");
    }
}
