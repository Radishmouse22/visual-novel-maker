using System.IO;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class Singleton : MonoBehaviour
{
    public const string PROJECT_SETTINGS_FILE_NAME = "project_settings";
    public const string STARRED_PROJECT_NAME_FILE = "starred";
    public const string NEW_PROJECT_DEFAULT_CONTENTS = "scene start";

    public static string starredProject = "";

    public ProjectSelector projectSelector;
    public EditorUIManager uiManager;
    public ProjectDetails details;
    public static DirectoryInfo workingProjectDirectory;

    public static Singleton Instance = null;
    void Awake()
    {
        if (Instance != null)
        {
            DestroyImmediate(this);
            return;
        }
        Instance = this;

        if (!ReadStarredProjectFile(out string error))
            Debug.LogWarning(error);
    }

    public static bool ReadStarredProjectFile(out string error)
    {
        if (!string.IsNullOrEmpty(starredProject)) 
        {
            error = null;
            return true;
        }

        string filePath = Path.Join(Application.streamingAssetsPath, STARRED_PROJECT_NAME_FILE);

        if (!File.Exists(filePath))
        {
            CreateStarredProjectFile();
            error = $"No \"{STARRED_PROJECT_NAME_FILE}\" file, created one";
            return false;
        }

        string contents = File.ReadAllText(new FileInfo(filePath).FullName);

        if (string.IsNullOrWhiteSpace(contents))
        {
            error = $"\"{STARRED_PROJECT_NAME_FILE}\" file is blank";
            return false;
        }

        foreach (var dir in new DirectoryInfo(Application.streamingAssetsPath).GetDirectories())
        {
            if (dir.Name == contents)
            {
                // open
                starredProject = contents;
                error = null;
                return true;
            }
        }

        error = $"no project present named \"{contents}\" - fix or clear \"{STARRED_PROJECT_NAME_FILE}\" file";
        return false;
    }

    static void CreateStarredProjectFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, STARRED_PROJECT_NAME_FILE);
        File.Create(path).Dispose();
        File.WriteAllText(path, "");
    }

    public static void WriteStarredProjectFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath, STARRED_PROJECT_NAME_FILE);
        File.WriteAllText(path, starredProject);
    }

    public static string TEST_PROJECT_NAME = null;
}
