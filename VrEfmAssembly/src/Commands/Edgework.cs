public static class EdgeworkCommands
{
    [DefaultCommand(Priority.After)]
    public static void UpdateEdgework(string command)
    {
        VrEfmService.instance.UpdateEdgework();
    }

    [Command("batteries ")]
    public static void Batteries(string command)
    {
        var splitted = command.Trim().ToLowerInvariant().Split(new string[] { " in " }, System.StringSplitOptions.RemoveEmptyEntries);
        if (splitted.Length<2 || !int.TryParse(splitted[0], out int batteries) || !int.TryParse(splitted[1], out int holders)) return;
        VrEfmService.instance.Edgework.Batteries = command;
    }

    [Command("indicator ")]
    public static void Indicator(string command)
    {
        var splitted = command.Trim().ToUpperInvariant().Split(' ');
        if (splitted.Length < 2) return;
        string indicator = "";
        switch(splitted[0])
        {
            case "LIT":
            case "ON":
                indicator += "*";
                break;
            case "UNLIT":
            case "OFF":
                break;
            default:return;
        }
        indicator += splitted[1];
        VrEfmService.instance.Edgework.Indicators += $" {indicator}";
    }

    [Command("portplate")]
    public static void Port(string command)
    {
        VrEfmService.instance.Edgework.Ports += $" [{command}]";
    }

    [Command("serial ")]
    public static void Serial(string command)
    {
        VrEfmService.instance.Edgework.Serial = command;
    }

    [Command("other ")]
    public static void Other(string command)
    {
        VrEfmService.instance.Edgework.Other += $" {command}";
    }
}