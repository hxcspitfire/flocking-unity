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
  Vector3 MinPos = Vector3.zero;
  [SerializeField]
  Vector3 MaxPos = new Vector3(50.0f, 50.0f, 50.0f);

  //[SerializeField]
  //GameObject SteerPos_Viz;

  // Start is called before the first frame update
  void Start()
  {
    for(int i = 0; i < NumBoids; ++i)
    {
      GameObject obj = Instantiate(PrefabBoid);
      obj.name = "Boid_" + i;
      //mAutonomous.Add(obj.AddComponent<Autonomous>());
      obj.transform.position = Autonomous.GetRandom(MinPos, MaxPos);
      mAutonomous.Add(obj.GetComponent<Autonomous>());
    }
  }

  // Update is called once per frame
  void Update()
  {
    Rule_Cohesion();
    Rule_Alignment();
  }

  void Rule_Cohesion()
  {
    for (int i = 0; i < mAutonomous.Count; ++i)
    {
      Vector3 steerPos = Vector3.zero;
      for (int j = 0; j < mAutonomous.Count; ++j)
      {
        if (i != j)
        {
          steerPos += mAutonomous[j].transform.position;
        }
      }
      steerPos = steerPos / (NumBoids - 1);

      mAutonomous[i].TargetPos = steerPos;
    }
  }

  void Rule_Alignment()
  {
    for(int i = 0; i < mAutonomous.Count; ++i)
    {
      Vector3 flockDir = Vector3.zero;
      for (int j = 0; j < mAutonomous.Count; ++j)
      {
        if (i != j)
        {
          flockDir += mAutonomous[j].transform.forward;
        }
      }
      flockDir = flockDir / (NumBoids - 1);
      flockDir.Normalize();

      mAutonomous[i].TargetDirection = flockDir;
    }
  }
}
