using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ColonistCommand : Command<Colonist>
{
    public ColonistCommand() : base() { }
    public ColonistCommand(ColonistCommandsRunner runner) : base(runner) { }

    public override Colonist EntityRef => ((ColonistCommandsRunner)CommandsRunner).Parent as Colonist;

    public abstract ColonistCommandSD CCSD { get; }
    public override CommandSD<Colonist> SD => CCSD;
}

[System.Serializable]
public abstract class ColonistCommandSD : CommandSD<Colonist>
{
    protected ColonistCommandSD(Command<Colonist> command) : base(command)
    {
    }
}