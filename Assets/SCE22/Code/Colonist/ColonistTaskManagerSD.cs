using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class ColonistTaskManagerSD : EntityComponentSD
{
    public List<ColonistOrderSD> Orders;
    public int Mov;

    public ColonistTaskManagerSD(ColonistTaskManager component) : base(component)
    {
        if (component.Mov.ReadOnlyValue >= 0)
        {
            Mov = component.Mov.ReadOnlyValue;
            Orders = new();

            foreach (var order in component.Orders.ReadonlyList)
                Orders.Add(order.GetSD());
        }
    }
}
