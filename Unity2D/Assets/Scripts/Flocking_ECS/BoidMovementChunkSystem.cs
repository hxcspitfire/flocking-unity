//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Profiling;
//using Unity.Transforms;

//// This system updates all entities in the scene with both a RotationSpeed_IJobChunkStructBased and Rotation component.

//// ReSharper disable once InconsistentNaming
//[BurstCompile]
//public struct BoidMovementChunkSystem : ISystemBase
//{
//  //EntityQuery m_RotationGroup;
//  EntityQuery m_TranslationGroup;

//  public void OnCreate(ref SystemState state)
//  {
//    // Cached access to a set of ComponentData based on a specific query
//    //m_RotationGroup = state.GetEntityQuery(typeof(Rotation), ComponentType.ReadOnly<BoidDirection>());
//    m_TranslationGroup = state.GetEntityQuery(typeof(Translation), ComponentType.ReadOnly<BoidDirection>());
//  }

//  // Use the [BurstCompile] attribute to compile a job with Burst. You may see significant speed ups, so try it!
//  [BurstCompile]
//  struct MovementSpeedJob : IJobChunk
//  {
//    public float DeltaTime;
//    [ReadOnly] public ComponentTypeHandle<Rotation> RotationTypeHandle;
//    public ComponentTypeHandle<Translation> TranslationTypeHandle;
//    [ReadOnly] public ComponentTypeHandle<BoidSpeed> BoidSpeedTypeHandle;

//    public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
//    {
//      var chunkRotations = chunk.GetNativeArray(RotationTypeHandle);
//      var chunkTranslations = chunk.GetNativeArray(TranslationTypeHandle);
//      var chunkSpeeds = chunk.GetNativeArray(BoidSpeedTypeHandle);

//      for (var i = 0; i < chunk.Count; i++)
//      {
//        var speed = chunkSpeeds[i];
//        var trans = chunkTranslations[i];
//        var rotation = chunkRotations[i];

//        float3 forward = math.mul(rotation.Value, new float3(1, 0, 0));
//        // Rotate something about its up vector at the speed given by RotationSpeed_IJobChunkStructBased.
//        chunkTranslations[i] = new Translation
//        {
//          Value = trans.Value + forward * speed.Speed * DeltaTime
//        };
//      }
//    }
//  }

//  // OnUpdate runs on the main thread.
//  // Note that from 2020.2 the update function itself can be burst compiled when using struct systems.
//#if UNITY_2020_2_OR_NEWER
//  [BurstCompile]
//#endif
//  public void OnUpdate(ref SystemState state)
//  {
//    // Explicitly declare:
//    // - Read-Write access to Rotation
//    // - Read-Only access to RotationSpeed_IJobChunkStructBased
//    var rotations = state.GetComponentTypeHandle<Rotation>(true);
//    var translations = state.GetComponentTypeHandle<Translation>();
//    var boidSpeeds = state.GetComponentTypeHandle<BoidSpeed>(true);

//    var job = new MovementSpeedJob()
//    {
//      RotationTypeHandle = rotations,
//      TranslationTypeHandle = translations,
//      BoidSpeedTypeHandle = boidSpeeds,
//      DeltaTime = state.Time.DeltaTime
//    };

//    state.Dependency = job.ScheduleSingle(m_TranslationGroup, state.Dependency);
//  }

//  public void OnDestroy(ref SystemState state)
//  {
//  }
//}
