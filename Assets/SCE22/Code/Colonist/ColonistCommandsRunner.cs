using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ColonistCommandsRunner : CommandsRunner<Colonist, ColonistCommand>
{
    public override CommandsRunnerSD<Colonist, ColonistCommand> SD
    {
        get
        {
            return new(this);
        }
    }
}
