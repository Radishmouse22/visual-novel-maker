using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Interpreter
{
    static List<string> errors;
    static Novel n;
    static List<string> sceneFiles;
    static InterpretMode mode;
    static List<ICommand> currentScene;
    static bool loadImages = true;
    static FileInfo jpsFile;
    static ParsedProjectSettings pps;
    static bool containsProjectSettings;

    static string e;
    static ICommand lastShowOrHideCommand;

    public const string START_SCENE_NAME = "start";

    /// <summary>
    /// Big daddy function that returns a horrificly awful stupid bad object tree of commands
    /// </summary>
    /// <returns>Whether the project contains a project settings file</returns>
    public static bool InterpretProject(DirectoryInfo dir, Novel n, List<string> errors, bool loadImages, out ParsedProjectSettings projectSettings)
    {
        Interpreter.errors = errors;
        Interpreter.n = n;
        Interpreter.sceneFiles = new();
        Interpreter.currentScene = null;
        Interpreter.loadImages = loadImages;
        Interpreter.jpsFile = null;
        Interpreter.pps = null;
        Interpreter.containsProjectSettings = false;

        // import images and shelve scene files
        ImportFiles(dir);
        if (!loadImages)
            Resources.UnloadUnusedAssets();

        // interpret the files for declarations
        mode = InterpretMode.DECLARATIONS;
        for (int i = 0; i < sceneFiles.Count; i++) InterpretFile(sceneFiles[i]);

        // then, for commands
        mode = InterpretMode.COMMANDS;
        for (int i = 0; i < sceneFiles.Count; i++) InterpretFile(sceneFiles[i]);

        // parse project settings
        if (jpsFile != null)
            ParseProjectSettings(jpsFile);

        // check for start scene
        if (!n.scenes.ContainsKey(START_SCENE_NAME))
            errors.Add($"project needs to contain a scene named \"{START_SCENE_NAME}\" to start at");

        // set these back to null to free memory
        Interpreter.errors = null;
        Interpreter.n = null;
        Interpreter.sceneFiles = null;
        Interpreter.currentScene = null;
        Interpreter.loadImages = true;
        Interpreter.jpsFile = null;
        // Interpreter.containsProjectSettings = false;

        projectSettings = Interpreter.pps;
        Interpreter.pps = null;
        return containsProjectSettings;
    }
    
    static void ImportFiles(DirectoryInfo dir)
    {
        // check every file in this directory
        foreach (var file in dir.GetFiles())
        {
            // if it's a scene file, add it to the list to interpret later
            if (file.Extension == "")
            {
                sceneFiles.Add(file.FullName);
                continue;
            }
            if (file.Extension == ".json" && file.Name == Constants.SETTINGS_FILE_NAME)
            {
                if (containsProjectSettings)
                {
                    RaiseError($"Project contains multiple project settings ({Constants.SETTINGS_FILE_NAME}) files");
                    continue;
                }
                jpsFile = file;
                containsProjectSettings = true;
                continue;
            }
            // if it's an image, let's do it now
            if (ValidImageFile(file.Extension))
                ImportImage(file);
        }

        // called recursively to search every subdirectory
        foreach (var subDir in dir.GetDirectories())
            ImportFiles(subDir);
    }
    static void ImportImage(FileInfo file)
    {
        string fullname = Path.GetFileNameWithoutExtension(file.Name).ToLower();

        // error if multiple images have the same name
        if (n.images.ContainsKey(fullname))
        {
            errors.Add(e+$"multiple images with name \"{fullname}\"");
            return;
        }

        // load in image as a sprite (so that it's usable in the scene)
        byte[] pngBytes = File.ReadAllBytes(file.FullName);
        Texture2D tex = new(2, 2);
        if (!tex.LoadImage(pngBytes))
        {
            errors.Add(e+$"couldn't load image on path {file.FullName}");
            return;
        }
        tex.filterMode = FilterMode.Point;

        // declare image within project
        n.images.Add(fullname, loadImages ? Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f) : null);
    }

    static void InterpretFile(string path)
    {
        // initialize per-file variables
        lastShowOrHideCommand = null;
        bool multiLineComment = false;
        string name = Path.GetFileNameWithoutExtension(path);

        // interate through each line
        string[] lines = File.ReadAllText(path).Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            // remove spacing and tabs from the beginning and end
            string line = lines[i].Trim(new char[] {' ', '\t', '\r'});
            if (line.Length == 0) continue;

            // comments
            if (line[0] == '#')
            {
                // if there are three pounds, start/end a multiline comment
                if (line.Length >= 3 && line[0..3] == "###")
                {
                    multiLineComment = !multiLineComment;
                    continue;
                }
                // otherwise, it's a normal comment and we just skip this line
                continue;
            }
            if (multiLineComment)
                continue;
            
            // split into words
            List<string> words = line.Split(' ').ToList();
            words.RemoveAll(x => string.IsNullOrWhiteSpace(x)); // allows for weird   word      spacing
            if (words.Count == 0)
                continue;

            // error header
            e = $"({name}, {i+1}) ";

            switch (mode)
            {
                case InterpretMode.DECLARATIONS:
                    InterpretLineForDeclarations(words);
                    break;
                case InterpretMode.COMMANDS:
                    InterpretLineForCommands(words);
                    break;
            }
        }
    }
    static void InterpretLineForDeclarations(List<string> words)
    {
        foreach (CommandTemplate dec in declarationTemplates)
        {
            if (words[0] == dec.keyword)
            {
                dec.Interpret(words);
                return;
            }
        }

        // unknown keywords will be handled in the command-checking step
        // because of speak statements
        // RaiseError("unknown keyword");
    }
    static void InterpretLineForCommands(List<string> words)
    {
        // not a command, but we still need to switch scenes in the command-interpreting step
        if (words[0] == "scene")
        {
            // simply return if the scene command doesn't work
            // because we've already checked it in the declarations pass
            if (words.Count == 1 || !n.scenes.ContainsKey(words[1]))
            {
                currentScene = null;
                return;
            }

            currentScene = new();
            n.scenes[words[1]] = currentScene;
            return;
        }

        // ignore any declarations
        foreach (CommandTemplate dec in declarationTemplates)
        {
            if (words[0] == dec.keyword)
                return;
        }

        // try to interpret a command
        foreach (CommandTemplate com in commandTemplates)
        {
            if (words[0] == com.keyword)
            {
                // all commands must be in a scene
                if (currentScene == null)
                {
                    RaiseError("command must be in a scene");
                    return;
                }
                com.Interpret(words);
                return;
            }
        }

        // if it wasn't a keyword command, it might be a speak command
        // [character shorthand] <speach*>
        if (n.characters.TryGetValue(words[0], out Character c))
        {
            // all commands must be in a scene
            if (currentScene == null)
            {
                RaiseError("command must be in a scene");
                return;
            }
            if (words.Count == 1)
            {
                currentScene.Add(new Speak(c.name, ""));
                return;
            }
            currentScene.Add(new Speak(c.name, GetRestOfLine(words, 1)));
            return;
        }

        RaiseError("unknown keyword");
    }

    readonly struct CommandTemplate
    {
        public readonly string keyword;
        readonly string[] argumentNames;
        readonly int TargetTermCount => argumentNames.Length+1;
        readonly bool lastTermIsMultiword;
        readonly Func<List<string>, string> ifValid;

        public CommandTemplate(string keyword, string[] argumentNames, bool lastTermIsMultiword, Func<List<string>, string> ifValid)
        {
            this.keyword = keyword;
            this.argumentNames = argumentNames;
            this.lastTermIsMultiword = lastTermIsMultiword;
            this.ifValid = ifValid;
        }

        public void Interpret(List<string> terms)
        {
            if (!lastTermIsMultiword && terms.Count > TargetTermCount)
            {
                RaiseError("too many arguments");
                return;
            }
            if (terms.Count < TargetTermCount)
            {
                StringBuilder b = new("missing arguments: ");
                for (int i = terms.Count; i < TargetTermCount; i++)
                {
                    if (i > terms.Count)
                        b.Append(", ");
                    b.Append(argumentNames[i-1]);
                }
                RaiseError(b.ToString());
                return;
            }
            if (ifValid != null)
            {
                string err = ifValid.Invoke(terms);
                if (err != null)
                {
                    RaiseError(err);
                    return;
                }
            }
        }
    }
    static readonly CommandTemplate[] declarationTemplates = new CommandTemplate[]
    {
        new("scene", new string[] {"<name>"}, false, terms => {
            if (terms[1] == SCENE_NAME_PLACEHOLDER)
                return "that name is not allowed";
            if (n.scenes.ContainsKey(terms[1]))
                return $"more than one scene named \"{terms[1]}\"";

            n.scenes.Add(terms[1], null);
            return null;
        }),


        new("char", new string[] {"<shorthand>","<display name*>"}, true, terms => {
            if (n.characters.ContainsKey(terms[1]))
                return "cannot reuse shorthand";
            if (declarationTemplates.Any(t => t.keyword == terms[1]) || commandTemplates.Any(t => t.keyword == terms[1]))
                return "shorthand cannot be a language keyword";

            n.characters.Add(terms[1], new(GetRestOfLine(terms, 2)));
            return null;
        }),


        new("var", new string[] {"<name>","<true or false>"}, false, terms => {
            bool value;
            switch (terms[2])
            {
                case "true": value = true; break;
                case "false": value = false; break;
                default:
                    return "must be true or false value";
            }
            if (n.variables.ContainsKey(terms[1]))
                return "variable already declared";

            n.variables.Add(terms[1], value);
            return null;
        }),


        new("choice", new string[] {"<shorthand>"}, false, terms => {
            if (n.choices.ContainsKey(terms[1]))
                return $"more than one choice named \"{terms[1]}\"";

            n.choices.Add(terms[1], new());
            return null;
        }),
    };
    static readonly CommandTemplate[] commandTemplates = new CommandTemplate[]
    {
        new("set", new string[] {"[variable]","<true or false>"}, false, terms => {
            bool value;
            switch (terms[2])
            {
                case "true": value = true; break;
                case "false": value = false; break;
                default:
                    return "must be true or false value";
            }
            if (!n.variables.ContainsKey(terms[1]))
                return "variable with that name is not declared";

            currentScene.Add(new Set(terms[1], value));
            return null;
        }),


        new("if", new string[] {"[variable]"}, false, terms => {
            if (!n.variables.ContainsKey(terms[1]))
                return "variable with that name is not declared";

            currentScene.Add(new If(terms[1]));
            return null;
        }),


        new("ifnot", new string[] {"[variable]"}, false, terms => {
            if (!n.variables.ContainsKey(terms[1]))
                return "variable with that name is not declared";

            currentScene.Add(new IfNot(terms[1]));
            return null;
        }),


        new("option", new string[] {"[shorthand]","[scene]","<display text*>"}, true, terms => {
            if (!n.choices.ContainsKey(terms[1]))
                return "choice with that shorthand is not declared";
            bool placeheld = terms[2] == SCENE_NAME_PLACEHOLDER;
            if (!placeheld && !n.scenes.ContainsKey(terms[2]))
                return "scene with that name is not declared";

            n.choices[terms[1]].Add(new(placeheld ? null : terms[2], GetRestOfLine(terms, 3)));
            return null;
        }),


        new("prompt", new string[] {"[shorthand]"}, false, terms => {
            if (!n.choices.ContainsKey(terms[1]))
                return "choice with that name is not declared";

            currentScene.Add(new Prompt(terms[1]));
            return null;
        }),


        new("cjump", new string[] {"[shorthand]"}, false, terms => {
            if (!n.choices.ContainsKey(terms[1]))
                return "choice with that name is not declared";

            currentScene.Add(new ChoiceBasedJump(terms[1]));
            currentScene = null;
            return null;
        }),


        new("jump", new string[] {"[scene]"}, false, terms => {
            if (!n.scenes.ContainsKey(terms[1]))
                return "scene with that name is not declared";

            currentScene.Add(new Jump(terms[1]));
            currentScene = null;
            return null;
        }),


        new("wait", new string[] {"<duration>"}, false, terms => {
            if (!float.TryParse(terms[1], out float duration))
                return "duration is not a valid decimal number";
            if (duration <= 0)
                return "duration must be greater than zero";

            currentScene.Add(new Wait(duration));
            return null;
        }),


        new("bg", new string[] {"[image fullname*]"}, true, terms => {
            string imageFullname = GetRestOfLine(terms, 1);
            if (!n.images.ContainsKey(imageFullname))
                return "no image with that fullname";

            currentScene.Add(new BackgroundChange(imageFullname));
            return null;
        }),


        new("show", new string[] {"[image fullname*]"}, true, terms => {
            string imageFullname = GetRestOfLine(terms, 1);
            if (!n.images.ContainsKey(imageFullname))
                return "no image with that fullname";

            Show sh = new(imageFullname);
            currentScene.Add(sh);
            lastShowOrHideCommand = sh;
            return null;
        }),


        new("hide", new string[] {"[image group]"}, false, terms => {
            // since we don't track currently displayed image groups during interpretation,
            // and it would be impossible to do so just with this method of interpretation
            // because of loops and conditions, we'll just handle trying to hide an image
            // group that isn't currently shown during runtime
            Hide h = new(terms[1]);
            currentScene.Add(h);
            lastShowOrHideCommand = h;
            return null;
        }),
            

        new("at", new string[] {"<horizontal>","<vertical>"}, false, terms => {
            if (lastShowOrHideCommand == null || lastShowOrHideCommand is not Show)
                return "no last show command";

            HorizontalImagePosition h = terms[1] switch
            {
                "left"        => HorizontalImagePosition.left,
                "leftcenter"  => HorizontalImagePosition.leftcenter,
                "center"      => HorizontalImagePosition.center,
                "rightcenter" => HorizontalImagePosition.rightcenter,
                "right"       => HorizontalImagePosition.right,
                _ => HorizontalImagePosition.ERROR
            };
            if (h == HorizontalImagePosition.ERROR)
                return "invalid horizontal position";

            VerticalImagePosition v = terms[2] switch
            {
                "bottom"       => VerticalImagePosition.bottom,
                "bottomcenter" => VerticalImagePosition.bottomcenter,
                "center"       => VerticalImagePosition.center,
                "topcenter"    => VerticalImagePosition.topcenter,
                "top"          => VerticalImagePosition.top,
                _ => VerticalImagePosition.ERROR
            };
            if (v == VerticalImagePosition.ERROR)
                return "invalid vertical position";

            Show sh = lastShowOrHideCommand as Show;
            sh.v = v;
            sh.h = h;
            return null;
        }),

        
        new("with", new string[] {"<mode>","<duration>"}, false, terms => {
            if (lastShowOrHideCommand == null)
                return "no last show or hide command";
            TransitionMode withMode = terms[1] switch
            {
                "none" => TransitionMode.None,
                "fade" => TransitionMode.Fade,
                _ => TransitionMode.ERROR
            };
            if (withMode == TransitionMode.ERROR)
                return "invalid mode";

            if (!float.TryParse(terms[2], out float duration))
                return "duration is not a valid decimal number";
            if (duration <= 0)
                return "duration must be greater than zero";

            if (lastShowOrHideCommand is Show sh)
            {
                sh.mode = withMode;
                sh.transitionDuration = duration;
                return null;
            }
            if (lastShowOrHideCommand is Hide h)
            {
                h.mode = withMode;
                h.transitionDuration = duration;
                return null;
            }
            return null;
        }),


        new("clear", new string[0], false, terms => {
            currentScene.Add(new Clear());
            return null;
        }),
    };

    static void ParseProjectSettings(FileInfo file)
    {
        e = "";

        // Interpret the file into an instance of the ProjectSettings class
        var jps = JsonUtility.FromJson<JsonProjectSettings>(File.ReadAllText(file.FullName)); // omg I love this utility idk what I'd do without it
        if (jps == null)
        {
            RaiseError("Error with project settings file - consider deleting");
            return;
        }

        pps = new();

        // Parse all of the fields
        if (!ColorUtility.TryParseHtmlString(jps.main_color, out Color c))
            RaiseError($"couldn't parse {nameof(jps.main_color)} in {Constants.SETTINGS_FILE_NAME}");
        else
            pps.mainColor = c;

        if (!n.images.ContainsKey(jps.menu_bg_image))
            RaiseError($"no image named \"{jps.menu_bg_image}\" in project");
        else
            pps.menuBGImage = jps.menu_bg_image;

        if (!n.images.ContainsKey(jps.dialogue_bg_image))
            RaiseError($"no image named \"{jps.dialogue_bg_image}\" in project");
        else
            pps.dialogueBGImage = jps.dialogue_bg_image;
    }

    static bool ValidImageFile(string ext) => ext == ".png" || ext == ".jpg";
    // static void RaiseError(string err) => errors.Add(e+err);
    static void RaiseError(string err)
    {
        Debug.Log(e+err);
        errors.Add(e+err);
    }
    static string GetRestOfLine(List<string> words, int startingAt)
        => string.Join(' ', words.GetRange(startingAt, words.Count-startingAt));
    const string SCENE_NAME_PLACEHOLDER = "_";

    enum InterpretMode
    {
        DECLARATIONS,
        COMMANDS
    }
}
