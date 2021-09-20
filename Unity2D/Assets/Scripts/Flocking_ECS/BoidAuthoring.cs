using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[AddComponentMenu("DOTS Samples/SpawnAndRemove/Boid")]
[ConverterVersion("joe", 1)]
public class BoidAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
  public float MaxSpeed = 20.0f;

  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
  {
    dstManager.AddComponentData(entity, new BoidSpeed { Speed = MaxSpeed });
    dstManager.AddComponentData(entity, new BoidDirection { Direction = new float3(1.0f, 0.0f, 0.0f) });
  }
}
