using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.Events;
using System.Linq;


#region Enum
public enum State
{
    Active,
    Wait,
    Deactivate
}

public enum Command
{
    print,
    color,
    emote,
    size,
    sound,
    speed,
    click,
    close,
    wait,
    rename,
    getname_ah,
    getname_yee,
    printer_off,
    fade
}

public enum TextColor
{
    aqua,
    black,
    blue,
    brown,
    cyan,
    darkblue,
    fuchsia,
    green,
    grey,
    lightblue,
    lime,
    magenta,
    maroon,
    navy,
    olive,
    orange,
    purple,
    red,
    silver,
    teal,
    white,
    yellow
}
#endregion

#region Emotion
[Serializable]
public class Emotion
{
    //================================================
    //Public Variable
    //================================================
    private Dictionary<string, Sprite> _data;
    public Dictionary<string, Sprite> Data
    {
        get
        {
            if (_data == null) _init_emotionList();
            return _data;
        }
    }

    public string[] _emotion = new string[] {  };
    public Sprite[] _sprite;

    //================================================
    //Private Method
    //================================================
    private void _init_emotionList()
    {
        _data = new Dictionary<string, Sprite>();

        if (_emotion.Length != _sprite.Length)
            Debug.LogError("Emotion and Sprite have different lengths");

        for (int i = 0; i < _emotion.Length; i++)
            _data.Add(_emotion[i], _sprite[i]);
    }
}
#endregion

/// <summary>
/// Convert string to Data. Contains List of DialogCommand and DialogFormat.
/// </summary>
public class DialogueData
{
    //================================================
    //Public Variable
    //================================================
    public string Character;
    public List<DialogCommand> Commands = new List<DialogCommand>();
    public DialogFormat Format = new DialogFormat();

    public string PrintText = string.Empty; // 이것 건드리면 안된다
    public string getString;

    public bool isSkippable = true;
    public UnityAction Callback = null;

    // For ResourceManager's load
    public int showIndex;
    public int hideIndex;

    //================================================
    //Public Method
    //================================================
    public DialogueData(string originalString, string character = "", UnityAction callback = null, bool isSkipable = true)
    {
        _convert(originalString);

        this.isSkippable = isSkipable;
        this.Callback = callback;
        this.Character = character;
    }

    public DialogueData(string originalString, string character, int showIndex, int hideIndex, UnityAction callback = null, bool isSkipable = true)
    {
        _convert(originalString);

        this.Character = character;
        this.showIndex = showIndex;
        this.hideIndex = hideIndex;
    }

    public override string ToString()
    {
        string val = "";
        val += "PrintText: " + PrintText + "\n";
        val += "Commands: ";
        foreach (var com in Commands) val += com.Command.ToString() + " " + com.Context;
        val += "\n";
        if (Callback != null) val+=Callback.ToString();
        return val;
    }

    //================================================
    //Private Method
    //================================================
    private void _convert(string originalString)
    {
        string printText = string.Empty;

        for (int i = 0; i < originalString.Length; i++)
        {
            if (originalString[i] != '/')
            {
                if (originalString[i] == '_')
                {
                    printText += '\n';
                }
                else
                {
                    printText += originalString[i];
                }   
            }
            else // If find '/'
            {
                // Convert last printText to command
                if (printText != string.Empty)
                {
                    Commands.Add(new DialogCommand(Command.print, printText));
                    printText = string.Empty;
                }

                // Substring /CommandSyntex/
                var nextSlashIndex = originalString.IndexOf('/', i + 1);
                string commandSyntex = originalString.Substring(i + 1, nextSlashIndex - i - 1);

                // Add converted command
                var com = _convert_Syntex_To_Command(commandSyntex);
                if (com != null) Commands.Add(com);

                // Move i
                i = nextSlashIndex;
            }
        }

        if (printText != string.Empty) Commands.Add(new DialogCommand(Command.print, printText));
        getString = printText;
    }

    private DialogCommand _convert_Syntex_To_Command(string text)
    {
        var spliter = text.Split(':');

        Command command;
        if (Enum.TryParse(spliter[0], out command))
        {
            if (spliter.Length >= 2) return new DialogCommand(command, spliter[1]);
            else return new DialogCommand(command);
        }
        else
        {
            Debug.LogError($"Cannot parse to commands: {text}");
        }

        return null;
    }
}

/// <summary>
/// You can get RichText tagger of size and color.
/// </summary>
public class DialogFormat
{
    //================================================
    //Private Variable
    //================================================
    public string DefaultSize = "15";
    private string _defaultColor = "#000B4F";

    private string _color;
    private string _size;


    //================================================
    //Public Method
    //================================================
    public DialogFormat(string defaultSize = "", string defaultColor = "")
    {
        _color = string.Empty;
        _size = string.Empty;

        if (defaultSize != string.Empty) DefaultSize = defaultSize;
        if (defaultColor != string.Empty) _defaultColor = defaultColor;
    }

    public string Color
    {
        set
        {
            if (isColorValid(value))
            {
                _color = value;
                if (_size == string.Empty) _size = DefaultSize;
            }
        }

        get => _color;
    }

    public string Size
    {
        set
        {
            if (isSizeValid(value))
            {
                _size = value;
                if (_color == string.Empty) _color = _defaultColor;
            }
        }

        get => _size;
    }

    public string OpenTagger
    {
        get
        {
            if (isValid) return $"<color={Color}><size={Size}>";
            else return string.Empty;
        }
    }

    public string CloseTagger
    {
        get
        {
            if (isValid) return "</size></color>";
            else return string.Empty;
        }
    }

    public void Resize(string command)
    {
        if (_size == string.Empty) Size = DefaultSize;

        switch (command)
        {
            case "up":
                _size = (int.Parse(_size) + 10).ToString();
                break;

            case "down":
                _size = (int.Parse(_size) - 10).ToString();
                break;

            case "init":
                _size = DefaultSize;
                break;

            default:
                _size = command;
                break;
        }
    }

    //================================================
    //Private Method
    //================================================
    private bool isValid
    {
        get => _color != string.Empty && _size != string.Empty;
    }

    private bool isColorValid(string Color)
    {
        TextColor textColor;
        Regex hexColor = new Regex("^#(?:[0-9a-fA-F]{3}){1,2}$");

        return Enum.TryParse(Color, out textColor) || hexColor.Match(Color).Success;
    }

    private bool isSizeValid(string Size)
    {
        float size;
        return float.TryParse(Size, out size);
    }

}

public class DialogCommand
{
    public Command Command;
    public string Context;

    public DialogCommand(Command command, string context = "")
    {
        Command = command;
        Context = context;
    }
}