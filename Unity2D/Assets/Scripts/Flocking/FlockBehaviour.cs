using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FlockBehaviour : MonoBehaviour
{
  List<Obstacle> mObstacles = new List<Obstacle>();

  [SerializeField]
  GameObject PrefabBoid;
  [SerializeField]
  GameObject[] PrefabObstacles;

  [SerializeField]
  BoxCollider2D Bounds;

  public List<Flock> flocks = new List<Flock>();
  void Reset()
  {
    flocks = new List<Flock>()
    {
      new Flock()
    };
  }

  public float TickDurationCohesion = 1.0f;
  public float TickDurationSeparation = 1.0f;
  public float TickDurationSeparationEnemy = 1.0f;
  public float TickDurationAlignment = 1.0f;
  public float TickDurationRandom = 1.0f;
  public float TickDurationObstacleSeparation = 1.0f;

  void Start()
  {
    foreach (Flock flock in flocks)
    {
      CreateFlock(flock);
    }

    StartCoroutine(Coroutine_Separation());
    StartCoroutine(Coroutine_Cohesion());
    StartCoroutine(Coroutine_Alignment());
    StartCoroutine(Coroutine_Random());
    StartCoroutine(Coroutine_AvoidObstacles());

    StartCoroutine(Coroutine_SeparationWithEnemies());
  }

  void CreateFlock(Flock flock)
  {
    for(int i = 0; i < flock.numBoids; ++i)
    {
      float x = Random.Range(Bounds.bounds.min.x, Bounds.bounds.max.x);
      float y = Random.Range(Bounds.bounds.min.y, Bounds.bounds.max.y);

      AddBoid(x, y, flock);
    }
  }

  void Update()
  {
    HandleInputs();
    Rule_CrossBorder();
  }

  void HandleInputs()
  {
    if (EventSystem.current.IsPointerOverGameObject() ||
       enabled == false)
    {
      return;
    }

    if (Input.GetMouseButtonDown(1))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
      AddBoid(rayPos.x, rayPos.y, flocks[0]);
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
      AddBoid(rayPos.x, rayPos.y, flocks[1]);
    }
  }

  void AddBoid(float x, float y, Flock flock)
  {
    GameObject obj = Instantiate(PrefabBoid);
    obj.name = "Boid_" + flock.name + "_" + flock.mAutonomous.Count;
    obj.transform.position = new Vector3(x, y, 0.0f);
    Autonomous boid = obj.GetComponent<Autonomous>();
    flock.mAutonomous.Add(boid);
    boid.MaxSpeed = flock.maxSpeed;
    boid.RotationSpeed = flock.maxRotationSpeed;
    boid.SetColor(flock.colour);
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

  IEnumerator Coroutine_Cohesion()
  {
    while (true)
    {
      foreach (Flock flock in flocks)
      {
        if (flock.useCohesionRule)
        {
          List<Autonomous> autonomousList = flock.mAutonomous;
          for (int i = 0; i < autonomousList.Count; ++i)
          {
            int count = 0;
            Vector3 steerPos = Vector3.zero;
            for (int j = 0; j < autonomousList.Count; ++j)
            {
              if (i != j && Distance(autonomousList[i], autonomousList[j]) < flock.visibility)
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
              autonomousList[i].TargetSpeed += speed * flock.weightCohesion;
              autonomousList[i].TargetSpeed /= 2.0f;
            }
          }
        }
        yield return null;
      }
      yield return new WaitForSeconds(TickDurationCohesion);
    }
  }
  IEnumerator Coroutine_Alignment()
  {
    while (true)
    {
      foreach (Flock flock in flocks)
      {
        if (flock.useAlignmentRule)
        {
          List<Autonomous> autonomousList = flock.mAutonomous;
          for (int i = 0; i < autonomousList.Count; ++i)
          {
            Vector3 flockDir = Vector3.zero;
            float speed = 0.0f;
            int count = 0;
            for (int j = 0; j < autonomousList.Count; ++j)
            {
              if (i != j && Distance(autonomousList[i], autonomousList[j]) < flock.visibility)
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

              autonomousList[i].TargetSpeed += speed * flock.weightAlignment;
              autonomousList[i].TargetSpeed /= 2.0f;
              //autonomousList[i].TargetDirection += flockDir * weight;
              //autonomousList[i].TargetDirection /= 2.0f;
              autonomousList[i].TargetDirection += flockDir;
              autonomousList[i].TargetDirection.Normalize();
            }
          }
        }
        yield return null;
      }
      yield return new WaitForSeconds(TickDurationAlignment);
    }
  }
  IEnumerator Coroutine_Separation()
  {
    while (true)
    {
      foreach (Flock flock in flocks)
      {
        if (flock.useSeparationRule)
        {
          List<Autonomous> autonomousList = flock.mAutonomous;
          for (int i = 0; i < autonomousList.Count; ++i)
          {
            for (int j = 0; j < autonomousList.Count; ++j)
            {
              if (i != j)
              {
                float dist = (
                  autonomousList[j].transform.position -
                  autonomousList[i].transform.position).magnitude;
                if (dist < flock.separationDistance)
                {
                  Vector3 targetDirection = (
                    autonomousList[i].transform.position -
                    autonomousList[j].transform.position).normalized;
                  //autonomousList[i].TargetDirection += targetDirection * weight;
                  autonomousList[i].TargetDirection += targetDirection;
                  autonomousList[i].TargetDirection.Normalize();
                  //autonomousList[i].TargetDirection /= 2.0f;
                  autonomousList[i].TargetSpeed += dist * flock.weightSeparation;
                  autonomousList[i].TargetSpeed /= 2.0f;
                }
              }
            }
          }
        }
        yield return null;
      }
      yield return new WaitForSeconds(TickDurationSeparation);
    }
  }

  void SeparationWithEnemies_Internal(
    List<Autonomous> boids, 
    List<Autonomous> enemies, 
    float sepDist, 
    float sepWeight)
  {
    for(int i = 0; i < boids.Count; ++i)
    {
      for (int j = 0; j < enemies.Count; ++j)
      {
        float dist = (
          enemies[j].transform.position -
          boids[i].transform.position).magnitude;
        if (dist < sepDist)
        {
          Vector3 targetDirection = (
            boids[i].transform.position -
            enemies[j].transform.position).normalized;

          boids[i].TargetDirection += targetDirection;
          boids[i].TargetDirection.Normalize();

          boids[i].TargetSpeed += dist * sepWeight;
          boids[i].TargetSpeed /= 2.0f;
        }
      }
    }
  }

  IEnumerator Coroutine_SeparationWithEnemies()
  {
    while (true)
    {
      foreach (Flock flock in flocks)
      {
        if (!flock.useFleeOnSightEnemyRule || flock.isPredator) continue;

        foreach (Flock enemies in flocks)
        {
          if (!enemies.isPredator) continue;

          SeparationWithEnemies_Internal(
            flock.mAutonomous, 
            enemies.mAutonomous, 
            flock.enemySeparationDistance, 
            flock.weightFleeOnSightEnemy);
        }
        yield return null;
      }
      yield return new WaitForSeconds(TickDurationSeparationEnemy);
    }
  }

  IEnumerator Coroutine_AvoidObstacles()
  {
    while (true)
    {
      foreach (Flock flock in flocks)
      {
        if (flock.useAvoidObstaclesRule)
        {
          List<Autonomous> autonomousList = flock.mAutonomous;
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
                autonomousList[i].TargetDirection += targetDirection * flock.weightAvoidObstacles;
                autonomousList[i].TargetDirection.Normalize();

                // dont change speed with obstancles.
                //autonomousList[i].TargetSpeed += dist * weight;
                //autonomousList[i].TargetSpeed /= 2.0f;
              }
            }
          }
        }
        yield return null;
      }
      yield return new WaitForSeconds(TickDurationObstacleSeparation);
    }
  }
  IEnumerator Coroutine_Random()
  {
    while (true)
    {
      foreach (Flock flock in flocks)
      {
        if (flock.useRandomRule)
        {
          List<Autonomous> autonomousList = flock.mAutonomous;
          for (int i = 0; i < autonomousList.Count; ++i)
          {
            float rand = Random.Range(0.0f, 1.0f);
            autonomousList[i].TargetDirection.Normalize();
            float angle = Mathf.Atan2(autonomousList[i].TargetDirection.y, autonomousList[i].TargetDirection.x);

            if (rand > 0.5f)
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

            autonomousList[i].TargetDirection += dir * flock.weightRandom;
            autonomousList[i].TargetDirection.Normalize();
            //Debug.Log(autonomousList[i].TargetDirection);

            float speed = Random.Range(1.0f, autonomousList[i].MaxSpeed);
            autonomousList[i].TargetSpeed += speed * flock.weightSeparation;
            autonomousList[i].TargetSpeed /= 2.0f;
          }
        }
        yield return null;
      }
      yield return new WaitForSeconds(TickDurationRandom);
    }
  }

  void Rule_CrossBorder()
  {
    foreach (Flock flock in flocks)
    {
      List<Autonomous> autonomousList = flock.mAutonomous;
      if (flock.bounceWall)
      {
        for (int i = 0; i < autonomousList.Count; ++i)
        {
          Vector3 pos = autonomousList[i].transform.position;
          if (autonomousList[i].transform.position.x + 5.0f > Bounds.bounds.max.x)
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
}
