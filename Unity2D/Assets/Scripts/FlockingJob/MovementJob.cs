using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

public struct MovementJob : IJobParallelForTransform
{
  public float MaxSpeed;
  public float speed;
  public float deltaTime;
  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Vector3> targetDirection;
  public float rotationSpeed;
  public float MinX;
  public float MaxX;
  public float MinY;
  public float MaxY;

  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Obstacle2> obstacles;
  public int obstaclesCount;
  public float weightAvoidObstacles;

  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Vector3> targetDirection_Flocking;

  public bool bounceWall;

  public NativeArray<Boid2> lastFrameData;

  public void Execute(int index, TransformAccess transform)
  {
    float flockingSpeed = targetDirection_Flocking[index].magnitude;
    Vector3 dir = targetDirection[index] + targetDirection_Flocking[index];
    dir.Normalize();

    dir = Rule_CrossBorder(dir, transform);
    dir = ObstacleVoidance(dir, transform);
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

    float totalSpeed = speed + flockingSpeed;
    if(totalSpeed > MaxSpeed)
    {
      totalSpeed = MaxSpeed;
    }
    Vector3 pos = transform.position;
    pos += speed * deltaTime * (transform.rotation * new Vector3(1.0f, 0.0f, 0.0f));
    transform.position = pos;

    lastFrameData[index] = new Boid2()
    {
      pos = pos,
      speed = speed,
      direction = dir
    };
  }

  Vector3 Rule_CrossBorder(Vector3 TargetDirection, TransformAccess transform)
  {
    if (bounceWall)
    {
      Vector3 pos = transform.position;
      if (transform.position.x + 5.0f > MaxX)
      {
        TargetDirection.x = -1.0f;
      }
      if (transform.position.x - 5.0f < MinX)
      {
        TargetDirection.x = 1.0f;
      }
      if (transform.position.y + 5.0f > MaxY)
      {
        TargetDirection.y = -1.0f;
      }
      if (transform.position.y - 5.0f < MinY)
      {
        TargetDirection.y = 1.0f;
      }
      TargetDirection.Normalize();
    }
    else
    {
      Vector3 pos = transform.position;
      if (transform.position.x > MaxX)
      {
        pos.x = MinX;
      }
      if (transform.position.x < MinX)
      {
        pos.x = MaxX;
      }
      if (transform.position.y > MaxY)
      {
        pos.y = MinY;
      }
      if (transform.position.y < MinY)
      {
        pos.y = MaxX;
      }
      transform.position = pos;
    }
    return TargetDirection;
  }

  Vector3 ObstacleVoidance(Vector3 TargetDirection, TransformAccess transform)
  {
    for (int j = 0; j < obstaclesCount; ++j)
    {
      float dist = (
        obstacles[j].position -
        transform.position).magnitude;
      if (dist < obstacles[j].avoidanceRadius)
      {
        Vector3 targetDirection = (
          transform.position -
          obstacles[j].position).normalized;

        TargetDirection += targetDirection * weightAvoidObstacles;
        TargetDirection.Normalize();
      }
    }
    return TargetDirection;
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

    Vector3 dir = Vector3.zero;
    dir.x = Mathf.Cos(Mathf.Deg2Rad* angle);
    dir.y = Mathf.Sin(Mathf.Deg2Rad * angle);

    result[index] = dir * weightRandom;
    result[index].Normalize();
  }
}

struct FlockingJob : IJobParallelFor
{
  [ReadOnly]
  public bool useCohesionRule;
  [ReadOnly]
  public bool useAlignmentRule;
  [ReadOnly]
  public bool useSeparationRule;

  [ReadOnly]
  public float visibility;
  [ReadOnly]
  public int boidsCount;
  [ReadOnly]
  public float weightAlignment;
  [ReadOnly]
  public float weightSeparation;
  [ReadOnly]
  public float separationDistance;
  [ReadOnly]
  public float weightCohesion;
  [ReadOnly]
  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Boid2> lastFrameData;
  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Vector3> targetVelocities;

  public void Execute(int i)
  {
    Vector3 flockDir = Vector3.zero;
    Vector3 separationDir = Vector3.zero;
    Vector3 cohesionDir = Vector3.zero;

    float speed = 0.0f;
    float separationSpeed = 0.0f;

    int count = 0;
    int separationCount = 0;
    Vector3 steerPos = Vector3.zero;

    for (int j = 0; j < boidsCount; ++j)
    {
      float dist = (lastFrameData[i].pos - lastFrameData[j].pos).magnitude;
      if (i != j && dist < visibility)
      {
        speed += lastFrameData[j].speed;
        flockDir += lastFrameData[j].direction;
        steerPos += lastFrameData[j].pos;
        count++;
      }
      if (i != j)
      {
        if (dist < separationDistance)
        {
          Vector3 targetDirection = (
            lastFrameData[i].pos -
            lastFrameData[j].pos).normalized;

          separationDir += targetDirection;
          separationSpeed += dist * weightSeparation;
        }
      }
    }
    if (count > 0)
    {
      speed = speed / count;
      flockDir = flockDir / count;
      flockDir.Normalize();

      steerPos = steerPos / count;
    }

    if (separationCount > 0)
    {
      separationSpeed = separationSpeed / count;
      separationDir = separationDir / separationSpeed;
      separationDir.Normalize();
    }

    targetVelocities[i] = 
      flockDir * speed * (useAlignmentRule ? weightAlignment : 0.0f) + 
      separationDir * separationSpeed * (useSeparationRule? weightSeparation:0.0f) + 
      (steerPos - lastFrameData[i].pos) * (useCohesionRule?weightCohesion:0.0f);
  }
}
