using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
  public float MaxSpeed = 10.0f;
  public float MinSpeed = 1.0f;
  public float MultiplicationFactor = 0.01f;
  public Vector2 Velocity
  {
    get;
    private set;
  } = new Vector2(0.0f, 0.0f);

  public Vector2 Acceleration
  {
    get;
    set;
  } = new Vector2(0.0f, 0.0f);

  public float DragConstant = 0.4f;

  void Start()
  {
    SetRandomVelocity();
  }

  void SetRandomVelocity()
  {
    float angle = Random.Range(-180.0f, 180.0f);
    Vector3 dir = new Vector3(1.0f, 0.0f, 0.0f);// Mathf.Cos(Mathf.Deg2Rad * angle), Mathf.Sin(Mathf.Deg2Rad * angle), 0.0f);
    float speed = MaxSpeed;// Random.Range(0.0f, MaxSpeed);
    Velocity = new Vector2(speed * dir.x, speed * dir.y);
  }

  private void FixedUpdate()
  {
    //if (Acceleration.magnitude > 0.0f)
    //{
      Debug.Log("Acceleration: " + Acceleration + "Velocity: " + Velocity);
    //}

    Vector2 normVel = Velocity;
    normVel.Normalize();
    Vector2 drag = (Velocity - normVel * MinSpeed) * DragConstant;
    Velocity = Velocity + 
      Acceleration * Time.fixedDeltaTime -
      drag;

    Vector2 disp = 
      Velocity * 
      Time.fixedDeltaTime + 
      0.5f * Acceleration * Time.fixedDeltaTime * Time.fixedDeltaTime;

    disp *= MultiplicationFactor;

    transform.position += new Vector3(disp.x, disp.y, 0.0f);
    transform.rotation = Quaternion.Euler(0.0f, 0.0f, Mathf.Rad2Deg * Mathf.Atan2(disp.y, disp.x));

    Acceleration = Vector2.zero;
  }

  private void Update()
  {
    HandleInputs();
  }

  void HandleInputs()
  {
    if (Input.GetMouseButtonDown(0))
    {
      Vector2 pos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

      Vector2 accel = pos - new Vector2(transform.position.x, transform.position.y);
      accel *= 10.0f;
      Acceleration = accel; 
    }
  }
}
