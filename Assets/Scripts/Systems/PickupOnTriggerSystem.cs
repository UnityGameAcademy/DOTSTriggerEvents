using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;

[UpdateAfter(typeof(EndFramePhysicsSystem))] 
public class PickupOnTriggerSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct PickupOnTriggerSystemJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<PickupTag> allPickups;
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> allPlayers;

        public EntityCommandBuffer entityCommandBuffer;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            if (allPickups.Exists(entityA) && allPickups.Exists(entityB))
            {
                return;
            }

            if (allPickups.Exists(entityA) && allPlayers.Exists(entityB))
            {
                //UnityEngine.Debug.Log("Pickup Entity A: " + entityA + " collided with Player Entity B: " + entityB);
                entityCommandBuffer.DestroyEntity(entityA);
            }
            else if (allPlayers.Exists(entityA) && allPickups.Exists(entityB))
            {
                //UnityEngine.Debug.Log("Player Entity A: " + entityA + " collided with PickUp Entity B: " + entityB);
                entityCommandBuffer.DestroyEntity(entityB);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new PickupOnTriggerSystemJob();
        job.allPickups = GetComponentDataFromEntity<PickupTag>(true);
        job.allPlayers = GetComponentDataFromEntity<PlayerTag>(true);
        job.entityCommandBuffer = commandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle = job.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld,
            inputDependencies);


        //jobHandle.Complete();
        commandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}