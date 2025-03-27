using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public TMP_Text novelTitle;
    public int editorBuildIndex;

    [Header("Dynamic Images")] 
    public Image speakerNameBackground;
    public Image dialogueBoxBackground;
    public Image menuBackground;

    void Awake()
    {
        novelTitle.text = NovelTesting.name;
    }

    void Start()
    {
        // apply settings
        speakerNameBackground.color = NovelTesting.settings.mainColor;
        dialogueBoxBackground.sprite = NovelTesting.novel.images[NovelTesting.settings.dialogueBGImage];
        dialogueBoxBackground.SetNativeSize();
        dialogueBoxBackground.SetAllDirty();
        menuBackground.sprite = NovelTesting.novel.images[NovelTesting.settings.menuBGImage];
    }

    // called from wrench button
    public void OpenEditor()
    {
        SceneManager.LoadScene(editorBuildIndex);
    }
}
