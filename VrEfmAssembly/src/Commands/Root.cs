using System;
using System.Reflection;

public static class Root
{
    [Command("manual ")]
    public static void RunManualCommand(string command)
    {
        ProcessCommand(command, typeof(ManualCommands));
    }

    [Command("notes ")]
    public static void RunNoteCommand(string command)
    {
        ProcessCommand(command, typeof(NoteCommands));
    }

    [Command("edgework ")]
    public static void RunEdgeworkCommand(string command)
    {
        ProcessCommand(command, typeof(EdgeworkCommands));
    }

    public static void ProcessCommand(string command, Type CommandBatch)
    {
        Action Finalize = () => { };
        foreach (var method in CommandBatch.GetMethods(BindingFlags.Static))
        {
            foreach(var attribute in method.GetCustomAttributes(false))
            {
                if(attribute is Command CommandAttribute)
                {
                    if(command.ToLowerInvariant().StartsWith(CommandAttribute.Start.ToLowerInvariant()))
                    {
                        method.Invoke(null, new object[] { command.Substring(CommandAttribute.Start.Length) });
                        break;
                    }
                }
                else if(attribute is DefaultCommand DefaultCommandAttribute)
                {
                    Action temp = () => method.Invoke(null, new object[] { command });
                    if (DefaultCommandAttribute.RunPriority == Priority.Before) temp();
                    else { Finalize = temp; }
                }
            }
        }
        Finalize();
    }
}