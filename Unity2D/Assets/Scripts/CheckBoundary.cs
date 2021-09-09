using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBoundary : MovementRule
{
  private Vector2 bl = new Vector2(0.0f, 0.0f);
  private Vector2 br = new Vector2(20.0f, 0.0f);
  private Vector2 tr = new Vector2(20.0f, 20.0f);
  private Vector2 tl = new Vector2(0.0f, 20.0f);

  public struct Intersection
  {
    public bool hit;
    public Vector2 point;
  }

  public CheckBoundary()
  {
  }

  public void SetBound(BoxCollider2D box)
  {
    Vector3 center = box.bounds.center;
    bl.x = box.bounds.min.x;
    bl.y = box.bounds.min.y;

    br.x = box.bounds.max.x;
    br.y = box.bounds.min.y;

    tr.x = box.bounds.max.x;
    tr.y = box.bounds.max.y;

    tl.x = box.bounds.min.x;
    tl.y = box.bounds.max.y;
  }

  Vector2 Reflection(Vector2 vec, Vector2 normal) 
  {
    Vector2 r = new Vector2();
    float d = Vector2.Dot(vec, normal);// Dot(vec, normal);
    r = vec - normal* (d* 2.0f );
    return r;
  }

  public override void Update(Boid boid)
  {
    displacement = Vector2.zero;

    // we are checking against a rect.
    Vector2 dir = new Vector2(
      Mathf.Cos(boid.angleInDegrees * Mathf.Deg2Rad),
      Mathf.Sin(boid.angleInDegrees * Mathf.Deg2Rad));

    Vector2 p1 = new Vector2(
      boid.position.x,
      boid.position.y);

    //Vector2 p2 = new Vector3(
    //  boid.position.x + Mathf.Cos(boid.angleInDegrees * Mathf.Deg2Rad) * boid.SeparationDistance,
    //  boid.position.y + Mathf.Sin(boid.angleInDegrees * Mathf.Deg2Rad) * boid.SeparationDistance);

    Vector2 p2 = p1 + dir * boid.SeparationDistance;

    boid.debugDrawing.Distance = boid.SeparationDistance;

    Intersection isect1 = LineLine(p1, p2, bl, br);
    if(isect1.hit)
    {
      boid.debugDrawing.Distance = (isect1.point - p1).magnitude;
      Vector2 re = Reflection(dir, new Vector2(0.0f, 1.0f));
      //Debug.Log("Intersection - (BL,BR) " + re.ToString());
      displacement = re;
    }
    Intersection isect2 = LineLine(p1, p2, br, tr);
    if(isect2.hit)
    {
      boid.debugDrawing.Distance = (isect2.point - p1).magnitude;
      Vector2 re = Reflection(dir, new Vector2(-1.0f, 0.0f));
      //Debug.Log("Intersection - (BR,TR) " + re.ToString());
      displacement = re;
    }
    Intersection isect3 = LineLine(p1, p2, tr, tl);
    if (isect3.hit)
    {
      boid.debugDrawing.Distance = (isect3.point - p1).magnitude;
      Vector2 re = Reflection(dir, new Vector2(0.0f, -1.0f));
      //Debug.Log("Intersection - (TR,TL) " + re.ToString());
      displacement = re;
    }
    Intersection isect4 = LineLine(p1, p2, tl, bl);
    if (isect4.hit)
    {
      boid.debugDrawing.Distance = (isect4.point - p1).magnitude;
      Vector2 re = Reflection(dir, new Vector2(1.0f, 0.0f));
      //Debug.Log("Intersection - (TL,BL) " + re.ToString());
      displacement = re;
    }
  }

  Intersection LineLine(
    Vector2 l1p1,
    Vector2 l1p2,
    Vector2 l2p1,
    Vector2 l2p2)
  {
    return LineLine(l1p1.x, l1p1.y, l1p2.x, l1p2.y,
      l2p1.x, l2p1.y, l2p2.x, l2p2.y);
  }

  Intersection LineLine(
    float x1, float y1, 
    float x2, float y2, 
    float x3, float y3, 
    float x4, float y4)
  {
    Intersection isect = new Intersection()
    {
      hit = false,
      point = new Vector2()
    };

    // calculate the distance to intersection point
    float uA = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));
    float uB = ((x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)) / ((y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1));

    // if uA and uB are between 0-1, lines are colliding
    if (uA >= 0 && uA <= 1 && uB >= 0 && uB <= 1)
    {
      float intersectionX = x1 + (uA * (x2 - x1));
      float intersectionY = y1 + (uA * (y2 - y1));

      isect.point = new Vector2(intersectionX, intersectionY);
      isect.hit = true;
    }
    return isect;
  }
}
