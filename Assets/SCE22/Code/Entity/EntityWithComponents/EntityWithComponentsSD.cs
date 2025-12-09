using System.Collections.Generic;

[System.Serializable]
public class EntityWithComponentsSD : EntitySD
{
    public List<EntityComponentSD> Components = new();

    public EntityWithComponentsSD() : base() { }
    public EntityWithComponentsSD(EntityWithComponents entity) : base(entity)
    {
        foreach (var component in entity.Components.ReadonlyList)
            Components.Add(component.SD);
    }
}