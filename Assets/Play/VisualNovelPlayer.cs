using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VisualNovelPlayer : MonoBehaviour
{
    public GameObject playerParent;
    public Image background;

    [Header("Components")]
    public DialogueBox box;
    public ImageManager imageManager;
    public Prompter prompter;

    // live variables
    int commandIndex = 0;
    List<ICommand> commands;
    bool skipNextCommand;
    Dictionary<string, string> selections; // choice, chosen scene
    public static Novel n => NovelTesting.novel;

    void Awake()
    {
        playerParent.SetActive(false);
    }

    // called from play button
    public void Play()
    {
        playerParent.SetActive(true);
        imageManager.DestroyAll();
        prompter.DestroyAll();
        commandIndex = 0;
        commands = n.scenes[Interpreter.START_SCENE_NAME];
        skipNextCommand = false;
        selections = new();
        NextEvent();
    }

    void NextEvent()
    {
        if (commandIndex >= commands.Count)
        {
            Done();
            return;
        }

        ICommand e = commands[commandIndex++];

        if (skipNextCommand)
        {
            skipNextCommand = false;
            NextEvent();
            return;
        }

        box.Shown = e is Speak;

        // horrible, absolutely abismal if-else chain; but I don't give a damn
             if (e is Set               s) Set(s);
        else if (e is If                i) If(i);
        else if (e is IfNot           ifn) IfNot(ifn);
        else if (e is Prompt            p) Prompt(p);
        else if (e is ChoiceBasedJump cbj) ChoiceBasedJump(cbj);
        else if (e is Jump              j) Jump(j);
        else if (e is Wait              w) Wait(w);
        else if (e is BackgroundChange bc) BackgroundChange(bc);
        else if (e is Show             sh) Show(sh);
        else if (e is Hide              h) Hide(h);
        else if (e is Clear             c) Clear(c);
        else if (e is Speak             d) Dialogue(d);
    }

    void Set(Set e)
    {
        n.variables[e.var] = e.value;
        NextEvent();
    }
    void If(If e)
    {
        skipNextCommand = !n.variables[e.var];
        NextEvent();
    }
    void IfNot(IfNot e)
    {
        skipNextCommand = n.variables[e.var];
        NextEvent();
    }
    void Prompt(Prompt e)
    {
        _Prompt(e.choice, NextEvent);
    }
    void _Prompt(string choice, Action after)
    {
        var options = n.choices[choice];

        prompter.Prompt(options, selection =>
        {
            selections[choice] = options[selection].sceneToJumpTo;
            after();
        });
    }
    void ChoiceBasedJump(ChoiceBasedJump cbj)
    {
        if (selections.ContainsKey(cbj.choice))
            _ChoiceBasedJump(cbj);
        else
        {
            _Prompt(cbj.choice, () => _ChoiceBasedJump(cbj));
            return;
        }
    }
    void _ChoiceBasedJump(ChoiceBasedJump cbj)
    {
        _Jump(selections[cbj.choice]);
        NextEvent();
    }
    void Jump(Jump e)
    {
        _Jump(e.jumpTo);
        NextEvent();
    }
    void _Jump(string to)
    {
        commands = n.scenes[to];
        commandIndex = 0;
    }
    void Wait(Wait e) => StartCoroutine(WaitCoroutine(e));
    IEnumerator WaitCoroutine(Wait e)
    {
        yield return new WaitForSeconds(e.duration);
        NextEvent();
    }
    void BackgroundChange(BackgroundChange e)
    {
        background.sprite = n.images[e.changeTo];
        NextEvent();
    }
    void Show(Show e)
    {
        imageManager.Show(e.toShow, e.h, e.v);
        NextEvent();
    }
    void Hide(Hide e)
    {
        imageManager.Hide(e.groupToHide);
        NextEvent();
    }
    void Clear(Clear e)
    {
        imageManager.Clear();
        NextEvent();
    }
    void Dialogue(Speak e)
    {
        box.Print(e.speaking, e.speach, NextEvent);
    }

    void Done()
    {
        playerParent.SetActive(false);

        // free up memory?
        commands = null;
        selections = null;
    }
}
