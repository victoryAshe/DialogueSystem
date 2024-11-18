using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Story
{
    // dialogue, characterType
    private Dictionary<string, string> _data;
    public Dictionary<string, string> Data
    {
        get
        {
            if (_data == null) _init_storyList();
            return _data;
        }
    }
    public string[] _dialogue = new string[] { };
    public string[] _charType = new string[] { };

    //================================================
    //Private Method
    //================================================

    void _init_storyList()
    {
        _data = new Dictionary<string, string>();

        if (_dialogue.Length != _charType.Length)
            Debug.LogError("Dialogue and CharacterType have different lengths");

        for (int i = 0; i < _dialogue.Length; i++)
        {
            _dialogue[i] = _dialogue[i].Replace("\\n", "\n");
            _data.Add(_dialogue[i], _charType[i]);
        }
    }
}
