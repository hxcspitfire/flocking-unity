using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMovement : MonoBehaviour
{
  public float Speed = 10.0f;
  public float SpeedRotation = 200.0f;
  public float ChangeRotDuration = 0.5f;
  public float ChangeRotAngleRange = 30.0f;
  public BoxCollider2D Boundary;

  float mAngle = 0.0f;

  public Boid boid;

  Vector2 mDisp = new Vector2();

  List<MovementRule> mRules = new List<MovementRule>();

  void Start()
  {
    StartCoroutine(
      Coroutine_RandomAngle(
        ChangeRotDuration));
    Speed = Random.Range(0.0f, Boid.MaxSpeed);
  }

  public void AddRule(MovementRule rule)
  {
    mRules.Add(rule);
  }

  void HandleInputs()
  {
    if(Input.GetKeyDown(KeyCode.RightArrow))
    {
      float start = mAngle;
      StartCoroutine(
        Coroutine_RotateWithSpeed(
          start,
          start + 45.0f,
          SpeedRotation));
    }
    if (Input.GetKeyDown(KeyCode.LeftAlt))
    {
      float start = mAngle;
      StartCoroutine(
        Coroutine_RotateWithSpeed(
          start,
          start - 45.0f,
          SpeedRotation));
    }
  }

  private void Update()
  {
    HandleInputs();

    mDisp = Vector2.zero;
    Move(Time.deltaTime);

    for (int i = 0; i < mRules.Count; ++i)
    {
      mRules[i].Update(boid);
      mDisp += mRules[i].displacement;
    }

    mDisp /= (float)(mRules.Count + 1);

    boid.displacement = mDisp;
    mAngle = boid.angleInDegrees;
  }

  private void Move(float dt)
  {
    float x = Mathf.Cos(Mathf.Deg2Rad * mAngle);
    float y = Mathf.Sin(Mathf.Deg2Rad * mAngle);

    Vector2 dir = new Vector2(x, y);
    Vector2 disp = dir * Speed * dt;

    mDisp += disp;

    //boid.displacement = disp;
    //boid.angleInDegrees = mAngle;
  }

  IEnumerator Coroutine_RandomAngle(float duration)
  {
    while(true)
    {
      float start = mAngle;
      float toss = Random.Range(0.0f, 1.0f);
      //mAngle += 90.0f;
      if (toss > 0.5f)
        mAngle += ChangeRotAngleRange;
      else
        mAngle -= ChangeRotAngleRange;
      yield return StartCoroutine(
        Coroutine_RotateWithSpeed(
          start, 
          mAngle, 
          SpeedRotation));
      yield return new WaitForSeconds(duration);
    }
  }

  // coroutine to move smoothly
  private IEnumerator Coroutine_RotateOverSeconds(
    float start,
    float end,
    float seconds)
  {
    float elapsedTime = 0;
    while (elapsedTime < seconds)
    {
      mAngle = Mathf.Lerp(
        start, 
        end, 
        (elapsedTime / seconds));
      elapsedTime += Time.deltaTime;

      yield return null;
    }
    mAngle = end;
  }

  IEnumerator Coroutine_RotateWithSpeed(
    float start,
    float end,
    float speed)
  {
    float duration = Mathf.Abs(end - start) / speed;
    yield return StartCoroutine(
      Coroutine_RotateOverSeconds(
        start,
        end,
        duration));
  }

  //// coroutine to move smoothly
  //private IEnumerator Coroutine_MoveOverSeconds(
  //  GameObject objectToMove,
  //  Vector3 end,
  //  float seconds)
  //{
  //  float elapsedTime = 0;
  //  Vector3 startingPos = 
  //    objectToMove.transform.position;
  //  while (elapsedTime < seconds)
  //  {
  //    objectToMove.transform.position =
  //      Vector3.Lerp(
  //        startingPos, 
  //        end, 
  //        (elapsedTime / seconds));
  //    elapsedTime += Time.deltaTime;

  //    yield return new WaitForEndOfFrame();
  //  }
  //  objectToMove.transform.position = end;
  //}

  //IEnumerator Coroutine_MoveToPoint(
  //  Vector2 p, 
  //  float speed)
  //{
  //  Vector3 endP = new Vector3(
  //    p.x, 
  //    p.y, 
  //    transform.position.z);
  //  float duration = 
  //    (transform.position - endP).magnitude / speed;

  //  yield return StartCoroutine(
  //    Coroutine_MoveOverSeconds(
  //      transform.gameObject,
  //      endP,
  //      duration));
  //}
}
