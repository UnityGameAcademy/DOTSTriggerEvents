using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;

public class PickupOnTriggerSystem : JobComponentSystem
{
    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
    }


    struct PickupOnTriggerSystemJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<PickupTag> allPickups;
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> allPlayers;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            if (allPickups.Exists(entityA) && allPlayers.Exists(entityB))
            {
                UnityEngine.Debug.Log("entity A " + entityA + " has collided with entityB " + entityB);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        PickupOnTriggerSystemJob pickupJob = new PickupOnTriggerSystemJob();
        pickupJob.allPlayers = GetComponentDataFromEntity<PlayerTag>();
        pickupJob.allPickups = GetComponentDataFromEntity<PickupTag>();

        JobHandle jobHandle = pickupJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld,
            inputDependencies);

        jobHandle.Complete();

        return jobHandle;
    }
}