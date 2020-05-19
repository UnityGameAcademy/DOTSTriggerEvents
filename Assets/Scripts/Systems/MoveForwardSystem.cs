using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class MoveForwardSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.
            WithAny<AsteroidTag, ChaserTag>().
            WithNone<PlayerTag>().
            ForEach((ref Translation pos, in MoveData moveData, in Rotation rot) =>
            {
                float3 forwardDirection = math.forward(rot.Value);
                pos.Value += forwardDirection * moveData.speed * deltaTime;

            }).ScheduleParallel();



    }
}
