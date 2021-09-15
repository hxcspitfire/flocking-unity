using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;

public struct MovementJob : IJobParallelForTransform
{
  public float speed;
  public float deltaTime;

  public void Execute(int index, TransformAccess transform)
  {
    Vector3 pos = transform.position;
    pos += speed * deltaTime * (transform.rotation * new Vector3(1.0f, 0.0f, 0.0f));
    transform.position = pos;
  }
}
