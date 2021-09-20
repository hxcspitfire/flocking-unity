using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

// ReSharper disable once InconsistentNaming
[AddComponentMenu("DOTS Samples/SpawnFromEntity/Spawner")]
public class SpawnerAuthoring_FromEntity : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
  public float MinX;
  public float MaxX;
  public float MinY;
  public float MaxY;
  public float MinZ;
  public float MaxZ;
  public int Count;
  public GameObject Prefab;

  // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
  public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
  {
    referencedPrefabs.Add(Prefab);
  }

  // Lets you convert the editor data representation to the entity optimal runtime representation
  public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
  {
    var spawnerData = new Spawner_FromEntity
    {
      // The referenced prefab will be converted due to DeclareReferencedPrefabs.
      // So here we simply map the game object to an entity reference to that prefab.
      Prefab = conversionSystem.GetPrimaryEntity(Prefab),
      Count = Count,
      MinX = MinX,
      MaxX = MaxX,
      MinY = MinY,
      MaxY = MaxY,
      MinZ = MinZ,
      MaxZ = MaxZ
    };
    dstManager.AddComponentData(entity, spawnerData);
  }
}
