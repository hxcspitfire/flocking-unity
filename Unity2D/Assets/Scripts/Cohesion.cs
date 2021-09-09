using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cohesion : MovementRule
{
  public Cohesion()
  {
  }

  public override void Update(Boid boid)
  {
    displacement = Vector2.zero;

    Vector2 p1 = new Vector2(
      boid.position.x,
      boid.position.y);

    Vector2 center = Vector2.zero;
    for (int i = 0; i < Boid.mBoids.Count; ++i)
    {
      center += Boid.mBoids[i].position;
    }
    if (Boid.mBoids.Count > 0)
    {
      center /= (float)Boid.mBoids.Count;
    }

    Vector2 dir = center - boid.position;
    float dist = dir.magnitude;
    dir.Normalize();
    displacement = dir * dist / 1000.0f;
  }

}
