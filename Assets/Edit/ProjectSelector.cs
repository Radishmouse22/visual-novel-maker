using System.IO;
using System.Text;
using TMPro;
using UnityEngine;

public class ProjectSelector : MonoBehaviour
{
    public TMP_Text errorPrintout;
    public GameObject projectListingPrefab;
    public RectTransform projectListingParent;

    void Start() => RefreshProjectListings();

    // also called from a in-scene refresh button
    public void RefreshProjectListings()
    {
        for (int i = 1; i < projectListingParent.childCount; i++)
            Destroy(projectListingParent.GetChild(i).gameObject);
        foreach (var dir in new DirectoryInfo(Application.streamingAssetsPath).GetDirectories())
        {
            VerifyProject(dir, out string errors);
            Instantiate(projectListingPrefab, projectListingParent).GetComponent<ProjectListing>().Init(dir, errors);
        }
    }

    void VerifyProject(DirectoryInfo dir, out string errors)
    {
        StringBuilder sb = new();

        FileInfo projectSettingsFile = null;

        foreach (var file in dir.GetFiles())
        {
            if (file.Name == Singleton.PROJECT_SETTINGS_FILE_NAME)
            {
                projectSettingsFile = file;
                break;
            }
        }

        if (projectSettingsFile == null)
            sb.Append($"does not contain a settings file named \'{Singleton.PROJECT_SETTINGS_FILE_NAME}\' in the root directory\n");
        else
        {
            var ps = JsonUtility.FromJson<ProjectSettings>(File.ReadAllText(projectSettingsFile.FullName));
            if (ps == null)
                sb.Append("invalid project settings - file may be empty");
            else if (ps.main == null)
                sb.Append("main color is not set in project settings");
        }

        errors = sb.ToString();
    }

    public void Selected(ProjectListing listing)
    {
        errorPrintout.text = "";

        if (listing.dir == null)
        {
            NewProject();
            return;
        }
        if (listing.errors.Length > 0)
        {
            errorPrintout.text = listing.errors;
            return;
        }

        OpenProject(listing.dir);
    }

    void NewProject()
    {
        int num = 0;
        string newDirPath;

        while (true)
        {
            string path = Path.Join(Application.streamingAssetsPath, "new project " + num.ToString());
            if (Directory.Exists(path))
            {
                num++;
                continue;
            }
            newDirPath = path;
            break;
        }
        var dir = Directory.CreateDirectory(newDirPath);

        // project settings
        string psPath = Path.Combine(newDirPath, Singleton.PROJECT_SETTINGS_FILE_NAME);
        File.Create(psPath).Dispose();
        File.WriteAllText(psPath, JsonUtility.ToJson(new ProjectSettings(), true));

        // scene file
        string sfPath = Path.Combine(newDirPath, "start");
        File.Create(sfPath).Dispose();
        File.WriteAllText(sfPath, Singleton.NEW_PROJECT_DEFAULT_CONTENTS);

        OpenProject(dir);
    }

    void OpenProject(DirectoryInfo dir)
    {
        Singleton.workingProjectDirectory = dir;
        Singleton.Instance.uiManager.SwitchToDetailView();
        Singleton.Instance.details.Refresh();
    }
}
