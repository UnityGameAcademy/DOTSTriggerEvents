
using Unity.Entities;

[GenerateAuthoringComponent]
public struct TargetData : IComponentData
{
    public Entity targetEntity;
}
