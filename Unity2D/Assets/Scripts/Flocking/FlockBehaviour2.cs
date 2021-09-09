using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviour2 : MonoBehaviour
{
  public GameObject BoidPrefab;
  public float speedLimit = 15.0f;

  public float width = 50.0f;
  public float height = 50.0f;

  public int numBoids = 100;
  public int visualRange = 30;
  public float minDistance = 5;
  public float avoidFactor = 0.05f;
  public float matchingFactor = 0.005f;
  public float centeringFactor = 0.005f; // adjust velocity by this %

  public bool useCohesion = true;
  public bool useAlignment = true;
  public bool useSeparation = true;
  public bool BounceWalls = true;

  public class Boid
  {
    public float x = 0.0f;
    public float y = 0.0f;
    public float dx = 0.0f;
    public float dy = 0.0f;

    public Boid()
    {
    }

    public void Randomize()
    {
      x = Random.Range(0.0f, 1.0f);
      y = Random.Range(0.0f, 1.0f);
      dx = Random.Range(0.0f, 1.0f);
      dy = Random.Range(0.0f, 1.0f);
    }
  }

  List<Boid> boids = new List<Boid>();
  List<GameObject> boidObjects = new List<GameObject>();

  private void Start()
  {
    for(int i = 0; i < numBoids; ++i)
    {
      Boid boid = new Boid();
      boid.Randomize();
      boid.x *= width;
      boid.y *= height;
      boid.dx *= speedLimit;
      boid.dy *= speedLimit;
      boids.Add(boid);

      GameObject obj = Instantiate(BoidPrefab);
      boidObjects.Add(obj);
    }
  }

  private void Update()
  {
    HandleInputs();
  }
  void HandleInputs()
  {
    if (Input.GetMouseButtonDown(1))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
      AddBoid(rayPos);
    }
  }

  void AddBoid(Vector2 pt)
  {
    GameObject obj = Instantiate(BoidPrefab);
    obj.name = "Boid_" + boids.Count;
    obj.transform.position = new Vector3(pt.x, pt.y, 0.0f);
    boidObjects.Add(obj);

    Boid boid = new Boid();

    boid.x = pt.x;
    boid.y = pt.y;
    boid.dx = Random.Range(0.0f, 1.0f) * speedLimit;
    boid.dy = Random.Range(0.0f, 1.0f) * speedLimit;
    boids.Add(boid);
  }

  private void FixedUpdate()
  {
    // Update each boid
    for (int i = 0; i < boids.Count; ++i)
    {
      Boid boid = boids[i];

      // Update the velocities according to each rule
      if (useCohesion)
      {
        FlyTowardsCenter(boid);
      }
      if (useSeparation)
      {
        AvoidOthers(boid);
      }
      if (useAlignment)
      {
        MatchVelocity(boid);
      }
      LimitSpeed(boid);
      KeepWithinBounds(boid);

      // Update the position based on the current velocity
      boid.x += boid.dx;
      boid.y += boid.dy;

      boidObjects[i].transform.position = new Vector3(boid.x, boid.y, 0.0f);
      float angle = Mathf.Atan2(boid.dy, boid.dx);

      //boidObjects[i].transform.rotation = Quaternion.Euler(0.0f, 0.0f, angle * Mathf.Rad2Deg);
    }
  }

  float Distance(Boid boid1, Boid boid2)
  {
    return Mathf.Sqrt(
      (boid1.x - boid2.x) * (boid1.x - boid2.x) +
      (boid1.y - boid2.y) * (boid1.y - boid2.y)  
    );
  }

  public Boid ClosestBoids(Boid boid)
  {
    int index = 0;
    float dist = float.MaxValue;
    for(int i = 0; i < boids.Count; ++i)
    {
      float d = Distance(boids[i], boid);
      if(d > 0.01f && d < dist)
      {
        index = i;
      }
    }
    return boids[index];
  }

  // Constrain a boid to within the window. If it gets too close to an edge,
  // nudge it back in and reverse its direction.
  void KeepWithinBounds(Boid boid)
  {
    if (BounceWalls)
    {
      float turnFactor = 1;

      if (boid.x < -width / 2.0f)
      {
        boid.dx += turnFactor;
      }
      if (boid.x > width / 2.0f)
      {
        boid.dx -= turnFactor;
      }
      if (boid.y < -height / 2.0f)
      {
        boid.dy += turnFactor;
      }
      if (boid.y > height / 2.0f)
      {
        boid.dy -= turnFactor;
      }
    }
    else
    {

      if (boid.x < -width / 2.0f)
      {
        boid.x += width;
      }
      if (boid.x > width / 2.0f)
      {
        boid.dx -= width;
      }
      if (boid.y < -height / 2.0f)
      {
        boid.dy += height;
      }
      if (boid.y > height / 2.0f)
      {
        boid.dy -= height;
      }

    }
  }

  // Find the center of mass of the other boids and adjust velocity slightly to
  // point towards the center of mass.
  void FlyTowardsCenter(Boid boid)
  {

    float centerX = 0;
    float centerY = 0;
    int numNeighbors = 0;

    for (int i = 0; i < boids.Count; ++i)
    {
      Boid otherBoid = boids[i];
      if (Distance(boid, otherBoid) < visualRange)
      {
        centerX += otherBoid.x;
        centerY += otherBoid.y;
        numNeighbors += 1;
      }
    }

    if (numNeighbors > 0)
    {
      centerX = centerX / numNeighbors;
      centerY = centerY / numNeighbors;

      boid.dx += (centerX - boid.x) * centeringFactor;
      boid.dy += (centerY - boid.y) * centeringFactor;
    }
  }

  // Move away from other boids that are too close to avoid colliding
  void AvoidOthers(Boid boid)
  {
    float moveX = 0;
    float moveY = 0;

    for (int i = 0; i < boids.Count; ++i)
    {
      Boid otherBoid = boids[i];
      if (otherBoid != boid)
      {
        if (Distance(boid, otherBoid) < minDistance)
        {
          moveX += boid.x - otherBoid.x;
          moveY += boid.y - otherBoid.y;
        }
      }
    }

    boid.dx += moveX * avoidFactor;
    boid.dy += moveY * avoidFactor;
  }

  // Find the average velocity (speed and direction) of the other boids and
  // adjust velocity slightly to match.
  void MatchVelocity(Boid boid)
  {

    float avgDX = 0;
    float avgDY = 0;
    int numNeighbors = 0;

    for (int i = 0; i < boids.Count; ++i)
    {
      Boid otherBoid = boids[i];
      if (Distance(boid, otherBoid) < visualRange)
      {
        avgDX += otherBoid.dx;
        avgDY += otherBoid.dy;
        numNeighbors += 1;
      }
    }

    if (numNeighbors > 0)
    {
      avgDX = avgDX / numNeighbors;
      avgDY = avgDY / numNeighbors;

      boid.dx += (avgDX - boid.dx) * matchingFactor;
      boid.dy += (avgDY - boid.dy) * matchingFactor;
    }
  }

  void LimitSpeed(Boid boid)
  {
    float speed = Mathf.Sqrt(boid.dx * boid.dx + boid.dy * boid.dy);
    if (speed > speedLimit)
    {
      boid.dx = (boid.dx / speed) * speedLimit;
      boid.dy = (boid.dy / speed) * speedLimit;
    }
  }
}
