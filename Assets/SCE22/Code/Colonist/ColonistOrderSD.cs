using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ColonistOrderSD
{
    public string STypeName;
    public string UID;
    public OrderState State;


    public ColonistOrderSD(ColonistOrder order)
    {
        STypeName = order.STypeName;
        UID = order.UID;
        State = order.State.ReadOnlyValue;
    }
}
