using System.IO;
using UnityEngine;
using TMPro;

public class ProjectListing : MonoBehaviour
{
    [HideInInspector] public DirectoryInfo dir = null;
    [HideInInspector] public string errors;

    public void Init(DirectoryInfo dir, string errors)
    {
        this.dir = dir;
        this.errors = errors;
        string name = dir.Name;

        if (errors.Length > 0)
            name += '*';
        gameObject.GetComponentInChildren<TMP_Text>().text = name;
    }

    // called from button
    public void Selected()
    {
        Singleton.Instance.projectSelector.Selected(this);
    }
}
