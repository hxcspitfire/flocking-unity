using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidCollision : MovementRule
{
  public struct Intersection
  {
    public bool hit;
    public Vector2 point;
  }

  public BoidCollision()
  {
  }

  public override void Update(Boid boid)
  {
    displacement = Vector2.zero;

    Vector2 p1 = new Vector2(
      boid.position.x,
      boid.position.y);

    int count = 0;
    for (int i = 0; i < Boid.mBoids.Count; ++i)
    {
      float dist = (p1 - Boid.mBoids[i].position).magnitude;
      if(dist >0.0f && dist < boid.SeparationDistance)
      {
        Vector2 dir = (p1 - Boid.mBoids[i].position);
        dir.Normalize();
        displacement += dir;
        count++;
      }
    }
    if(count > 0)
    {
      displacement /= (float)count;
    }
  }
}
