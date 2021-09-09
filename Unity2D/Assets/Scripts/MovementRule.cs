using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{
  public static float MaxSpeed = 10.0f;
  public static float MaxForce = 1.0f;
  public static float MaxSeparation = 2.0f;

  public static List<Boid> mBoids = new List<Boid>();

  //public Vector2 position;
  public float angleInDegrees
  {
    //get; set;
    get
    {
      return Mathf.Rad2Deg * Mathf.Atan2(displacement.y, displacement.x);
    }
  }

  public float angleRelativeInDegrees = 0.0f;

  public Vector2 position
  {
    get
    {
      return obj.transform.position;
    }
    set
    {
      obj.transform.position = value;
    }
  }

  public Vector2 last_displacement = new Vector2();
  public Vector2 displacement = new Vector2();

  public GameObject obj;

  public float SeparationDistance
  {
    get { return MaxSeparation; }
  }

  public VectorDrawing debugDrawing;

  public Vector2 velocity = new Vector2();
  public Vector2 acceleration = new Vector2();

  public static Vector2 Limit(Vector2 v, float f)
  {
    float size = v.magnitude;

    if (size > f)
    {
      v /= size;
    }
    return v;
  }
}

public class MovementRule
{
  public Vector2 displacement = new Vector2();

  public MovementRule()
  {

  }

  virtual public void Update(Boid boid)
  {

  }
}