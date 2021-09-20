using Unity.Entities;

public struct Spawner_FromEntity : IComponentData
{
  public float MinX;
  public float MaxX;
  public float MinY;
  public float MaxY;
  public float MinZ;
  public float MaxZ;
  public int Count;
  public Entity Prefab;
}
