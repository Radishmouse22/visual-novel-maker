using System;
using System.Collections.Generic;
using UnityEngine;

public class Prompter : MonoBehaviour
{
    public GameObject optionPrefab;
    public Transform optionsParent;

    Queue<ClickableOption> options = new();
    List<ClickableOption> shownOptions = new();
    Action<int> onSelected;

    public static Prompter singleton = null;
    void Awake()
    {
        if (singleton != null)
        {
            DestroyImmediate(this);
            Debug.Log("duplicates");
            return;
        }
        singleton = this;
        optionPrefab.gameObject.SetActive(false);
    }

    public void Prompt(List<ChoiceOption> optionsData, Action<int> onSelected)
    {
        optionsParent.gameObject.SetActive(true);

        this.onSelected = onSelected;

        // enable and/or spawn necessary option objects
        while (options.Count < optionsData.Count)
        {
            var newOption = Instantiate(optionPrefab, optionsParent).GetComponent<ClickableOption>();
            options.Enqueue(newOption);
        }

        // show options
        for (int i = 0; i < optionsData.Count; i++)
        {
            var o = options.Dequeue();
            shownOptions.Add(o);
            o.Show(i, optionsData[i].displayText);
        }
    }

    public void Clicked(int id)
    {
        // add options back to the end of the queue
        for (int i = 0; i < shownOptions.Count; i++)
        {
            var o = shownOptions[i];
            options.Enqueue(o);
            o.Hide();
        }
        shownOptions.Clear();

        optionsParent.gameObject.SetActive(false);

        onSelected(id);
    }

    public void DestroyAll()
    {
        while (options.Count > 0)
            Destroy(options.Dequeue().gameObject);
        options.Clear();
    }
}
