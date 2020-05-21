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

    private EndSimulationEntityCommandBufferSystem commandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();

        commandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

    }

    struct PickupOnTriggerSystemJob : ITriggerEventsJob
    {
        public EntityCommandBuffer entityCommandBuffer;
        [ReadOnly] public ComponentDataFromEntity<PickupTag> allPickups;
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> allPlayers;

        public bool isTriggered;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;

            // return if both Entities are triggers
            if (allPickups.Exists(entityA) && allPickups.Exists(entityB))
            {
                return;
            }

            if (isTriggered)
            {
                return;
            }

            // if Entity A is a pickup, and Entity B is the Player
            if (allPickups.Exists(entityA) && allPlayers.Exists(entityB))
            {

                //UnityEngine.Debug.Log("Pickup entity A " + entityA + " has collided with Player entityB " + entityB);
                entityCommandBuffer.DestroyEntity(entityA);


                isTriggered = true;

            }
            // if Entity A is the player, and Entity B is the PickUp
            else if (allPickups.Exists(entityB) && allPlayers.Exists(entityA))
            {
                //UnityEngine.Debug.Log("Player Entity A " + entityA + " has collided with Pickup entityB " + entityB);
                entityCommandBuffer.DestroyEntity(entityB);
                isTriggered = true;
            }
        }
    }

    public static void DestroyEntityChildren(Entity entity)
    {

    }


    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        PickupOnTriggerSystemJob pickupJob = new PickupOnTriggerSystemJob();
        pickupJob.allPlayers = GetComponentDataFromEntity<PlayerTag>(true);
        pickupJob.allPickups = GetComponentDataFromEntity<PickupTag>(true);
        pickupJob.entityCommandBuffer = commandBufferSystem.CreateCommandBuffer();

        pickupJob.isTriggered = false;

        JobHandle jobHandle = pickupJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld,
            inputDependencies);

        jobHandle.Complete();

        return jobHandle;
    }
}