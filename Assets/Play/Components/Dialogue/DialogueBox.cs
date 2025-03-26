using System;
using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;

public class DialogueBox : MonoBehaviour
{
    bool _shown;
    [HideInInspector] public bool Shown
    {
        get => _shown;
        set
        {
            if (_shown == value)
                return;
            _shown = value;

            box.SetActive(value);
        }
    }

    void Awake()
    {
        box.SetActive(false);
    }

    public float printCharacterDelay;
    public GameObject box;
    public TMP_Text speechText, speakerText;
    Action whenDone;
    bool printing = false;
    string target;

    public void Print(string speaker, string speech, Action whenDone)
    {
        printing = false;
        this.whenDone = whenDone;
        speakerText.text = speaker;
        speechText.text = "";
        target = speech;
        StartCoroutine(PrintRoutine());
    }

    IEnumerator PrintRoutine()
    {
        printing = true;
        StringBuilder sb = new();
        int i = 0;

        while (i < target.Length)
        {
            sb.Append(target[i++]);
            speechText.text = sb.ToString();
            yield return new WaitForSeconds(printCharacterDelay);
        }

        printing = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Shown)
            Skip();
    }

    void Skip()
    {
        if (printing)
        {
            printing = false;
            StopAllCoroutines();
            speechText.text = target;
            return;
        }
        whenDone();
    }
}
