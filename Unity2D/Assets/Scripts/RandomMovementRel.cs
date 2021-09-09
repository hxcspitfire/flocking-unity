using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovementRel : MonoBehaviour
{
  public float Speed = 5.0f;
  public float SpeedRotation = 200.0f;
  public float ChangeRotDuration = 0.5f;
  public float ChangeRotAngleRange = 45.0f;
  public BoxCollider2D Boundary;

  float mAngle = 0.0f;

  public Boid boid;

  List<MovementRule> mRules = new List<MovementRule>();

  void Start()
  {
  }

  public void AddRule(MovementRule rule)
  {
    mRules.Add(rule);
  }

  //private void FixedUpdate()
  //{
  //  Move(Time.fixedDeltaTime);
  //}

  private void Update()
  {
    Move(Time.deltaTime);
  }

  public void Rotate(float angle)
  {

  }

  private void Move(float dt)
  {
    float x = Mathf.Cos(Mathf.Deg2Rad * mAngle);
    float y = Mathf.Sin(Mathf.Deg2Rad * mAngle);

    Vector2 dir = new Vector2(x, y);
    Vector2 disp = dir * Speed * dt;

    boid.displacement = disp;
    //boid.angleInDegrees += mAngle;
    boid.angleRelativeInDegrees = mAngle;

    // apply all rules
    for(int i = 0; i < mRules.Count; ++i)
    {
      mRules[i].Update(boid);
    }
  }
}
