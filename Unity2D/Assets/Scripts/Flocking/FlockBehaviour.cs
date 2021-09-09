using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviour : MonoBehaviour
{
  List<Autonomous> mAutonomous = new List<Autonomous>();
  [SerializeField]
  int NumBoids = 10;

  [SerializeField]
  GameObject PrefabBoid;

  [SerializeField]
  BoxCollider2D Bounds;

  //[SerializeField]
  //GameObject SteerPos_Viz;

  //private List<float> mSpeed = new List<float>();
  //private List<Vector3> mDirection = new List<Vector3>();
  public bool UseRandom = true;
  public float RandomWeight = 1.0f;
  public bool UseAlignment = true;
  public float AlignmentWeight = 1.0f;
  public bool UseCohesion = true;
  public float CohesionWeight = 1.0f;
  //public bool UseAlignSpeed = true;
  public bool UseSeparation = true;
  public float SeparationWeight = 1.0f;
  public float SeparationDistance = 5.0f;
  public float Visibility = 20.0f;

  // Start is called before the first frame update
  void Start()
  {
    for (int i = 0; i < NumBoids; ++i)
    {
      GameObject obj = Instantiate(PrefabBoid);
      obj.name = "Boid_" + i;
      //mAutonomous.Add(obj.AddComponent<Autonomous>());
      obj.transform.position = Autonomous.GetRandom(Bounds.bounds.min, Bounds.bounds.max);
      Autonomous veh = obj.GetComponent<Autonomous>();
      veh.Bound = Bounds;
      mAutonomous.Add(veh);
    }

    StartCoroutine(Coroutine_Separation(0.5f));
    StartCoroutine(Coroutine_Cohesion());
    StartCoroutine(Coroutine_Alignment());
    StartCoroutine(Coroutine_Random());
  }

  // Update is called once per frame
  void Update()
  {
    HandleInputs();
    Rule_CrossBorder();
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
  }

  void AddBoid(Vector2 pt)
  {
    GameObject obj = Instantiate(PrefabBoid);
    obj.name = "Boid_" + mAutonomous.Count;
    obj.transform.position = new Vector3(pt.x, pt.y, 0.0f);
    Autonomous veh = obj.GetComponent<Autonomous>();
    veh.Bound = Bounds;
    mAutonomous.Add(veh);
  }

  static float Distance(Autonomous a1, Autonomous a2)
  {
    return (a1.transform.position - a2.transform.position).magnitude;
  }

  IEnumerator Coroutine_Cohesion(float duration = 1.0f)
  {
    while (true)
    {
      if (UseCohesion)
      {
        for (int i = 0; i < mAutonomous.Count; ++i)
        {
          int count = 0;
          Vector3 steerPos = Vector3.zero;
          for (int j = 0; j < mAutonomous.Count; ++j)
          {
            if (i != j && Distance(mAutonomous[i], mAutonomous[j]) < Visibility)
            {
              steerPos += mAutonomous[j].transform.position;
              count++;
            }
          }
          if (count > 0)
          {
            steerPos = steerPos / count;

            mAutonomous[i].TargetPos = steerPos;
            Vector3 targetDirection = (steerPos - mAutonomous[i].transform.position).normalized;
            mAutonomous[i].TargetDirection += targetDirection * CohesionWeight;
            mAutonomous[i].TargetDirection /= 2.0f;

            float speed = (steerPos - mAutonomous[i].transform.position).magnitude / 2.0f;
            mAutonomous[i].TargetSpeed += speed * CohesionWeight;
          }
        }
        yield return new WaitForSeconds(duration);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_Alignment(float duration = 1.0f)
  {
    while (true)
    {
      if (UseAlignment)
      {
        for (int i = 0; i < mAutonomous.Count; ++i)
        {
          Vector3 flockDir = Vector3.zero;
          float speed = 0.0f;
          int count = 0;
          for (int j = 0; j < mAutonomous.Count; ++j)
          {
            if (i != j && Distance(mAutonomous[i], mAutonomous[j]) < Visibility)
            {
              speed += mAutonomous[j].Speed;
              flockDir += mAutonomous[j].transform.right;
              count++;
            }
          }
          if (count > 0)
          {
            speed = speed / count;
            flockDir = flockDir / count;
            flockDir.Normalize();

            mAutonomous[i].TargetSpeed += speed * AlignmentWeight;
            mAutonomous[i].TargetDirection += flockDir * AlignmentWeight;
            mAutonomous[i].TargetDirection /= 2.0f;
          }
        }
        yield return new WaitForSeconds(duration);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_Separation(float duration = 1.0f)
  {
    while (true)
    {
      if (UseSeparation)
      {
        for (int i = 0; i < mAutonomous.Count; ++i)
        {
          for (int j = 0; j < mAutonomous.Count; ++j)
          {
            if (i != j)
            {
              float dist = (
                mAutonomous[j].transform.position -
                mAutonomous[i].transform.position).magnitude;
              if(dist < SeparationDistance)
              {
                Vector3 targetDirection = (
                  mAutonomous[i].transform.position - 
                  mAutonomous[j].transform.position).normalized;
                mAutonomous[i].TargetDirection += targetDirection * SeparationWeight;
                mAutonomous[i].TargetDirection /= 2.0f;
                mAutonomous[i].TargetSpeed += dist/2.0f * CohesionWeight;

              }
            }
          }
        }
        yield return new WaitForSeconds(duration);
      }
      yield return null;
    }
  }
  IEnumerator Coroutine_Random(float duration = 3.0f)
  {
    while (true)
    {
      if (UseRandom)
      {
        for (int i = 0; i < mAutonomous.Count; ++i)
        {
          float speed = Random.Range(1.0f, mAutonomous[i].MaxSpeed);
          mAutonomous[i].TargetSpeed += speed * RandomWeight;
        }
        yield return new WaitForSeconds(duration);
      }
      yield return null;
    }
  }

  void Rule_CrossBorder()
  {
    for (int i = 0; i < mAutonomous.Count; ++i)
    {
      Vector3 pos = mAutonomous[i].transform.position;
      if (mAutonomous[i].transform.position.x > Bounds.bounds.max.x)
      {
        pos.x = Bounds.bounds.min.x;
      }
      if (mAutonomous[i].transform.position.x < Bounds.bounds.min.x)
      {
        pos.x = Bounds.bounds.max.x;
      }
      if (mAutonomous[i].transform.position.y > Bounds.bounds.max.y)
      {
        pos.y = Bounds.bounds.min.y;
      }
      if (mAutonomous[i].transform.position.y < Bounds.bounds.min.y)
      {
        pos.y = Bounds.bounds.max.y;
      }
      mAutonomous[i].transform.position = pos;
    }
  }
}
