using System.Collections.Generic;
using UnityEngine;

public class Novel
{
    public Dictionary<string, Character> characters = new(); // shorthand, character
    public Dictionary<string, bool> variables = new(); // name, value
    public Dictionary<string, List<ChoiceOption>> choices = new(); // shorthand, options

    public Dictionary<string, Sprite> images = new(); // name, image

    // name, events
    public Dictionary<string, List<ICommand>> scenes = new();
}

// public struct ImageName : IEquatable<ImageName>
// {
//     public string fullName;
//     public string group;
//     public string name;

//     public ImageName(string fullName)
//     {
//         this.fullName = fullName;
//         string[] parts = fullName.Split(' ');
//         group = parts[0];
//         name = parts.Length < 2 ? "" : fullName[(group.Length+1)..];
//     }

//     public override readonly int GetHashCode()
//     {
//         return fullName.GetHashCode();
//     }

//     public override readonly bool Equals(object obj)
//     {
//         if (obj == null)
//             return false;

//         if (obj is ImageName other)
//             return fullName.Equals(other.fullName);

//         return false;
//     }

//     public readonly bool Equals(ImageName other)
//     {
//         return fullName.Equals(other.fullName);
//     }
// }

public struct Character
{
    public string name;

    public Character(string name)
    {
        this.name = name;
    }
}

public struct ChoiceOption
{
    public string sceneToJumpTo;
    public string displayText;

    public ChoiceOption(string sceneToJumpTo, string displayText)
    {
        this.sceneToJumpTo = sceneToJumpTo;
        this.displayText = displayText;
    }
}

public interface ICommand {}

public struct Set : ICommand
{
    public string var;
    public bool value;

    public Set(string var, bool value)
    {
        this.var = var;
        this.value = value;
    }
}

public struct If : ICommand
{
    public string var;

    public If(string var)
    {
        this.var = var;
    }
}

public struct IfNot : ICommand
{
    public string var;

    public IfNot(string var)
    {
        this.var = var;
    }
}

public struct Prompt : ICommand
{
    public string choice;

    public Prompt(string choice)
    {
        this.choice = choice;
    }
}

public struct ChoiceBasedJump : ICommand
{
    public string choice;

    public ChoiceBasedJump(string choice)
    {
        this.choice = choice;
    }
}

public struct Jump : ICommand
{
    public string jumpTo;

    public Jump(string jumpTo)
    {
        this.jumpTo = jumpTo;
    }
}

public struct Wait : ICommand
{
    public float duration;

    public Wait(float duration)
    {
        this.duration = duration;
    }
}

public struct BackgroundChange : ICommand
{
    // fullname of image to change to
    public string changeTo;

    public BackgroundChange(string changeTo)
    {
        this.changeTo = changeTo;
    }
}

public class Show : ICommand
{
    // fullname of image to change to
    public string toShow;
    public VerticalImagePosition v;
    public HorizontalImagePosition h;
    public TransitionMode mode;
    public float transitionDuration;

    public Show(string toShow)
    {
        this.toShow = toShow;

        v = VerticalImagePosition.bottomcenter;
        h = HorizontalImagePosition.center;

        mode = TransitionMode.Fade;
        transitionDuration = 0.5f;
    }
}

public class Hide : ICommand
{
    // fullname of image to change to
    public string groupToHide;
    public TransitionMode mode;
    public float transitionDuration;

    public Hide(string groupToHide)
    {
        this.groupToHide = groupToHide;

        mode = TransitionMode.Fade;
        transitionDuration = 0.5f;
    }
}
// at
// with

public struct Clear : ICommand {}

public struct Speak : ICommand
{
    public string speaking;
    public string speach;

    public Speak(string speaking, string speach)
    {
        this.speaking = speaking;
        this.speach = speach;
    }
}

public enum TransitionMode
{
    None,
    Fade,
    ERROR
}

public enum HorizontalImagePosition
{
    ERROR       = -1,
    left        = 0,
    leftcenter  = 1,
    center      = 2,
    rightcenter = 3,
    right       = 4,
}

public enum VerticalImagePosition : int
{
    ERROR        = -1,
    bottom       = 0,
    bottomcenter = 1,
    center       = 2,
    topcenter    = 3,
    top          = 4,
}