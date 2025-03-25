using System.Collections.Generic;
using UnityEngine;

public class ImageManager : MonoBehaviour
{
    public GameObject imagePrefab;
    public Transform imageParent;
    public RectTransform canvas;
    readonly Dictionary<string, GameImage> imageDictionary = new();
    readonly Queue<GameImage> availableImages = new();

    public void Show(string fullName, HorizontalImagePosition h, VerticalImagePosition v)
    {
        GameImage imageToSet;

        // replace an existing image in the same group if one is already displayed
        string group = GetGroup(fullName);
        if (imageDictionary.TryGetValue(group, out GameImage i))
            imageToSet = i;

        // OR get one from the queue, if one is available
        else if (availableImages.Count > 0)
            imageToSet = availableImages.Dequeue();

        // OR, *sigh*, we have to create one
        else
            imageToSet = Instantiate(imagePrefab, imageParent).GetComponent<GameImage>();

        imageToSet.Set(VisualNovelPlayer.n.images[fullName], GetScreenPosition(h, v));
        imageDictionary[group] = imageToSet;
    }

    string GetGroup(string fullName) => fullName.Contains(' ') ? fullName.Split(' ', 2)[0] : fullName;
    Vector2 GetScreenPosition(HorizontalImagePosition h, VerticalImagePosition v) =>
        new(
            (float)((int)h+1)/(5+1)*canvas.sizeDelta.x,
            (float)((int)v+1)/(5+1)*canvas.sizeDelta.y
        );

    public void Hide(string group)
    {
        if (!imageDictionary.TryGetValue(group, out GameImage i))
            return;

        i.Hide();
        imageDictionary.Remove(group);
        availableImages.Enqueue(i);
    }

    public void Clear()
    {
        foreach (GameImage i in imageDictionary.Values)
        {
            i.Hide();
            availableImages.Enqueue(i);
        }
        imageDictionary.Clear();
    }

    public void DestroyAll()
    {
        foreach (GameImage i in imageDictionary.Values)
            Destroy(i.gameObject);
        while (availableImages.Count > 0)
            Destroy(availableImages.Dequeue().gameObject);

        imageDictionary.Clear();
        availableImages.Clear();
    }
}
