using System;
using Unity.Entities;

[Serializable]
public struct BoidSpeed : IComponentData
{
  public float Speed;
}
