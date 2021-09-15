using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Jobs;

public class Manager : Patterns.Singleton<Manager>
{
  #region Manager stuff
  public GameObject PrefabBoid;
  public float MaxSpeed = 5.0f;
  public int BoidCount = 1;
  public int BoidIncrementCount = 1;
  public float MinX = -100.0f;
  public float MaxX = 100.0f;
  public float MinY = -100.0f;
  public float MaxY = 100.0f;
  public UI_Job UiJob;

  public bool useRandomRule = true;
  public float TickDurationRandom = 1.0f;
  #endregion

  TransformAccessArray transforms;
  MovementJob movementJob;
  JobHandle movementJobHandle;
  RandomMovementJob randomMovementJob;
  JobHandle randomMovementJobHandle;
  [NativeDisableContainerSafetyRestriction]
  public NativeArray<Vector3> targetDirections;

  System.Random mSeed;

  private void OnDisable()
  {
    movementJobHandle.Complete();
    randomMovementJobHandle.Complete();
    transforms.Dispose();
    targetDirections.Dispose();
  }

  private void Start()
  {
    mSeed = new System.Random();
    transforms = new TransformAccessArray(0, -1);
    AddBoids(BoidCount);

    StartCoroutine(Coroutine_Random());
  }

  void AddBoids(int count)
  {
    movementJobHandle.Complete();
    randomMovementJobHandle.Complete();

    transforms.capacity = transforms.length + count;
    for (int i = 0; i < count; ++i)
    {
      float x = Random.Range(MinX, MaxX);
      float y = Random.Range(MinY, MaxY);
      GameObject obj = Instantiate(PrefabBoid, new Vector3(x, y, 0.0f), Quaternion.identity);
      transforms.Add(obj.transform);
    }
    targetDirections = new NativeArray<Vector3>(transforms.length, Allocator.TempJob);
    BoidCount = transforms.length;
    UiJob.SetBoidCount(BoidCount);
  }

  private void Update()
  {
    movementJobHandle.Complete();
    randomMovementJobHandle.Complete();

    HandleInputs();

    movementJob = new MovementJob()
    {
      speed = 5.0f,
      deltaTime = Time.deltaTime,
      targetDirection = targetDirections,
      rotationSpeed = 100.0f
    };

    movementJobHandle = movementJob.Schedule(transforms);

    JobHandle.ScheduleBatchedJobs();
  }

  void HandleInputs()
  {
    if (EventSystem.current.IsPointerOverGameObject() ||
       enabled == false)
    {
      return;
    }

    if(Input.GetKeyDown(KeyCode.Space))
    {
      AddBoids(BoidIncrementCount);
    }
  }

  IEnumerator Coroutine_Random()
  {
    while (true)
    {
      if (useRandomRule)
      {
        randomMovementJob = new RandomMovementJob()
        {
          result = targetDirections,
          weightRandom = 1.0f,
          rnd = new Unity.Mathematics.Random((uint)mSeed.Next())
        };

        randomMovementJobHandle = randomMovementJob.Schedule(transforms);
      }
      yield return new WaitForSeconds(TickDurationRandom);
    }
  }
}
