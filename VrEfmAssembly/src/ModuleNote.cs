using System.Collections.Generic;

public sealed class ModuleNote
{
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
            //TODO: Change text on object
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

    public ModuleNote(string StartingText = "")
    {
        _text = StartingText;
        History.Add(StartingText);
    }
}