using UnityEngine;

[System.Serializable]
public class JsonProjectSettings
{
    public string main_color = "#efa421";
    public string menu_bg_image = "";
    public string dialogue_bg_image = "";
}

public class ParsedProjectSettings
{
    public Color mainColor;
    public string menuBGImage;
    public string dialogueBGImage;
}