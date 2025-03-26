using UnityEngine;
using TMPro;

public class ClickableOption : MonoBehaviour
{
    public TMP_Text textObject;
    int id;

    public void Show(int id, string displayText)
    {
        gameObject.SetActive(true);
        this.id = id;
        textObject.text = displayText;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    // called from eventTrigger
    public void Clicked()
    {
        Prompter.singleton.Clicked(id);
    }
}
