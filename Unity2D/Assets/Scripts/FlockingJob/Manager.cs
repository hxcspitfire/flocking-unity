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

  public int MAX_BOIDS = 10000;
  public GameObject PrefabBoid;
  public float MaxSpeed = 5.0f;
  public int BoidCount = 1;
  public int BoidIncrementCount = 1;
  public float MinX = -100.0f;
  public float MaxX = 100.0f;
  public float MinY = -100.0f;
  public float MaxY = 100.0f;
  public UI_Job UiJob;

  [SerializeField]
  GameObject[] PrefabObstacles;

  public bool useRandomRule = true;
  public bool useCohesionRule = true;
  public bool useAlignmentRule = true;
  public bool useSeparationRule = true;

  public float TickDurationRandom = 1.0f;
  public float TickDuration = 0.1f;

  public float weightCohesion = 1.0f;
  public float weightAlignment = 1.0f;
  public float weightSeparation = 1.0f;

  public float weightAvoidObstacles = 1.0f;
  public float avoidanceRadiusMultiplier = 1.5f;

  public float visibility = 20.0f;
  public float separationDistance = 2.0f;

  public int NumBatches = 1024;
  #endregion

  TransformAccessArray transforms;
  MovementJob movementJob;
  JobHandle movementJobHandle;
  RandomMovementJob randomMovementJob;
  JobHandle randomMovementJobHandle;
  [NativeDisableContainerSafetyRestriction]
  NativeArray<Vector3> targetDirections;

  [NativeDisableContainerSafetyRestriction]
  NativeArray<Obstacle2> obstacles;

  NativeArray<Boid2> lastFrameData;
  NativeArray<Boid2> dbl_buffer_lastFrameData;

  FlockingJob jobFlocking;
  JobHandle handleFlocking;

  [NativeDisableContainerSafetyRestriction]
  [ReadOnly]
  NativeArray<Vector3> tgv_flocking;

  int obstaclesCount = 0;

  System.Random mSeed;

  private void OnDisable()
  {
    movementJobHandle.Complete();
    randomMovementJobHandle.Complete();
    handleFlocking.Complete();

    transforms.Dispose();
    targetDirections.Dispose();
    obstacles.Dispose();
    tgv_flocking.Dispose();
    lastFrameData.Dispose();
    dbl_buffer_lastFrameData.Dispose();
  }

  private void Start()
  {
    mSeed = new System.Random();
    transforms = new TransformAccessArray(0, -1);
    targetDirections = new NativeArray<Vector3>(MAX_BOIDS, Allocator.Persistent);
    obstacles = new NativeArray<Obstacle2>(50, Allocator.Persistent);
    tgv_flocking = new NativeArray<Vector3>(MAX_BOIDS, Allocator.Persistent);
    lastFrameData = new NativeArray<Boid2>(MAX_BOIDS, Allocator.Persistent);
    dbl_buffer_lastFrameData = new NativeArray<Boid2>(MAX_BOIDS, Allocator.Persistent);

    AddBoids(BoidCount);

    StartCoroutine(Coroutine_Random());
    StartCoroutine(Coroutine_Alignment());
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
      lastFrameData[BoidCount + i] = new Boid2()
      {
        pos = new Vector3(x, y, 0.0f),
        direction = Vector3.zero,
        speed = MaxSpeed
      };
    }
    BoidCount = transforms.length;
    UiJob.SetBoidCount(BoidCount);
  }

  void AddObstacle()
  {
    movementJobHandle.Complete();
    randomMovementJobHandle.Complete();

    Vector2 pt = new Vector2(
        Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
        Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

    GameObject obj = Instantiate(PrefabObstacles[Random.Range(0, PrefabObstacles.Length)]);
    CircleCollider2D coll = obj.GetComponent<CircleCollider2D>();
    float radius = 10.0f;
    if(coll)
    {
      radius = coll.radius;
    }
    obj.name = "Obstacle_" + obstaclesCount;
    obj.transform.position = new Vector3(pt.x, pt.y, 0.0f);
    obstacles[obstaclesCount] = new Obstacle2()
    {
      position = new Vector3(pt.x, pt.y, 0.0f),
      avoidanceRadius = radius * avoidanceRadiusMultiplier
    };
    obstaclesCount++;
  }

  void HandleInputs()
  {
    if (EventSystem.current.IsPointerOverGameObject() ||
       enabled == false)
    {
      return;
    }

    if (Input.GetKeyDown(KeyCode.Space))
    {
      AddBoids(BoidIncrementCount);
    }

    if (Input.GetMouseButtonDown(2))
    {
      AddObstacle();
    }
  }

  private void Update()
  {
    movementJobHandle.Complete();
    randomMovementJobHandle.Complete();

    dbl_buffer_lastFrameData.CopyFrom(lastFrameData);

    HandleInputs();

    movementJob = new MovementJob()
    {
      MaxSpeed  = MaxSpeed,
      speed = MaxSpeed,
      deltaTime = Time.deltaTime,
      targetDirection = targetDirections,
      targetDirection_Flocking = tgv_flocking,
      rotationSpeed = 100.0f,
      MinX = MinX,
      MaxX = MaxX,
      MinY = MinY,
      MaxY = MaxY,
      bounceWall = true,
      obstacles = obstacles,
      obstaclesCount = obstaclesCount,
      weightAvoidObstacles = weightAvoidObstacles,
      lastFrameData = lastFrameData
    };
    movementJobHandle = movementJob.Schedule(transforms);
    JobHandle.ScheduleBatchedJobs();
  }

  IEnumerator Coroutine_Alignment()
  {
    while (true)
    {
      if (useAlignmentRule)
      {
        if (handleFlocking.IsCompleted)
        {
          jobFlocking = new FlockingJob()
          {
            useAlignmentRule = useAlignmentRule,
            useCohesionRule = useCohesionRule,
            useSeparationRule = useSeparationRule,
            visibility = visibility,
            weightSeparation = weightSeparation,
            separationDistance = separationDistance,
            boidsCount = transforms.length,
            weightAlignment = weightAlignment,
            lastFrameData = dbl_buffer_lastFrameData,
            targetVelocities = tgv_flocking,
            weightCohesion = weightCohesion
          };
          handleFlocking = jobFlocking.Schedule(transforms.length, NumBatches);
        }
      }
      yield return new WaitForSeconds(TickDuration);
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
