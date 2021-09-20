using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

// This system updates all entities in the scene with both a RotationSpeed_SpawnAndRemove and Rotation component.

// ReSharper disable once InconsistentNaming
public partial class BoidMovementSystem : SystemBase
{
  // OnUpdate runs on the main thread.
  protected override void OnUpdate()
  {
    var deltaTime = Time.DeltaTime;

    Entities
      .WithName("BoidSpeed")
      .ForEach((ref Translation trans, in Rotation rotation, in BoidSpeed boidSpeed) =>
      {
        float3 forward = math.mul(rotation.Value, new float3(1, 0, 0));
        trans.Value += forward * boidSpeed.Speed * deltaTime;
      })
      .ScheduleParallel();
  }
}
