using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlignVelocity : MovementRule
{
  public AlignVelocity()
  {
  }

  public override void Update(Boid boid)
  {
    displacement = Vector2.zero;

    Vector2 p1 = new Vector2(
      boid.position.x,
      boid.position.y);

    for (int i = 0; i < Boid.mBoids.Count; ++i)
    {
      displacement += Boid.mBoids[i].last_displacement;
    }
    if (Boid.mBoids.Count > 0)
    {
      displacement /= (float)Boid.mBoids.Count;
    }
  }
}
