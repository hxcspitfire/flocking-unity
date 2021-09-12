using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class Obstacle : MonoBehaviour
{
  public float AvoidanceRadiusMultFactor = 1.5f;
  public float AvoidanceRadius
  {
    get
    {
      return mCollider.radius * 2 * AvoidanceRadiusMultFactor;
    }
  }

  CircleCollider2D mCollider;

  void Start()
  {
    mCollider = GetComponent<CircleCollider2D>();
  }
}
