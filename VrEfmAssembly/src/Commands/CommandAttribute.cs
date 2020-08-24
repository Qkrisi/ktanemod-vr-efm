using System;

[AttributeUsage(AttributeTargets.Method)]
public sealed class Command : Attribute
{
    public string Start { get; private set; }

    public Command(string start)
    {
        if (String.IsNullOrEmpty(start)) throw new ArgumentException("Start cannot be null or empty!");
        Start = start;
    }
}