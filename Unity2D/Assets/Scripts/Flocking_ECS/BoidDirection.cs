using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct BoidDirection : IComponentData
{
  public float3 Direction;
}
