using Game;
using UnityEngine;

public class ExternalLinkButton : MonoBehaviour
{
    [SerializeField] private string linkURL;

    public void OpenLink()
    {
        if (GameSettingsManager.AllowExternalLinks) Application.OpenURL(linkURL);
    }
}
