using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Jobs;

public struct MovementJob : IJobParallelForTransform
{
  public float speed;
  public float deltaTime;
  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Vector3> targetDirection;
  public float rotationSpeed;

  public void Execute(int index, TransformAccess transform)
  {
    Vector3 dir = targetDirection[index];
    dir.Normalize();

    Vector3 rotatedVectorToTarget =
      Quaternion.Euler(0, 0, 90) *
      dir;

    Quaternion targetRotation = Quaternion.LookRotation(
      forward: Vector3.forward,
      upwards: rotatedVectorToTarget);

    transform.rotation = Quaternion.RotateTowards(
      transform.rotation,
      targetRotation,
      rotationSpeed * deltaTime);

    Vector3 pos = transform.position;
    pos += speed * deltaTime * (transform.rotation * new Vector3(1.0f, 0.0f, 0.0f));
    transform.position = pos;
  }
}

public struct RandomMovementJob : IJobParallelForTransform
{
  public NativeArray<Vector3> result;
  public float weightRandom;
  public Unity.Mathematics.Random rnd; 

  public void Execute(int index, TransformAccess transform)
  {      
    float angle = transform.rotation.eulerAngles.z;

    float rand = rnd.NextFloat(0.0f, 2.0f);
    if (rand > 1.0f)
    {
      angle += 45.0f;
    }
    else
    {
      angle -= 45.0f;
    }
    //Debug.Log("Angle: " + angle.ToString("F2"));
    //Debug.Log("Random: " + rand.ToString("F2"));
    Vector3 dir = Vector3.zero;
    dir.x = Mathf.Cos(Mathf.Deg2Rad* angle);
    dir.y = Mathf.Sin(Mathf.Deg2Rad * angle);

    result[index] = dir * weightRandom;
    result[index].Normalize();
  }
}
