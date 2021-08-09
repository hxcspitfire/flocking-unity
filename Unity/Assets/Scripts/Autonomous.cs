using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autonomous : MonoBehaviour
{
  Vector3 TargetPos;
  float mWaitTillNextTargetGen = 10.0f;
  [SerializeField]
  Bounds Bound = new Bounds(Vector3.zero, Vector3.one * 5.0f);

  [SerializeField]
  GameObject TargetPos_Viz;

  [SerializeField]
  float RotationSpeed = 20.0f;

  [SerializeField]
  float MaxSpeed = 10.0f;
  //float mCurrentSpeed = 0.0f;

  // Start is called before the first frame update
  void Start()
  {
    //StartCoroutine(Coroutine_GenerateRandomTarget());
    StartCoroutine(Coroutine_GenerateTargetInFront());
    StartCoroutine(Coroutine_CheckNearBorder());
  }

  // Update is called once per frame
  void Update()
  {
    Vector3 targetDirection = (TargetPos - transform.position).normalized;
    Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

    if (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
    {
      transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
    }

    Vector3 pos = transform.forward * MaxSpeed *Time.deltaTime;
    transform.position += pos;
  }

  private void FixedUpdate()
  {
  }

  Vector3 GetRandom(Vector3 min, Vector3 max)
  {
    return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
  }

  // create a coroutine to set a random target position after 
  // every few seconds.
  IEnumerator Coroutine_GenerateRandomTarget()
  {
    while(true)
    {
      TargetPos = GetRandom(Vector3.zero, new Vector3(50.0f, 50.0f, 50.0f));
      TargetPos_Viz.transform.position = TargetPos;
      Debug.Log(TargetPos.ToString());
      yield return new WaitForSeconds(mWaitTillNextTargetGen);
    }
  }
  
  // create a coroutine to set a random target position after 
  // every few seconds.
  IEnumerator Coroutine_GenerateTargetInFront()
  {
    while (true)
    {
      TargetPos += transform.forward * 10.0f;
      TargetPos_Viz.transform.position = TargetPos;
      Debug.Log(TargetPos.ToString());
      yield return new WaitForSeconds(2);
    }
  }

  bool IsTherObstacleInFront()
  {
    Vector3 targetPos = transform.forward * 10.0f;

    // check collision between camera and the player.
    Vector3 dirTmp = targetPos - transform.position;
    dirTmp = dirTmp.normalized;

    //LayerMask allMask = ~PlayerMask;
    RaycastHit hit;
    if (Physics.Raycast(transform.position, dirTmp, out hit))
    {
      //hit.normal
      return true;
    }
    return false;
  }


  IEnumerator Coroutine_CheckNearBorder()
  {
    while (true)
    {
      if(IsTherObstacleInFront())
      {
        // take action if there is an obstacle.
        // for now we just get a random point within the bound.
        TargetPos = Vector3.zero;
      }
      yield return new WaitForSeconds(1.0f);
    }
  }
}
