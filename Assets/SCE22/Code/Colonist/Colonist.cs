using System.Collections;
using System.Collections.Generic;
using TMPro;
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
            if (_navMeshAgentCached == null)
                _navMeshAgentCached = GetComponent<NavMeshAgent>();
            return _navMeshAgentCached;
        }
    }

    [field: SerializeField] public float _baseSpeed;
    public float RealSpeed
    {
        get
        {
            return _baseSpeed * GameTime.Singleton.TimeMultiplier.ReadOnlyValue;
        }
    }

    protected override void OnTimeMultiplayerChanged()
    {
        base.OnTimeMultiplayerChanged();
        NavAgent.speed = RealSpeed;
        Debug.Log("TIME CHANGED");
    }
}
