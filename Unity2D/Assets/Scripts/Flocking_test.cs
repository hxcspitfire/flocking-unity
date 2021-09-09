using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flocking_test : MonoBehaviour
{
  public class Avoid
  {
    public Vector3 pos = new Vector3();
    public SpriteRenderer obj;

    public Avoid(float xx, float yy)
    {
      pos.x = xx;
      pos.y = yy;

    }

    public void go()
    {

    }

    public void draw()
    {
      obj.transform.position = pos;
    }

  }
  public class Boid2
  {
    static public List<Boid2> boids = new List<Boid2>();
    static public List<Avoid> avoids = new List<Avoid>();

    public static float globalScale = 0.91f;
    public static float eraseRadius = 20.0f;
    static public string tool = "boids";

    // boid control
    static public float maxSpeed;
    static public float friendRadius;
    static public float crowdRadius;
    static public float avoidRadius;
    static public float coheseRadius;

    static public bool option_friend = true;
    static public bool option_crowd = true;
    static public bool option_avoid = true;
    static public bool option_noise = true;
    static public bool option_cohese = true;

    // gui crap
    static public int messageTimer = 0;
    static public string messageText = "";

    static public int width = 20;
    static public int height = 20;

    // main fields
    Vector3 pos = new Vector3();
    Vector3 move = new Vector3();
    float shade;
    List<Boid2> friends = new List<Boid2>();

    int thinkTimer = 0;
    public SpriteRenderer obj;

    public Boid2(float xx, float yy)
    {
      pos.x = xx;
      pos.y = yy;
      thinkTimer = Random.Range(0, 10);
      shade = Random.Range(0, 255.0f);
    }

    void getFriends()
    {
      List<Boid2> nearby = new List<Boid2>();
      for (int i = 0; i < boids.Count; i++)
      {
        Boid2 test = boids[i];
        if (test == this)
          continue;
      
        if (
          Mathf.Abs(test.pos.x - this.pos.x) < friendRadius &&
          Mathf.Abs(test.pos.y - this.pos.y) < friendRadius)
        {
          nearby.Add(test);
        }
      }
      friends = nearby;
    }

    public void go()
    {
      increment();
      wrap();

      if (thinkTimer == 0)
      {
        // update our friend array (lots of square roots)
        getFriends();
      }
      flock();
      pos += move;
    }
    void flock()
    {
      Vector3 allign = getAverageDir();
      Vector3 avoidDir = getAvoidDir();
      Vector3 avoidObjects = getAvoidAvoids();
      Vector3 noise = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
      Vector3 cohese = getCohesion();

      allign = allign * 1;
      if (!option_friend)
        allign = allign * 0;

      avoidDir = avoidDir * 1;
      if (!option_crowd)
        avoidDir = avoidDir * 0;

      avoidObjects = avoidObjects * 3;
      if (!option_avoid)
        avoidObjects = avoidObjects * 0;

      noise = noise * 0.001f;
      if (!option_noise)
        noise = noise * 0;

      cohese = cohese * 0.01f;
      if (!option_cohese)
        cohese = cohese * 0;

      //stroke(0, 255, 160);

      move += allign;
      //move += avoidDir;
      //move += avoidObjects;
      //move += noise;
      move += cohese;

      Limit(move, maxSpeed);

      shade += getAverageColor() * 0.03f;
      shade += Random.Range(0, 2.0f) - 1.0f;
      shade = (shade + 255) % 255; //max(0, min(255, shade));
      obj.color = new Color(shade / 255.0f, 0.0f, 0.0f, 1.0f);
    }

    float getAverageColor()
    {
      float total = 0;
      float count = 0;
      for (int i = 0; i < friends.Count; ++i)
      {
        Boid2 other = friends[i];
        if (other.shade - shade < -128)
        {
          total += other.shade + 255 - shade;
        }
        else if (other.shade - shade > 128)
        {
          total += other.shade - 255 - shade;
        }
        else
        {
          total += other.shade - shade;
        }
        count++;
      }
      if (count == 0) return 0;
      return total / (float)count;
    }

    Vector3 getAverageDir()
    {
      Vector3 sum = new Vector3(0, 0);
      int count = 0;

      for (int i = 0; i < friends.Count; ++i)
      {
        Boid2 other = friends[i];
        float d = (pos - other.pos).magnitude;
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < friendRadius))
        {
          Vector3 copy = new Vector3(other.move.x, other.move.y, other.move.z);
          copy.Normalize();
          copy = copy/d;
          sum = sum + copy;
          count++;
        }
        if (count > 0)
        {
          sum = sum / (float)count;
        }
      }
      return sum;
    }

    Vector3 getAverageSpeed()
    {
      Vector3 sum = new Vector3(0, 0);
      int count = 0;

      for (int i = 0; i < friends.Count; ++i)
      {
        Boid2 other = friends[i];
        float d = (pos - other.pos).magnitude;
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < friendRadius))
        {
          Vector3 copy = new Vector3(other.move.x, other.move.y, other.move.z);
          copy.Normalize();
          copy = copy / d;
          sum = sum + copy;
          count++;
        }
        if (count > 0)
        {
          sum = sum / (float)count;
        }
      }
      return sum;
    }

    Vector3 getAvoidDir()
    {
      Vector3 steer = new Vector3(0, 0);
      int count = 0;

      for (int i = 0; i< friends.Count; ++i)
      {
        Boid2 other = friends[i];
        float d = (pos - other.pos).magnitude;
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < crowdRadius))
        {
          // Calculate vector pointing away from neighbor
          Vector3 diff = pos - other.pos;
          diff.Normalize();
          diff = diff/d;        // Weight by distance
          steer = steer + diff;
          count++;            // Keep track of how many
        }
      }
      if (count > 0)
      {
        //steer.div((float) count);
      }
      return steer;
    }

    Vector3 getAvoidAvoids()
    {
      Vector3 steer = new Vector3(0, 0);
      int count = 0;

      for (int i = 0; i < avoids.Count; ++i)
      {
        Avoid other = avoids[i];
        float d = (pos - other.pos).magnitude;
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < avoidRadius))
        {
          // Calculate vector pointing away from neighbor
          Vector3 diff = pos - other.pos;
          diff.Normalize();
          diff = diff / d;        // Weight by distance
          steer = steer + diff;
          count++;            // Keep track of how many
        }
      }
      return steer;
    }

    Vector3 getCohesion()
    {
      Vector3 sum = new Vector3(0, 0);   // Start with empty vector to accumulate all locations
      int count = 0;
      for (int i = 0; i < friends.Count; ++i)
      {
        Boid2 other = friends[i];
        float d = (pos - other.pos).magnitude;
        if ((d > 0) && (d < coheseRadius))
        {
          sum = sum + other.pos; // Add location
          count++;
        }
      }
      if (count > 0)
      {
        sum = sum / count;

        Vector3 desired = sum - pos;
        desired.Normalize();
        desired *= 0.05f;
        return desired;
      }
      else
      {
        return new Vector3(0, 0);
      }
    }

    public static Vector3 Limit(Vector3 v, float f)
    {
      float size = v.magnitude;

      if (size > f)
      {
        v /= size;
      }
      return v;
    }

    // update all those timers!
    void increment()
    {
      thinkTimer = (thinkTimer + 1) % 5;
    }

    void wrap()
    {
      pos.x = (pos.x + width) % width;
      pos.y = (pos.y + height) % height;
    }

    public void draw()
    {
      obj.transform.position = pos;
    }

  }

  public GameObject BoidPrefab;
  public GameObject AvoidPrefab;

  void Start()
  {
    recalculateConstants();
  }

  void Update()
  {
    if(Input.GetMouseButtonDown(0))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

      Boid2 boid = new Boid2(rayPos.x, rayPos.y);
      GameObject obj = Instantiate(BoidPrefab);
      boid.obj = obj.GetComponent<SpriteRenderer>();
      Boid2.boids.Add(boid);
    }
    //if(Input.GetMouseButtonDown(1))
    //{
    //  Vector2 rayPos = new Vector2(
    //      Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
    //      Camera.main.ScreenToWorldPoint(Input.mousePosition).y);

    //  Avoid avoid = new Avoid(rayPos.x, rayPos.y);
    //  avoid.obj = Instantiate(AvoidPrefab);
    //  Boid2.avoids.Add(avoid);
    //}
  }

  private void FixedUpdate()
  {
    draw();
  }


  void recalculateConstants()
  {
    Boid2.maxSpeed = 2.0f;// 2.1f * Boid2.globalScale;
    Boid2.friendRadius = 10.0f;
    Boid2.crowdRadius = Boid2.friendRadius / 1.3f;
    Boid2.avoidRadius = 20.0f;
    Boid2.coheseRadius = Boid2.friendRadius;
  }
  void setupWalls()
  {
    for (int x = 0; x < Boid2.width; x += 20)
    {
      Boid2.avoids.Add(new Avoid(x, 10));
      Boid2.avoids.Add(new Avoid(x, Boid2.height - 10));
    }
  }

  void setupCircle()
  {
    for (int x = 0; x < 50; x += 1)
    {
      float dir = (x / 50.0f) * 2.0f * Mathf.PI;
      Boid2.avoids.Add(
        new Avoid(
          Boid2.width * 
          0.5f + Mathf.Cos(dir) * 
          Boid2.height * .4f, 
          Boid2.height * 
          0.5f + Mathf.Sin(dir) * 
          Boid2.height * .4f));
    }
  }
  void draw()
  {
    //noStroke();
    //colorMode(HSB);
    //fill(0, 100);
    //rect(0, 0, width, height);


    //if (tool == "erase")
    //{
    //  noFill();
    //  stroke(0, 100, 260);
    //  rect(mouseX - eraseRadius, mouseY - eraseRadius, eraseRadius * 2, eraseRadius * 2);
    //  if (mousePressed)
    //  {
    //    erase();
    //  }
    //}
    //else if (tool == "avoids")
    //{
    //  noStroke();
    //  fill(0, 200, 200);
    //  ellipse(mouseX, mouseY, 15, 15);
    //}
    for (int i = 0; i < Boid2.boids.Count; i++)
    {
      Boid2 current = Boid2.boids[i];
      current.go();
      current.draw();
    }

    for (int i = 0; i < Boid2.avoids.Count; i++)
    {
      Avoid current = Boid2.avoids[i];
      current.go();
      current.draw();
    }
  }

}
