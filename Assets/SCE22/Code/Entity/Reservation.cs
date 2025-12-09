using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class Reservation
{
    public readonly Entity Entity;

    public Reservation() { }
    public Reservation(Entity entity)
    {
        Entity = entity;
    }
}
