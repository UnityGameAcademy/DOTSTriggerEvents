using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;


public class TargetToDirectionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.
            WithoutBurst().
            WithNone<PlayerTag>().
            WithAll<ChaserTag>().
            ForEach((ref MoveData moveData, ref Rotation rot, in Translation pos, in TargetData targetData) =>
            {
                ComponentDataFromEntity<Translation> allTranslations = GetComponentDataFromEntity<Translation>(true);
                if (!allTranslations.Exists(targetData.targetEntity))
                {
                    return;
                }

                Translation targetPos = allTranslations[targetData.targetEntity];
                float3 dirToTarget = targetPos.Value - pos.Value;
                moveData.direction = dirToTarget;

            }).Run();
    }
}