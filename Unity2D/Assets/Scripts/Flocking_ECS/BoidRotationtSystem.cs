using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.

// ReSharper disable once InconsistentNaming
public partial class BoidRotationSystem : SystemBase
{
  // OnUpdate runs on the main thread.
  [BurstCompile]
  protected override void OnUpdate()
  {
    var deltaTime = Time.DeltaTime;

    Entities
      .WithName("BoidDirection")
      .ForEach((ref Rotation rotation, in BoidDirection boidDirection) =>
      {
        //rotation.Value = math.mul(math.normalize(rotation.Value), quaternion.LookRotation(boidDirection.Direction, math.up()));
        rotation.Value = quaternion.LookRotation(boidDirection.Direction, math.up());
      })
      .ScheduleParallel();
  }
}
