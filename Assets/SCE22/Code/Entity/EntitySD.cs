using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntitySD
{
    public string PrefabUID;
    public string UID;
    public SType.Vector3 Position = Vector3.zero;
    public SType.Vector3 Rotation = Vector3.zero;

    public SType.SEntity Reservation = null;

    public EntitySD() { }
    public EntitySD(Entity entity)
    {
        PrefabUID = (entity.Prefab != null) ? entity.Prefab.UID : "404-error";
        UID = entity.UID;

        Position = entity.RealPosition;
        Rotation = entity.RealRotation;

        if (entity.Reservation.ReadOnlyValue != null)
            Reservation = entity.Reservation.ReadOnlyValue.Entity;
    }
}
