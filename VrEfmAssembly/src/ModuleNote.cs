﻿using System.Collections.Generic;

public sealed class ModuleNote
{
    private readonly string StartingText;
    private List<string> History = new List<string>();
    private string __text;
    private string _text
    {
        get
        {
            return __text;
        }
        set
        {
            __text = value;
            VrEfmService.instance.ChangeNoteText(__text);
        }
    }
    public string Text
    {
        get
        {
            return _text;
        }
        set
        {
            History.Add(Text);
            _text = value;
        }
    }

    public void Undo(int times = 1)
    {
        History.Reverse();
        for(int i = 0;i<times;i++)
        {
            if (History.Count > 1) History.RemoveAt(0);
        }
        _text = History[0];
        History.Reverse();
    }

    public void Append(string t, string separator = "")
    {
        Text = $"{Text}{separator}{t}";
    }

    public void Reset()
    {
        Text = StartingText;
    }

    public ModuleNote(string startingText = "")
    {
        StartingText = startingText;
        _text = startingText;
        History.Add(startingText);
    }
}