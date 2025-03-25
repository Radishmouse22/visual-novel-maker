using UnityEngine;
using System.IO;
using TMPro;

public class Test : MonoBehaviour
{
    public TMP_Text output;

    void Start()
    {
        var info = new DirectoryInfo(Application.streamingAssetsPath);
        var fileInfo = info.GetFiles();
        foreach (var file in fileInfo)
        {
            output.text += file.Name + '\n';
        }
    }
}
