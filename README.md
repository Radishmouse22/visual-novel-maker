# Sam's Visual Novel Maker
A tool for creating and playing visual novel games.
Currently developed in the Unity game engine with plans to move to a standalone application.
This readme will become more in-depth and useful as development continues.

# LANGUAGE
Commands make things happen in the visual novel. A command makes up a line of code. Only one command is allowed per line.

#### Case
All language keywords are lowercase because I care about your pinky and holding `shift` sucks. For this reason, it is convention for all object, scene, and image names to also be lowercase. This also helps you be able to tell the difference between speach (`sam Hey! That's mine!`) and commands (`jump confrontation`).

#### Indentation
Indentation is completely optional. Tabs (yay) or spaces (ew) are fine.
It is convention to indent commands in a scene, like so:
```
scene start
    sam I'm indented!
```
and to indent under an `if` statment, like so:
```
scene tutorial
    if playedBefore:
        jump beginning
```

#### Naming
Since most command arguments are one word long (An asterisk `*` indicates otherwise) you should use `camelCase` (yay) or `snake_case` (ew) to fit multiple words and maintain readability.

#### Special Characters
Only these standard English characters are conventional: `A-Z` `a-z` `,.:;'"/()!@#$%^&*-=_+~`
Obscure UTF-8 characters, emojis, unusual Unicode characters, or any other characters might result in undefined behaviour.
Line both Windows (`CRLF`) and UNIX (`LF`) line endings are allowed.

# COMMANDS
Non-declarations must be defined under a scene.

## Declarations
Declarations are a special kind of command that define a new object that other commands will use.
All objects must have unique names, but this only applies per-type. A character named "sam" and a scene named "sam" is okay.

#### Scenes
Declare a new scene
`scene <name>`

All commands that follow will be a part of this scene. You can jump between scenes with the `jump` command, and doing so will end the current scene.

The underscore `_` placeholder is not an allowed scene name.

A scene named `start` is required in your project, which is where execution will begin.

#### Characters
Declare a new character
`char <shorthand> <display name*>`

Characters are used for dialogue. Characters have shorthands that make typing out dialogue between several characters very easy and efficient.

Since dialogue commands start with the character's shorthand, language keywords like `choice` or `if` are not allowed characters names.

#### Choices
Declare a new choice event
`choice <shorthand>`

Choice events are used to create story trees, jumping to specific scenes depending on the player's selection with the `cjump` command. Players can be prompted with a choice at any time with the `prompt` command. Choices can be prompted to the player and jumped off of multiple times.

A choice's options must be declared individually with the `option` command.

You might notice that creating a choice is done in a declaration, but using it and setting its options are done in commands. For this reason, it is convention to declare a choice along with its usage if it is only used once, like so:
```
scene bigDecision
    choice bd
    option bd nope Nope, I'm good.
    option bd yep Yep, sounds good!
    cjump bd

scene nope ...
scene yep ...
```
#### Variables
Declare a new boolean variable
`var <name> <true or false>`

Variables are used to conditionally execute commands with a preceeding `if` or `ifnot` command, like so:
```
var wentOnDate false
scene home
    if wentOnDate
        sam Whew, that was stressful.
    ifnot wentOnDate
        sam Damn, I can't believe she said no...
```
A variable's value can be set with the `set` command

## Normal Commands
#### Dialogue
`[character shorthand] <dialogue*>`
This allows a character to speak their mind.

#### Set Variable
`set [variable] <true or false>`
Sets the value of a variable after it has been declared.

#### If Conditional
`if [variable]`
Only executes the next command if the varible is set to true.

#### If-not Conditional
`ifnot [variable]`
Only executes the next command if the varible is set to false.

#### Set Choice Option
`option [choice shorthand] [scene] <display text*>`
Defines an option of a choice.

If you do not wish to define a scene to jump to, a placeholding underscore `_` can be used.

>Because scenes end when jumping, calling the `cjump` command on a choice where a placeheld option is selected will cause the game to end, rather than throwing some kind of runtime exception.

#### Prompt with Choice
`prompt [choice shorthand]`
Prompts the player to make a selection from a choice's options.

A `cjump` command can be used afterwards to jump scenes based on this selection.

>if you are using the 'cjump' command immediately after this one, this command can be omitted since 'cjump' automatically prompts the player anyways

#### Choice-based Jump
`cjump [choice shorthand]`
Jumps scenes based on a player's selection in the choice event.

If the player has not been prompted with this choice yet, they will be automatically prompted before the jump executes.

>Because scenes end when jumping, calling the `cjump` command on a choice where a placeheld option is selected will cause the game to end, rather than throwing some kind of runtime exception.

#### Jump Between Scenes
`jump [scene]`
Switches to a different scene.

#### Wait
`wait <duration>`
Waits a time (in seconds) before executing the next command.

#### Change/Set Background
`bg [image fullname*]`
Changes the background image.

#### Show Image
`show [image fullname*]`
Displays an image on the screen, replacing an existing image of the same group if one is currently displayed.

#### Hide Image
`hide [image group]`
    hides the image of a group if one is being displayed; if not, nothing happens

#### Set Image Position
>should be written right after a `show` command

`at <horizontal> <vertical>`

Specifies where an image should be shown on the screen.

The horizontal options: `left`, `leftcenter`, `center`, `rightcenter`, `right`
vertical options: `top`, `topcenter`, `center`, `bottomcenter`, `bottom`

The default image position is `center bottomcenter`

#### Image Hide/Show Transition
>should be written right after a `show` or `hide` command

`with <mode> <duration> # in seconds`

Defines a transitions that an image should be shown or hidden with.

The modes are: `none`, `fade`

The default is `fade 0.5`

#### Clear Images
`clear`
Clears the screen of all images (except for the background).
