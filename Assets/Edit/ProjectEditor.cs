using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProjectEditor : MonoBehaviour
{
    public GameObject projectListingPrefab;
    public RectTransform projectListingParent;

    DirectoryInfo projectDir;

    public static ProjectEditor Singleton;
    void Start()
    {
        if (Singleton != null)
        {
            Debug.LogWarning($"Multiple instances of {nameof(ProjectEditor)}");
            Destroy(this);
            return;
        }
        Singleton = this;
        RefreshProjectListing();
        RefreshProjectData();
    }

    // --- PROJECT LIST ---

    // also called from the in-scene refresh button
    public void RefreshProjectListing()
    {
        // replaces all previous listings
        // TODO maybe a more complex solution to avoid having to completely replace all listings?

        // destroy all previous
        for (int i = 1; i < projectListingParent.childCount; i++)
            Destroy(projectListingParent.GetChild(i).gameObject);
        // create listings, assuming that all folders in streamingassets are project folders
        foreach (var dir in new DirectoryInfo(Application.streamingAssetsPath).GetDirectories())
            Instantiate(projectListingPrefab, projectListingParent).GetComponent<ProjectListing>().Init(dir);
    }

    // called from projectListing buttons in the scroll view
    public static void Selected(ProjectListing listing)
    {
        Singleton.projectDir = listing.newButton ? Singleton.NewProject() : listing.dir;
        if (listing.newButton)
            Singleton.RefreshProjectListing();
        Singleton.RefreshProjectData();
    }

    // -- PROJECT DATA ---

    public TMP_Text nameplate, errorPrintout;
    public Button playButton;

    Novel novel;
    ParsedProjectSettings settings;
    List<string> errors;

    // also also called from the in-scene refresh button
    public void RefreshProjectData()
    {
        // if we don't currently have a project selected, just keep everything empty
        if (projectDir == null)
        {
            nameplate.text = "";
            errorPrintout.text = "";
            playButton.interactable = false;
            return;
        }

        nameplate.text = projectDir.Name;
        novel = new();
        errors = new();
        if (!Interpreter.InterpretProject(projectDir, novel, errors, true, out settings))
            errors.Add($"project does not contain a \"{Constants.SETTINGS_FILE_NAME}\" file");
        errorPrintout.text = string.Join('\n', errors);
        playButton.interactable = errors.Count == 0;
    }

    // --- PLAYING ---

    public int playerBuildIndex;

    // called from play button
    public void PlayProject()
    {
        NovelTesting.novel = novel;
        NovelTesting.settings = settings;
        NovelTesting.name = projectDir.Name;
        SceneManager.LoadScene(playerBuildIndex);
    }

    // --- HELPERS ---

    DirectoryInfo NewProject()
    {
        // nth new project gives name "new project n"
        int newProjectNumber = 0;
        string newDirPath = Path.Join(Application.streamingAssetsPath, "new project ");

        while (Directory.Exists(newDirPath + newProjectNumber.ToString()))
            newProjectNumber++;

        newDirPath += newProjectNumber.ToString();

        // create directory
        var dir = Directory.CreateDirectory(newDirPath);

        // project settings
        string psPath = Path.Combine(newDirPath, Constants.SETTINGS_FILE_NAME);
        File.Create(psPath).Dispose();
        File.WriteAllText(psPath, JsonUtility.ToJson(Constants.DEFAULT_PROJECT_SETTINGS, true));

        // scene file
        string sfPath = Path.Combine(newDirPath, "start");
        File.Create(sfPath).Dispose();
        File.WriteAllText(sfPath, Constants.DEFAULT_SCENE_FILE_CONTENTS);

        return dir;
    }
}
