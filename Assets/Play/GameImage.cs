using UnityEngine;
using UnityEngine.UI;

public class GameImage : MonoBehaviour
{
    public Image image;
    public RectTransform rectTransform;

    public void Set(Sprite s, Vector2 position)
    {
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = s.rect.size;
        image.sprite = s;
        image.color = Color.white;
    }

    public void Hide()
    {
        image.color = Color.clear;
    }
}
