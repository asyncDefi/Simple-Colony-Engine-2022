using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TicksHandler : MonoBehaviour
{
    public virtual void UpdateTick() { }
    public virtual void FixedTick() { }
    public virtual void LateTick() { }
}
