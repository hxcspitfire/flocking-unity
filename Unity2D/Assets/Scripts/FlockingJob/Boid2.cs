using UnityEngine;

public struct Boid2
{
  public Vector3 pos;
  public Vector3 direction;
  public float speed;
}

public struct Obstacle2
{
  public Vector3 position;
  public float avoidanceRadius;
}