using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_Text novelTitle;
    public int editorBuildIndex;

    void Awake()
    {
        novelTitle.text = NovelTesting.name;
    }

    // called from wrench button
    public void OpenEditor()
    {
        SceneManager.LoadScene(editorBuildIndex);
    }
}
