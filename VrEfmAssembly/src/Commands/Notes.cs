public static class NoteCommands
{
    [Command("undo")]
    public static void Undo(string command)
    {
        int.TryParse(command.Trim(), out int times);
        VrEfmService.instance.CurrentNote.Undo(times <= 0 ? 1 : times);
    }

    [Command("newline")]
    [Command("new line")]
    public static void NewLine(string command)
    {
        VrEfmService.instance.CurrentNote.Append(command, "\n");
    }

    [Command("append")]
    public static void Append(string command)
    {
        VrEfmService.instance.CurrentNote.Append(command);
    }

    [Command("reset")]
    public static void Reset(string command)
    {
        VrEfmService.instance.CurrentNote.Reset();
    }

    [Command("clear")]
    public static void Clear(string command)
    {
        VrEfmService.instance.CurrentNote.Text = "";
    }
}
