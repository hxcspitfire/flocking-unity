using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Autonomous : MonoBehaviour
{
  public Vector3 TargetPos;
  float mWaitTillNextTargetGen = 1.0f;

  [SerializeField]
  GameObject TargetPos_Viz;

  public
  float MaxRotationSpeed = 100.0f;

  public float MaxSpeed = 10.0f;
  //float mCurrentSpeed = 0.0f;

  public float Speed { get; private set; } = 0.0f;
  public float TargetSpeed = 0.0f;
  public Vector3 TargetDirection = Vector3.zero;
  public float RotationSpeed = 0.0f;

  public BoxCollider2D Bound;


  // Start is called before the first frame update
  void Start()
  {
    Speed = 0.0f;
    //SetRandomSpeed();
    SetRandomDirection();
  }

  void SetRandomSpeed()
  {
    float speed = Random.Range(0.0f, MaxSpeed);
  }

  void SetRandomDirection()
  {
    // add random direction.
    float angle = Random.Range(-180.0f, 180.0f);
    Vector3 dir = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0.0f);
    TargetDirection = dir;
  }

  // Update is called once per frame
  public void Update()
  {
    //Vector3 targetDirection = (TargetPos - transform.position).normalized;
    //targetDirection += TargetDirection;
    Vector3 targetDirection = TargetDirection;
    targetDirection.Normalize();

    Vector3 rotatedVectorToTarget = Quaternion.Euler(0, 0, 90) * targetDirection;

    // get the rotation that points the Z axis forward, and the Y axis 90 degrees away from the target
    // (resulting in the X axis facing the target)
    Quaternion targetRotation = Quaternion.LookRotation(forward: Vector3.forward, upwards: rotatedVectorToTarget);
    // changed this from a lerp to a RotateTowards because you were supplying a "speed" not an interpolation value
    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);

    //float speed = TargetSpeed;

    Speed = Speed + ((TargetSpeed - Speed)/10.0f) * Time.deltaTime;

    if (Speed > MaxSpeed)
      Speed = MaxSpeed;

    transform.Translate(Vector3.right * Speed * Time.deltaTime, Space.Self);
    //transform.Translate(targetDirection * Speed * Time.deltaTime, Space.World);
  }

  private void FixedUpdate()
  {
  }

  //public void SetTargetSpeed(float target)
  //{
  //  if (target > MaxSpeed)
  //    target = MaxSpeed;
  //  TargetSpeed = target;
  //  //StartCoroutine(Coroutine_LerpTargetSpeed(Speed, target));
  //}

  private IEnumerator Coroutine_LerpTargetSpeed(
    float start,
    float end,
    float seconds = 2.0f)
  {
    float elapsedTime = 0;
    while (elapsedTime < seconds)
    {
      Speed = Mathf.Lerp(
        start,
        end,
        (elapsedTime / seconds));
      elapsedTime += Time.deltaTime;
      //Debug.Log("Speed: " + Speed.ToString("F2"));
      yield return null;
    }
    Speed = end;
  }

  private IEnumerator Coroutine_LerpTargetSpeedCont(
  float seconds = 2.0f)
  {
    float elapsedTime = 0;
    while (elapsedTime < seconds)
    {
      Speed = Mathf.Lerp(
        Speed,
        TargetSpeed,
        (elapsedTime / seconds));
      elapsedTime += Time.deltaTime;
      //Debug.Log("Speed: " + Speed.ToString("F2"));
      yield return null;
    }
    Speed = TargetSpeed;
  }

  static public Vector3 GetRandom(Vector3 min, Vector3 max)
  {
    return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
  }

  // create a coroutine to set a random target position after 
  // every few seconds.
  IEnumerator Coroutine_GenerateRandomTarget()
  {
    while(true)
    {
      TargetPos = GetRandom(Bound.bounds.min, Bound.bounds.max);
      //TargetPos_Viz.transform.position = TargetPos;
      //Debug.Log(TargetPos.ToString());
      yield return new WaitForSeconds(mWaitTillNextTargetGen);
    }
  }
  
  //// create a coroutine to set a random target position after 
  //// every few seconds.
  //IEnumerator Coroutine_GenerateTargetInFront()
  //{
  //  while (true)
  //  {
  //    TargetPos += transform.right * 10.0f;
  //    //TargetPos_Viz.transform.position = TargetPos;
  //    Debug.Log(TargetPos.ToString());
  //    yield return new WaitForSeconds(2);
  //  }
  //}

  //bool IsTherObstacleInFront()
  //{
  //  Vector3 targetPos = transform.right * 10.0f;

  //  // check collision between camera and the player.
  //  Vector3 dirTmp = targetPos - transform.position;
  //  dirTmp = dirTmp.normalized;

  //  //LayerMask allMask = ~PlayerMask;
  //  RaycastHit hit;
  //  if (Physics.Raycast(transform.position, dirTmp, out hit))
  //  {
  //    //hit.normal
  //    return true;
  //  }
  //  return false;
  //}


  //IEnumerator Coroutine_CheckNearBorder()
  //{
  //  while (true)
  //  {
  //    if(IsTherObstacleInFront())
  //    {
  //      // take action if there is an obstacle.
  //      // for now we just get a random point within the bound.
  //      TargetPos = Vector3.zero;
  //    }
  //    yield return new WaitForSeconds(1.0f);
  //  }
  //}
}
