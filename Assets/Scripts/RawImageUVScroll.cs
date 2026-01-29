using UnityEngine;
using UnityEngine.UI;
using UVAnimation;

[RequireComponent(typeof(RawImage))]
public class RawImageUVScroll : MonoBehaviour
{
    private Coroutine _routine;
    [SerializeField] private RawImage rawImage;
    [Space]
    [SerializeField] private Vector2 scrollDirection = new Vector2(1, 1);
    [SerializeField] private float scrollSpeed = 1f;

    private void Awake()
    {
        if (!rawImage) rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        rawImage.uvRect = new Rect(UVScrolling.UVScroll(rawImage.rectTransform.sizeDelta, scrollSpeed,
            scrollDirection), rawImage.uvRect.size);
    }
}
