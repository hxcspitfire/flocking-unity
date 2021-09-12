using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviour : MonoBehaviour
{
  List<Autonomous> mAutonomous = new List<Autonomous>();
  List<Autonomous> mEnemies = new List<Autonomous>();
  List<Obstacle> mObstacles = new List<Obstacle>();

  [SerializeField]
  int NumBoids = 10;
  [SerializeField]
  int NumEnemyBoids = 10;

  [SerializeField]
  GameObject PrefabBoid;
  [SerializeField]
  GameObject PrefabEnemyBoid;
  [SerializeField]
  GameObject[] PrefabObstacles;

  [SerializeField]
  BoxCollider2D Bounds;

  public bool UseRandom = true;
  public float RandomWeight = 1.0f;
  public bool UseAlignment = true;
  public float AlignmentWeight = 1.0f;
  public bool UseCohesion = true;
  public float CohesionWeight = 1.0f;
  public bool UseSeparation = true;
  public bool UseSeparationEnemies = true;
  public bool UseAvoidObstacles = true;
  public float SeparationWeight = 1.0f;
  public float SeparationDistance = 5.0f;
  public float SeparationWeightEnemy = 1.0f;
  public float SeparationDistanceEnemy = 5.0f;
  public float ObstacleSeparationDistance = 10.0f;
  public float ObstacleSeparationWeight = 1.0f;
  public float Visibility = 20.0f;
  public float TickDurationCohesion = 1.0f;
  public float TickDurationSeparation = 1.0f;
  public float TickDurationSeparationEnemy = 1.0f;
  public float TickDurationAlignment = 1.0f;
  public float TickDurationRandom = 1.0f;
  public float TickDurationObstacleSeparation = 1.0f;
  public bool BounceWall = true;

  [SerializeField]
  float MaxRotationSpeed = 100.0f;

  [SerializeField]
  float MaxSpeed = 10.0f;
  [SerializeField]
  float MaxRotationSpeedEnemy = 250.0f;

  [SerializeField]
  float MaxSpeedEnemy = 20.0f;

  // Start is called before the first frame update
  void Start()
  {
    for (int i = 0; i < NumBoids; ++i)
    {
      GameObject obj = Instantiate(PrefabBoid);
      obj.name = "Boid_" + i;
      obj.transform.position = Autonomous.GetRandom(Bounds.bounds.min, Bounds.bounds.max);
      Autonomous veh = obj.GetComponent<Autonomous>();
      veh.Bound = Bounds;
      mAutonomous.Add(veh);
      veh.MaxSpeed = MaxSpeed;
      veh.RotationSpeed = MaxRotationSpeed;
    }

    for(int i = 0; i < NumEnemyBoids; ++i)
    {
      Vector3 pos = Autonomous.GetRandom(Bounds.bounds.min, Bounds.bounds.max);
      AddEnemy(new Vector2(pos.x, pos.y));
    }

    StartCoroutine(Coroutine_Separation(mAutonomous, SeparationWeight));
    StartCoroutine(Coroutine_Cohesion(mAutonomous, CohesionWeight));
    StartCoroutine(Coroutine_Alignment(mAutonomous, AlignmentWeight));
    StartCoroutine(Coroutine_Random(mAutonomous, RandomWeight));
    StartCoroutine(Coroutine_AvoidObstacles(mAutonomous, ObstacleSeparationWeight));
    StartCoroutine(Coroutine_Separation(mEnemies, SeparationWeight));
    StartCoroutine(Coroutine_Cohesion(mEnemies, CohesionWeight));
    StartCoroutine(Coroutine_Alignment(mEnemies, AlignmentWeight));
    StartCoroutine(Coroutine_Random(mEnemies, RandomWeight));
    StartCoroutine(Coroutine_AvoidObstacles(mEnemies, ObstacleSeparationWeight));
    StartCoroutine(Coroutine_SeparationWithEnemies());
  }

  // Update is called once per frame
  void Update()
  {
    for(int i = 0; i < mAutonomous.Count; ++i)
    {
      mAutonomous[i].RotationSpeed = MaxRotationSpeed;
      mAutonomous[i].MaxSpeed = MaxSpeed;
    }
    HandleInputs();
    Rule_CrossBorder(mAutonomous);
    Rule_CrossBorder(mEnemies);
  }

  void HandleInputs()
  {
    if(Input.GetMouseButtonDown(1))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
      AddBoid(rayPos);
    }
    if (Input.GetMouseButtonDown(0))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
      AddObstacle(rayPos);
    }
    if (Input.GetMouseButtonDown(2))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
      AddEnemy(rayPos);
    }
  }

  void AddBoid(Vector2 pt)
  {
    GameObject obj = Instantiate(PrefabBoid);
    obj.name = "Boid_" + mAutonomous.Count;
    obj.transform.position = new Vector3(pt.x, pt.y, 0.0f);
    Autonomous veh = obj.GetComponent<Autonomous>();
    veh.Bound = Bounds;
    mAutonomous.Add(veh);
    veh.MaxSpeed = MaxSpeed;
  }

  void AddEnemy(Vector2 pt)
  {
    GameObject obj = Instantiate(PrefabEnemyBoid);
    obj.name = "Enemy_" + mEnemies.Count;
    obj.transform.position = new Vector3(pt.x, pt.y, 0.0f);
    Autonomous veh = obj.GetComponent<Autonomous>();
    veh.Bound = Bounds;
    mEnemies.Add(veh);
    veh.MaxSpeed = MaxSpeedEnemy;
    veh.RotationSpeed = MaxRotationSpeedEnemy;
  }

  void AddObstacle(Vector2 pt)
  {
    GameObject obj = Instantiate(PrefabObstacles[Random.Range(0, PrefabObstacles.Length)]);
    obj.name = "Obstacle_" + mObstacles.Count;
    obj.transform.position = new Vector3(pt.x, pt.y, 0.0f);
    Obstacle veh = obj.GetComponent<Obstacle>();
    mObstacles.Add(veh);
  }

  static float Distance(Autonomous a1, Autonomous a2)
  {
    return (a1.transform.position - a2.transform.position).magnitude;
  }

  IEnumerator Coroutine_Cohesion(List<Autonomous> autonomousList, float weight)
  {
    while (true)
    {
      if (UseCohesion)
      {
        for (int i = 0; i < autonomousList.Count; ++i)
        {
          int count = 0;
          Vector3 steerPos = Vector3.zero;
          for (int j = 0; j < autonomousList.Count; ++j)
          {
            if (i != j && Distance(autonomousList[i], autonomousList[j]) < Visibility)
            {
              steerPos += autonomousList[j].transform.position;
              count++;
            }
          }
          if (count > 0)
          {
            steerPos = steerPos / count;

            Vector3 targetDirection = (steerPos - autonomousList[i].transform.position).normalized;
            //autonomousList[i].TargetDirection += targetDirection * weight;
            //autonomousList[i].TargetDirection /= 2.0f;
            autonomousList[i].TargetDirection += targetDirection;
            autonomousList[i].TargetDirection.Normalize();

            float speed = (steerPos - autonomousList[i].transform.position).magnitude / 2.0f;
            autonomousList[i].TargetSpeed += speed * weight;
            autonomousList[i].TargetSpeed /= 2.0f;
          }
        }
        yield return new WaitForSeconds(TickDurationCohesion);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_Alignment(List<Autonomous> autonomousList, float weight)
  {
    while (true)
    {
      if (UseAlignment)
      {
        for (int i = 0; i < autonomousList.Count; ++i)
        {
          Vector3 flockDir = Vector3.zero;
          float speed = 0.0f;
          int count = 0;
          for (int j = 0; j < autonomousList.Count; ++j)
          {
            if (i != j && Distance(autonomousList[i], autonomousList[j]) < Visibility)
            {
              speed += autonomousList[j].Speed;
              flockDir += autonomousList[j].transform.right;
              count++;
            }
          }
          if (count > 0)
          {
            speed = speed / count;
            flockDir = flockDir / count;
            flockDir.Normalize();

            autonomousList[i].TargetSpeed += speed * weight;
            autonomousList[i].TargetSpeed /= 2.0f;
            //autonomousList[i].TargetDirection += flockDir * weight;
            //autonomousList[i].TargetDirection /= 2.0f;
            autonomousList[i].TargetDirection += flockDir;
            autonomousList[i].TargetDirection.Normalize();
          }
        }
        yield return new WaitForSeconds(TickDurationAlignment);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_Separation(List<Autonomous> autonomousList, float weight)
  {
    while (true)
    {
      if (UseSeparation)
      {
        for (int i = 0; i < autonomousList.Count; ++i)
        {
          for (int j = 0; j < autonomousList.Count; ++j)
          {
            if (i != j)
            {
              float dist = (
                autonomousList[j].transform.position -
                autonomousList[i].transform.position).magnitude;
              if(dist < SeparationDistance)
              {
                Vector3 targetDirection = (
                  autonomousList[i].transform.position - 
                  autonomousList[j].transform.position).normalized;
                //autonomousList[i].TargetDirection += targetDirection * weight;
                autonomousList[i].TargetDirection += targetDirection;
                autonomousList[i].TargetDirection.Normalize();
                //autonomousList[i].TargetDirection /= 2.0f;
                autonomousList[i].TargetSpeed += dist * weight;
                autonomousList[i].TargetSpeed /= 2.0f;
              }
            }
          }
        }
        yield return new WaitForSeconds(TickDurationSeparation);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_SeparationWithEnemies()
  {
    while (true)
    {
      if (UseSeparationEnemies)
      {
        for (int i = 0; i < mAutonomous.Count; ++i)
        {
          for (int j = 0; j < mEnemies.Count; ++j)
          {
            float dist = (
              mEnemies[j].transform.position -
              mAutonomous[i].transform.position).magnitude;
            if (dist < SeparationDistanceEnemy)
            {
              Vector3 targetDirection = (
                mAutonomous[i].transform.position -
                mEnemies[j].transform.position).normalized;

              //mAutonomous[i].TargetDirection += targetDirection * SeparationWeightEnemy;
              mAutonomous[i].TargetDirection += targetDirection;
              mAutonomous[i].TargetDirection.Normalize();

              //mAutonomous[i].TargetDirection /= 2.0f;
              mAutonomous[i].TargetSpeed += dist * SeparationWeightEnemy;
              mAutonomous[i].TargetSpeed /= 2.0f;
            }
          }
        }
        yield return new WaitForSeconds(TickDurationSeparationEnemy);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_AvoidObstacles(List<Autonomous> autonomousList, float weight)
  {
    while (true)
    {
      if (UseAvoidObstacles)
      {
        for (int i = 0; i < autonomousList.Count; ++i)
        {
          for (int j = 0; j < mObstacles.Count; ++j)
          {
            float dist = (
              mObstacles[j].transform.position -
              autonomousList[i].transform.position).magnitude;
            if (dist < mObstacles[j].AvoidanceRadius)
            {
              Vector3 targetDirection = (
                autonomousList[i].transform.position -
                mObstacles[j].transform.position).normalized;

              //autonomousList[i].TargetDirection.Normalize();
              autonomousList[i].TargetDirection += targetDirection * weight;
              autonomousList[i].TargetDirection.Normalize();

              // dont change speed with obstancles.
              //autonomousList[i].TargetSpeed += dist * weight;
              //autonomousList[i].TargetSpeed /= 2.0f;
            }
          }
        }
        yield return new WaitForSeconds(TickDurationObstacleSeparation);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_Random(List<Autonomous> autonomousList, float weight)
  {
    while (true)
    {
      if (UseRandom)
      {
        for (int i = 0; i < autonomousList.Count; ++i)
        {
          float rand = Random.Range(0.0f, 1.0f);
          autonomousList[i].TargetDirection.Normalize();
          float angle = Mathf.Atan2(autonomousList[i].TargetDirection.y, autonomousList[i].TargetDirection.x);

          if(rand > 0.5f)
          {
            angle += Mathf.Deg2Rad * 45.0f;
          }
          else
          {
            angle -= Mathf.Deg2Rad * 45.0f;
          }
          Vector3 dir = Vector3.zero;
          dir.x = Mathf.Cos(angle);
          dir.y = Mathf.Sin(angle);

          autonomousList[i].TargetDirection += dir * weight;
          autonomousList[i].TargetDirection.Normalize();
          //Debug.Log(autonomousList[i].TargetDirection);

          float speed = Random.Range(1.0f, autonomousList[i].MaxSpeed);
          autonomousList[i].TargetSpeed += speed * weight;
          autonomousList[i].TargetSpeed /= 2.0f;
        }
        yield return new WaitForSeconds(TickDurationRandom);
      }
      yield return null;
    }
  }

  void Rule_CrossBorder(List<Autonomous> autonomousList)
  {
    if (BounceWall)
    {
      for (int i = 0; i < autonomousList.Count; ++i)
      {
        Vector3 pos = autonomousList[i].transform.position;
        if (autonomousList[i].transform.position.x + 5.0f> Bounds.bounds.max.x)
        {
          autonomousList[i].TargetDirection.x = -1.0f;
        }
        if (autonomousList[i].transform.position.x - 5.0f < Bounds.bounds.min.x)
        {
          autonomousList[i].TargetDirection.x = 1.0f;
        }
        if (autonomousList[i].transform.position.y + 5.0f > Bounds.bounds.max.y)
        {
          autonomousList[i].TargetDirection.y = -1.0f;
        }
        if (autonomousList[i].transform.position.y - 5.0f < Bounds.bounds.min.y)
        {
          autonomousList[i].TargetDirection.y = 1.0f;
        }
        autonomousList[i].TargetDirection.Normalize();
      }
    }
    else
    {
      for (int i = 0; i < autonomousList.Count; ++i)
      {
        Vector3 pos = autonomousList[i].transform.position;
        if (autonomousList[i].transform.position.x > Bounds.bounds.max.x)
        {
          pos.x = Bounds.bounds.min.x;
        }
        if (autonomousList[i].transform.position.x < Bounds.bounds.min.x)
        {
          pos.x = Bounds.bounds.max.x;
        }
        if (autonomousList[i].transform.position.y > Bounds.bounds.max.y)
        {
          pos.y = Bounds.bounds.min.y;
        }
        if (autonomousList[i].transform.position.y < Bounds.bounds.min.y)
        {
          pos.y = Bounds.bounds.max.y;
        }
        autonomousList[i].transform.position = pos;
      }
    }
  }
}
