using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(TransformSystemGroup))]
public class FaceDirectionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.
            ForEach((ref Rotation rot, in Translation pos, in MoveData moveData) =>
            {
                FaceDirection(ref rot, moveData);

            }).Schedule();
    }

    private static void FaceDirection(ref Rotation rot, MoveData moveData)
    {
        if (!moveData.direction.Equals(float3.zero))
        {
            quaternion targetRotation = quaternion.LookRotationSafe(moveData.direction, math.up());
            rot.Value = math.slerp(rot.Value, targetRotation, moveData.turnSpeed);
        }
    }
}
