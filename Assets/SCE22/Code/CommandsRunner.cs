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

    public abstract CommandsRunnerSD<TEntity, TCommand> SD { get; }

    public virtual void Load(CommandsRunnerSD<TEntity, TCommand> sd)
    {
        Debug.Log($"[CommandsRunner] Loading with Mov={sd.Mov}, Commands count={sd.Commands.Count}");
        _mov.ReactValue = sd.Mov;
        foreach (var commandSD in sd.Commands)
        {
            try
            {
                var instance = Activator.CreateInstance(Type.GetType(commandSD.AssemblyQualifiedName)) as TCommand;
                instance.Load(commandSD, this);
                Debug.Log($"[CommandsRunner] Loaded command: {commandSD.AssemblyQualifiedName}, UID={commandSD.UID}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[CommandsRunner] Failed to load command: {commandSD.AssemblyQualifiedName}, Error: {e.Message}");
                continue;
            }

        }
    }
    public virtual void RefreshReferences(CommandsRunnerSD<TEntity, TCommand> sd)
    {
        Debug.Log($"[CommandsRunner] RefreshReferences for {sd.Commands.Count} commands");
        foreach (var commandSD in sd.Commands)
        {
            var instace = _commands.FirstOrDefault(com => com.UID == commandSD.UID);
            instace?.RefreshReferences(commandSD);
        }
    }
    public virtual void PostRefreshReferences(CommandsRunnerSD<TEntity, TCommand> sd)
    {
        Debug.Log($"[CommandsRunner] PostRefreshReferences for {sd.Commands.Count} commands");
        foreach (var commandSD in sd.Commands)
        {
            var instace = _commands.FirstOrDefault(com => com.UID == commandSD.UID);
            instace?.PostRefreshReferences(commandSD);
        }
    }

    public virtual void AddCommand(TCommand command)
    {
        Debug.Log($"[CommandsRunner] AddCommand: {command.GetType().Name}, UID={command.UID}");
        _commands.Add(command);
        if (Mov.ReadOnlyValue < 0)
            _mov.ReactValue = 0;

    }
    public virtual void AddCommands(ICollection<TCommand> commands)
    {
        Debug.Log($"[CommandsRunner] AddCommands: {commands.Count} commands");
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
                Debug.Log($"[CommandsRunner] Command done: {CurrentCommand.GetType().Name}, State={CurrentCommand.State}, Moving to next (Mov: {_mov.ReactValue} -> {_mov.ReactValue + 1})");
                _mov.ReactValue++;
                if (CurrentCommand == null)
                {
                    Debug.Log($"[CommandsRunner] All commands completed, clearing queue");
                    _commands.Clear();
                    _mov.ReactValue = -1;

                    return;
                }
                else
                {
                    Debug.Log($"[CommandsRunner] Starting next command: {CurrentCommand.GetType().Name}");
                    CurrentCommand.Start();
                }

            }

            if (CurrentCommand.State != CommandState.InProgress)
            {
                Debug.Log($"[CommandsRunner] Starting command: {CurrentCommand.GetType().Name}");
                CurrentCommand.Start();
            }

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
        Debug.Log($"[CommandsRunner] Clearing all commands (Count: {_commands.Count})");
        CurrentCommand?.Cancel();
        _commands.Clear();
        _mov.ReactValue = -1;
    }
}

[System.Serializable]
public sealed class CommandsRunnerSD<TEntity, TCommand> : EntityComponent where TEntity : Entity where TCommand : Command<TEntity>
{
    public int Mov;
    public List<CommandSD<TEntity>> Commands;

    public CommandsRunnerSD(CommandsRunner<TEntity, TCommand> commandsRunner)
    {
        Mov = commandsRunner.Mov.ReadOnlyValue;
        Commands = new();

        foreach (var command in commandsRunner.Commands)
            Commands.Add(command.SD);
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
        Debug.Log($"[Command] Created: {GetType().Name}, UID={UID}");
    }

    public virtual bool IsDone
    {
        get
        {
            return State == CommandState.Cancelled || State == CommandState.Completed || State == CommandState.Failed;
        }
    }

    public virtual void Start()
    {
        Debug.Log($"[Command] Start: {GetType().Name}, UID={UID}");
        State = CommandState.InProgress;
    }

    public virtual void UpdateTick() { if (State != CommandState.InProgress) return; }
    public virtual void FixedTick() { if (State != CommandState.InProgress) return; }
    public virtual void LateTick() { if (State != CommandState.InProgress) return; }

    public virtual CommandSD<T> SD { get; }

    public virtual void Load(CommandSD<T> sd, object commandsRunner)
    {
        Debug.Log($"[Command] Load: {GetType().Name}, UID={sd.UID}, State={sd.State}");
        CommandsRunner = commandsRunner;

        UID = sd.UID;
        State = sd.State;
    }
    public virtual void RefreshReferences(CommandSD<T> sd) { }
    public virtual void PostRefreshReferences(CommandSD<T> sd) { }


    public virtual void Complete()
    {
        Debug.Log($"[Command] Complete: {GetType().Name}, UID={UID}");
        State = CommandState.Completed;
    }
    public virtual void Cancel()
    {
        Debug.Log($"[Command] Cancel: {GetType().Name}, UID={UID}");
        State = CommandState.Cancelled;
    }
    public virtual void Fail()
    {
        Debug.Log($"[Command] Fail: {GetType().Name}, UID={UID}");
        State = CommandState.Failed;
    }
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