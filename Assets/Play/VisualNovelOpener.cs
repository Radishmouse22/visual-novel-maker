using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.IO;

public class VisualNovelOpener : MonoBehaviour
{
    public PlayerUIManager uiManager;
    public VisualNovelPlayer player;
    public Button playButton;
    public TMP_Text title, errorText;
    public int editorBuildIndex;

    static Novel n;

    void Awake()
    {
        if (Singleton.TEST_PROJECT_NAME != null)
        {
            OpenProject(Singleton.TEST_PROJECT_NAME);
            return;
        }

        // try to open a starred project if there is one
        if (Singleton.ReadStarredProjectFile(out string error))
        {
            OpenProject(Singleton.starredProject);
            return;
        }

        // errorText.text = error;
        // playButton.interactable = false;
        Debug.LogWarning(error);
        OpenEditor();
    }

    void OpenProject(string name)
    {
        title.text = name;

        n = new();
        List<string> errors = new();
        Interpreter.InterpretProject(new(Path.Combine(Application.streamingAssetsPath, name)), n, errors, true);

        // contains errors
        if (errors.Count > 0)
        {
            errorText.text = $"starred project \"{Singleton.starredProject}\" contains errors";
            playButton.interactable = false;
            return;
        }
    }

    // called from play button
    public void Play()
    {
        uiManager.SwitchToPlayer();
        player.Play(n);
    }

    public void OpenEditor()
    {
        SceneManager.LoadScene(editorBuildIndex);
    }
}
