public static class ManualCommands
{
    [Command("previous")]
    public static void Previous(string command)
    {
        VrEfmService.instance.MovePage(MoveDirection.Previous);
    }

    [Command("next")]
    public static void Next(string command)
    {
        VrEfmService.instance.MovePage(MoveDirection.Next);
    }

    [Command("open")]
    public static void Open(string command)
    {
        VrEfmService.instance.Open(command);
    }
}