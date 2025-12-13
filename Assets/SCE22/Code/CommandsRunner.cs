using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using SimpleReactive;
using UnityEngine;

public abstract class CommandsRunner<TEntity, TCommand> : EntityComponent where TEntity : Entity where TCommand : Command<TEntity>
{
    private List<TCommand> _commands = new();
    public IReadOnlyCollection<TCommand> Commands => _commands;

    [SerializeField] private ReactiveVar<int> _mov = new(-1);
    public IReadOnlyReactiveVariable<int> Mov => _mov;

    public TCommand CurrentCommand
    {
        get
        {
            if (_mov.ReactValue < 0) return null;
            if (_mov.ReactValue >= _commands.Count) return null;

            return _commands[_mov.ReactValue];
        }
    }


    public virtual void AddCommand(TCommand command)
    {
        _commands.Add(command);
    }
    public virtual void AddCommands(ICollection<TCommand> commands)
    {
        _commands.AddRange(commands);
        if (_mov.ReactValue < 0)
            _mov.ReactValue = 0;
    }

    public override void UpdateTick()
    {
        if (_commands.Any() && CurrentCommand != null)
        {
            if (CurrentCommand.IsDone)
            {
                _mov.ReactValue++;
                if (CurrentCommand == null)
                {
                    _commands.Clear();
                    _mov.ReactValue = -1;

                    return;
                }
                else
                {
                    CurrentCommand.Start();
                }

            }

            if (CurrentCommand.State != CommandState.InProgress)
                CurrentCommand.Start();

            CurrentCommand.UpdateTick();
        }

    }
    public override void FixedTick()
    {
        if (_commands.Any() && CurrentCommand != null)
        {
            CurrentCommand?.FixedTick();
        }
    }
    public override void LateTick()
    {
        if (_commands.Any() && CurrentCommand != null)
        {
            CurrentCommand?.LateTick();
        }
    }

    public virtual void Clear()
    {
        CurrentCommand?.Cancel();
        _commands.Clear();
        _mov.ReactValue = -1;
    }
}


[System.Serializable]
public abstract class Command<T> where T : Entity
{
    public virtual string AssemblyQualifiedName => GetType().AssemblyQualifiedName;

    public string UID;

    public object CommandsRunner;
    public abstract T EntityRef { get; }

    public CommandState State;

    public Command() { }
    public Command(object commandsRunner)
    {
        CommandsRunner = commandsRunner;
        UID = Guid.NewGuid().ToString();
    }

    public virtual bool IsDone
    {
        get
        {
            return State == CommandState.Cancelled || State == CommandState.Completed || State == CommandState.Failed;
        }
    }

    public virtual void Start() { State = CommandState.InProgress; }

    public virtual void UpdateTick() { if (IsDone || State != CommandState.InProgress) return; }
    public virtual void FixedTick() { if (IsDone || State != CommandState.InProgress) return; }
    public virtual void LateTick() { if (IsDone || State != CommandState.InProgress) return; }

    public virtual void Load(CommandSD<T> sd, object commandsRunner)
    {
        CommandsRunner = commandsRunner;

        UID = sd.UID;
        State = sd.State;
    }
    public virtual void RefreshReferences(CommandSD<T> sd) { }
    public virtual void PostRefreshReferences(CommandSD<T> sd) { }


    public virtual void Complete() { State = CommandState.Completed; }
    public virtual void Cancel() { State = CommandState.Cancelled; }
    public virtual void Fail() { State = CommandState.Failed; }
}

[System.Serializable]
public class CommandSD<T> where T : Entity
{
    public string AssemblyQualifiedName;
    public string UID;
    public CommandState State;

    public CommandSD(Command<T> command)
    {
        AssemblyQualifiedName = command.AssemblyQualifiedName;
        UID = command.UID;
        State = command.State;
    }
}


public enum CommandState : byte
{
    Idle,
    InProgress,
    Cancelled,
    Completed,
    Failed
}