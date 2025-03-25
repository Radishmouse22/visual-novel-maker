using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProjectDetails : MonoBehaviour
{
    public TMP_Text nameplate, errorPrintout;
    public Image starButtonImage;
    public Sprite starred, unstarred;
    public Button playButton;
    public int playerBuildIndex;

    DirectoryInfo Dir => Singleton.workingProjectDirectory;
    Novel n;
    List<string> errors;
    bool isStarred;

    public void Refresh()
    {
        nameplate.text = Dir.Name;
        n = new();
        errors = new();
        Singleton.ReadStarredProjectFile(out _);
        isStarred = Singleton.starredProject == Dir.Name;
        starButtonImage.sprite = isStarred ? starred : unstarred;
        Interpreter.InterpretProject(Dir, n, errors, false);
        errorPrintout.text = string.Join('\n', errors);
        playButton.interactable = errors.Count == 0;
    }

    public void Star()
    {
        Singleton.starredProject = isStarred ? "" : Dir.Name;
        Singleton.WriteStarredProjectFile();
        isStarred = !isStarred;
        starButtonImage.sprite = isStarred ? starred : unstarred;
    }

    public void TestProject()
    {
        Singleton.TEST_PROJECT_NAME = Dir.Name;
        SceneManager.LoadScene(playerBuildIndex);
    }
}

public class ProjectSettings
{
    public Color main = Color.white;
}
