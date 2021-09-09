using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
  [SerializeField]
  GameObject BoidPrefab;
  [SerializeField]
  BoxCollider2D Boundary;

  [SerializeField]
  bool UseRandomMovement = true;
  [SerializeField]
  bool UseBoidCollision = true;

  [SerializeField]
  bool UseCheckBoundary = true;
  [SerializeField]
  bool UseAlignVelocity = true;
  [SerializeField]
  bool UseCohesion = true;

  //List<Boid> mBoids = new List<Boid>();

  private void Start()
  {
    
  }

  private void Update()
  {
    HandleInputs();
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

  public void AddBoid(Vector2 pos)
  {
    GameObject obj = Instantiate(
      BoidPrefab, 
      new Vector3(pos.x, pos.y, -1.0f), 
      Quaternion.identity);

    Boid boid = new Boid();
    boid.obj = obj;

    // add debug drawing.
    GameObject deb_obj = new GameObject();
    deb_obj.transform.SetParent(obj.transform);
    VectorDrawing vd = obj.AddComponent<VectorDrawing>();
    boid.debugDrawing = vd;
    vd.SetBoid(boid);

    //TestMovement rule = obj.AddComponent<TestMovement>();
    RandomMovement movement = obj.AddComponent<RandomMovement>();
    movement.Boundary = Boundary;
    movement.boid = boid;


    if (UseCheckBoundary)
    {
      CheckBoundary cb = new CheckBoundary();
      cb.SetBound(Boundary);
      movement.AddRule(cb);
    }
    if (UseBoidCollision)
    {
      BoidCollision bc = new BoidCollision();
      movement.AddRule(bc);
    }
    if (UseAlignVelocity)
    {
      AlignVelocity bc = new AlignVelocity();
      movement.AddRule(bc);
    }
    if (UseCohesion)
    {
      Cohesion bc = new Cohesion();
      movement.AddRule(bc);
    }

    Boid.mBoids.Add(boid);
    //mBoids.Add(boid);
  }

  private void FixedUpdate()
  {
    for (int i = 0; i < Boid.mBoids.Count; ++i)
    {
      Boid.mBoids[i].obj.transform.position +=
        new Vector3(
          Boid.mBoids[i].displacement.x,
          Boid.mBoids[i].displacement.y,
          0.0f);

      float angle = Boid.mBoids[i].angleInDegrees;

      Boid.mBoids[i].obj.transform.rotation =
        Quaternion.Euler(
          0.0f,
          0.0f,
          angle);

      // reset the displacement.
      Boid.mBoids[i].last_displacement = Boid.mBoids[i].displacement;
      Boid.mBoids[i].displacement = Vector2.zero;
    }
  }
}
