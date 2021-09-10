using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockBehaviour1 : MonoBehaviour
{

  public class Avoid
  {
    public Pvector pos;
    public SpriteRenderer obj;

    public Avoid(float xx, float yy)
    {
      pos = new Pvector(xx, yy);
    }

    public void go()
    {

    }

    public void draw()
    {
      //fill(0, 255, 200);
      //ellipse(pos.x, pos.y, 15, 15);
    }
  }

  public class Boid
  {
    // timers
    //int thinkTimer = 0;
    static public bool option_friend = true;
    static public bool option_crowd = true;
    static public bool option_avoid = true;
    static public bool option_noise = true;
    static public bool option_cohese = true;
    //public bool option_crowd = true;
    static public float maxSpeed = 2.0f;
    static public float friendRadius = 10.0f;
    static public float crowdRadius = 10.0f;
    static public float avoidRadius = 5.0f;
    static public float coheseRadius = 20.0f;
    static public float width = 100.0f;
    static public float height = 100.0f;

    // main fields
    public Pvector pos;
    public Pvector move;
    public float shade;
    List<Boid> friends = new List<Boid>();

    static public List<Avoid> avoids = new List<Avoid>();
    static public List<Boid> boids = new List<Boid>();

    public SpriteRenderer obj;

    public Boid(float xx, float yy)
    {
      move = new Pvector(0, 0);
      pos = new Pvector(0, 0);
      pos.x = xx;
      pos.y = yy;
      //thinkTimer = int(random(10));
      shade = Random.Range(1, 255);
    }

    IEnumerator Coroutine_Think()
    {
      while(true)
      {
        getFriends();
        yield return new WaitForSeconds(5.0f);
      }
    }

    public void go()
    {
      wrap();

      flock();
      pos.add(move);
    }

    void flock()
    {
      Pvector allign = getAverageDir();
      Pvector avoidDir = getAvoidDir();
      Pvector avoidObjects = getAvoidAvoids();
      Pvector noise = new Pvector(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
      Pvector cohese = getCohesion();

      allign.mult(1.0f);
      if (!option_friend) allign.mult(0);

      avoidDir.mult(1);
      if (!option_crowd) avoidDir.mult(0);

      avoidObjects.mult(3);
      if (!option_avoid) avoidObjects.mult(0);

      noise.mult(0.1f);
      if (!option_noise) noise.mult(0);

      cohese.mult(1);
      if (!option_cohese) cohese.mult(0);

      stroke(0, 255, 160);

      move.add(allign);
      move.add(avoidDir);
      move.add(avoidObjects);
      move.add(noise);
      move.add(cohese);

      move.mult(0.01f);

      move.limit(maxSpeed);

      shade += getAverageColor() * 0.03f;
      //shade += (random(2) - 1);
      shade = (shade + 255) % 255; //max(0, min(255, shade));
    }

    public void stroke(int r, int g, int b)
    {
      obj.color = new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    void getFriends()
    {
      List<Boid> nearby = new List<Boid>();
      for (int i = 0; i < boids.Count; i++)
      {
        Boid test = boids[i];
        if (test == this) continue;
        if (
          Mathf.Abs(test.pos.x - this.pos.x) < friendRadius &&
          Mathf.Abs(test.pos.y - this.pos.y) < friendRadius)
        {
          nearby.Add(test);
        }
      }
      friends = nearby;
    }

    float getAverageColor()
    {
      float total = 0;
      float count = 0;
      foreach (Boid other in friends)
      {
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

    Pvector getAverageDir()
    {
      Pvector sum = new Pvector(0, 0);
      int count = 0;

      foreach (Boid other in friends)
      {
        float d = Pvector.dist(pos, other.pos);
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < friendRadius))
        {
          Pvector copy = other.move.copy();
          copy.normalize();
          copy.div(d);
          sum.add(copy);
          count++;
        }
        if (count > 0)
        {
          //sum.div((float)count);
        }
      }
      return sum;
    }

    Pvector getAvoidDir()
    {
      Pvector steer = new Pvector(0, 0);
      int count = 0;

      foreach (Boid other in friends)
      {
        float d = Pvector.dist(pos, other.pos);
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < crowdRadius))
        {
          // Calculate vector pointing away from neighbor
          Pvector diff = Pvector.sub(pos, other.pos);
          diff.normalize();
          diff.div(d);        // Weight by distance
          steer.add(diff);
          count++;            // Keep track of how many
        }
      }
      if (count > 0)
      {
        //steer.div((float) count);
      }
      return steer;
    }

    Pvector getAvoidAvoids()
    {
      Pvector steer = new Pvector(0, 0);
      int count = 0;

      foreach (Avoid other in avoids)
      {
        float d = Pvector.dist(pos, other.pos);
        // If the distance is greater than 0 and less than an arbitrary amount (0 when you are yourself)
        if ((d > 0) && (d < avoidRadius))
        {
          // Calculate vector pointing away from neighbor
          Pvector diff = Pvector.sub(pos, other.pos);
          diff.normalize();
          diff.div(d);        // Weight by distance
          steer.add(diff);
          count++;            // Keep track of how many
        }
      }
      return steer;
    }

    Pvector getCohesion()
    {
      //float neighbordist = 50.0f;
      Pvector sum = new Pvector(0, 0);   // Start with empty vector to accumulate all locations
      int count = 0;
      foreach (Boid other in friends)
      {
        float d = Pvector.dist(pos, other.pos);
        if ((d > 0) && (d < coheseRadius))
        {
          sum.add(other.pos); // Add location
          count++;
        }
      }
      if (count > 0)
      {
        sum.div(count);

        Pvector desired = Pvector.sub(sum, pos);
        desired.setMag(0.05f);
        return desired;
      }
      else
      {
        return new Pvector(0, 0);
      }
    }

    public void draw()
    {
      obj.transform.position = new Vector3(pos.x, pos.y, 0.0f);
      for (int i = 0; i < friends.Count; i++)
      {
        Boid f = friends[i];
        stroke(90, 90, 90);
        //line(this.pos.x, this.pos.y, f.pos.x, f.pos.y);
      }
      //noStroke();
      //fill(shade, 90, 200);
      //pushMatrix();
      //translate(pos.x, pos.y);
      //rotate(move.heading());
      //beginShape();
      //vertex(15 * globalScale, 0);
      //vertex(-7 * globalScale, 7 * globalScale);
      //vertex(-7 * globalScale, -7 * globalScale);
      //endShape(CLOSE);
      //popMatrix();
    }

    //// update all those timers!
    //void increment()
    //{
    //  thinkTimer = (thinkTimer + 1) % 5;
    //}

    void wrap()
    {
      pos.x = (pos.x + width) % width - width/2.0f;
      pos.y = (pos.y + height) % height - height/2.0f;
    }
  }

  public int numBoids = 100;
  public int width = 100;
  public int height = 100;
  public float TickDuration = 0.1f;

  public bool option_friend = true;
  public bool option_crowd = true;
  public bool option_avoid = true;
  public bool option_noise = true;
  public bool option_cohese = true;
  public float maxSpeed = 2.0f;
  public float friendRadius = 10.0f;
  public float crowdRadius = 10.0f;
  public float avoidRadius = 5.0f;
  public float coheseRadius = 20.0f;

  public SpriteRenderer BoidPrefab;
  public SpriteRenderer AvoidPrefab;

  private void Start()
  {
    for(int i = 0; i < numBoids; ++i)
    {
      Boid boid = new Boid(
        Random.Range(-width / 2.0f, width / 2.0f),
        Random.Range(-height / 2.0f, height / 2.0f));

      SpriteRenderer obj = Instantiate(BoidPrefab);
      obj.transform.position = new Vector3(boid.pos.x, boid.pos.y, 0.0f);
      boid.obj = obj;

      Boid.boids.Add(boid);
    }

    StartCoroutine(Coroutine_Update());
  }

  IEnumerator Coroutine_Update()
  {
    while(true)
    {
      for (int i = 0; i < Boid.boids.Count; i++)
      {
        Boid current = Boid.boids[i];
        current.go();
        current.draw();
      }

      for (int i = 0; i < Boid.avoids.Count; i++)
      {
        Avoid current = Boid.avoids[i];
        current.go();
        current.draw();
      }
      yield return new WaitForSeconds(TickDuration);
    }
  }

  private void Update()
  {
    SetParams();
    HandleInputs();
  }

  void SetParams()
  {
    Boid.option_friend = option_friend;
    Boid.option_crowd = option_crowd;
    Boid.option_avoid = option_avoid;
    Boid.option_noise = option_noise;
    Boid.option_cohese = option_cohese;
    Boid.maxSpeed = maxSpeed;
    Boid.friendRadius = friendRadius;
    Boid.crowdRadius = crowdRadius;
    Boid.avoidRadius = avoidRadius;
    Boid.coheseRadius = coheseRadius;
  }

  void HandleInputs()
  {
    if(Input.GetMouseButtonDown(1))
    {
      Vector2 rayPos = new Vector2(
          Camera.main.ScreenToWorldPoint(Input.mousePosition).x,
          Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
      Avoid avoid = new Avoid(rayPos.x, rayPos.y);
      Boid.avoids.Add(avoid);

      SpriteRenderer av = Instantiate(AvoidPrefab);
      av.transform.position = new Vector3(rayPos.x, rayPos.y, 0.0f);

      avoid.obj = av;
    }
  }
}
