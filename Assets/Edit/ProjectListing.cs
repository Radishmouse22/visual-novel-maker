using System.IO;
using UnityEngine;
using TMPro;

public class ProjectListing : MonoBehaviour
{
    // set in inspector
    public bool newButton = false;
    [HideInInspector] public DirectoryInfo dir;

    public void Init(DirectoryInfo dir)
    {
        this.dir = dir;
        gameObject.GetComponentInChildren<TMP_Text>().text = dir.Name;
    }

    // called from button
    public void Selected() => ProjectEditor.Selected(this);
}
