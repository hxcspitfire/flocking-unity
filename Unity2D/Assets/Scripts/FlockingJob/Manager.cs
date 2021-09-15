using System.Collections;
using System.Collections.Generic;
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
  #endregion

  TransformAccessArray transforms;
  MovementJob movementJob;
  JobHandle movementJobHandle;

  private void OnDisable()
  {
    movementJobHandle.Complete();
    transforms.Dispose();
  }

  private void Start()
  {
    transforms = new TransformAccessArray(0, -1);
    AddBoids(BoidCount);
  }

  void AddBoids(int count)
  {
    movementJobHandle.Complete();
    transforms.capacity = transforms.length + count;
    for(int i = 0; i < count; ++i)
    {
      float x = Random.Range(MinX, MaxX);
      float y = Random.Range(MinY, MaxY);
      GameObject obj = Instantiate(PrefabBoid, new Vector3(x, y, 0.0f), Quaternion.identity);
      transforms.Add(obj.transform);
    }
    BoidCount = transforms.length;
    UiJob.SetBoidCount(BoidCount);
  }

  private void Update()
  {
    movementJobHandle.Complete();
    HandleInputs();

    movementJob = new MovementJob()
    {
      speed = MaxSpeed,
      deltaTime = Time.deltaTime
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
}
