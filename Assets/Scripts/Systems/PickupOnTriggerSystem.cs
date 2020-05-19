using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class PickupOnTriggerSystem : JobComponentSystem
{

    private BuildPhysicsWorld buildPhysicsWorld;
    private StepPhysicsWorld stepPhysicsWorld;

    public EndSimulationEntityCommandBufferSystem endSimulationSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();

        endSimulationSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    private struct CollisionJob : ICollisionEventsJob
    {
        public void Execute(CollisionEvent collisionEvent)
        {

        }
    }

    private struct PickupOnTriggerJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<DestroyableTag> destroyers;
        [ReadOnly] public ComponentDataFromEntity<DestroyOnTriggerTag> destructibles;

        public EntityCommandBuffer ecb;

        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.Entities.EntityA;
            Entity entityB = triggerEvent.Entities.EntityB;


            // logic // destroy one entity

            // if entityA and entityB can collide, destroy each smashable Entity
            if (destructibles.Exists(entityA) && destroyers.Exists(entityB))
            {
                ecb.DestroyEntity(entityA);
                Debug.Log("mark for removal");
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {

        // create the job
        PickupOnTriggerJob pickupOnTriggerJob = new PickupOnTriggerJob();

        // initialize the job
        pickupOnTriggerJob.destroyers = GetComponentDataFromEntity<DestroyableTag>();
        pickupOnTriggerJob.destructibles = GetComponentDataFromEntity<DestroyOnTriggerTag>();

        pickupOnTriggerJob.ecb = endSimulationSystem.CreateCommandBuffer();

        // schedule the job
        inputDependencies = pickupOnTriggerJob.Schedule(stepPhysicsWorld.Simulation, ref buildPhysicsWorld.PhysicsWorld, inputDependencies);

        // complete the job for structural changes (must happen on Main Thread)
        inputDependencies.Complete();

        //        Creating entities
        //        Deleting entities
        //        Adding components to an entity
        //        Removing components from an entity
        //        Changing the value of shared components

        // Now that the job is set up, schedule it to be run.
        return inputDependencies;
    }
}
