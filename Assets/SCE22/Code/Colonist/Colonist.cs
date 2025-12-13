using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Colonist : EntityWithComponents
{
    private NavMeshAgent _navMeshAgentCached;
    public NavMeshAgent NavAgent
    {
        get
        {
            if (_navMeshAgentCached)
                _navMeshAgentCached = GetComponent<NavMeshAgent>();
            return _navMeshAgentCached;
        }
    }


}
