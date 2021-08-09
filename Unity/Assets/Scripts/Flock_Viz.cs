using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAI.Flocking;

public class Flock_Viz : MonoBehaviour
{
  Flock mFlock = new Flock();
  [SerializeField]
  int NumBoids = 100;

  [SerializeField]
  GameObject BoidPrefab;

  List<GameObject> mBoidsGobjs = new List<GameObject>();

  // Start is called before the first frame update
  void Start()
  {
    mFlock.Initialize(NumBoids);
    for(int i = 0; i < NumBoids; ++i)
    {
      mBoidsGobjs.Add(Instantiate(BoidPrefab));
    }

    FollowRunnerCamera follow = Camera.main.GetComponent<FollowRunnerCamera>();
    if(follow != null)
    {
      follow.mPlayer = mBoidsGobjs[0].transform;
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(KeyCode.Space))
    {
      mFlock.Step(Time.timeAsDouble);
      CopyAttributes();
    }
  }

  private void FixedUpdate()
  {
    mFlock.Step(Time.timeAsDouble);
    CopyAttributes();
  }

  void CopyAttributes()
  {
    for (int i = 0; i < NumBoids; ++i)
    {
      mBoidsGobjs[i].transform.position = mFlock.Boids[i].mPosition;
    }
  }
}
