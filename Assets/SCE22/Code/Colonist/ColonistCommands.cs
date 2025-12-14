using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ColonistCommands
{
    public sealed class ColonistCommand_GoToCords : ColonistCommand
    {
        public Vector3 Cords;
        public override ColonistCommandSD CCSD => new ColonistCommand_GoToCordsSD(this);

        private NavMeshAgent _cachedAgent;
        private NavMeshAgent _agent
        {
            get
            {
                if (_cachedAgent == null)
                    _cachedAgent = this.EntityRef.GetComponent<NavMeshAgent>();

                return _cachedAgent;
            }
        }

        public ColonistCommand_GoToCords() : base() { }
        public ColonistCommand_GoToCords(ColonistCommandsRunner runner, Vector3 cords) : base(runner)
        {
            Cords = cords;
        }

        public override void Start()
        {
            base.Start();

            _agent.SetDestination(Cords);
        }
        public override void UpdateTick()
        {
            base.UpdateTick();

            float distance = Vector3.Distance(this.EntityRef.RealPosition, Cords);
            if (distance <= _agent.stoppingDistance)
                Complete();

        }

        public override void Load(CommandSD<Colonist> sd, object commandsRunner)
        {
            base.Load(sd, commandsRunner);
            ColonistCommand_GoToCordsSD selfSD = sd as ColonistCommand_GoToCordsSD;
            if (selfSD != null)
            {
                Cords = selfSD.Cords;
            }
        }
        public override void PostRefreshReferences(CommandSD<Colonist> sd)
        {
            base.PostRefreshReferences(sd);
            if (State == CommandState.InProgress)
            {
                _agent.SetDestination(Cords);
            }
        }

        [System.Serializable]
        public class ColonistCommand_GoToCordsSD : ColonistCommandSD
        {
            public SType.Vector3 Cords;

            public ColonistCommand_GoToCordsSD(ColonistCommand_GoToCords command) : base(command)
            {
                Cords = command.Cords;
            }
        }
    }
}