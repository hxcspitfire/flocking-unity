using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameAI
{
  namespace Flocking
  {
    public class Boid
    {
      static public float MinSpeed = 0.1f;
      static public float MaxSpeed = 1.0f;

      public Vector3 mPosition;
      public Vector3 mVelocity;

      public void Step(float dt)
      {
        // check if there is an onstacle infront.

      }
    }

    public class Flock
    {
      List<Boid> mBoids = new List<Boid>();
      public List<Boid> Boids { get { return mBoids; } }

      private Bounds mBounds = new Bounds(new Vector3(0, 0, 0), new Vector3(10, 5, 10));
      private float mMaxSpeed = 0.01f;

      public Flock()
      {
        
      }

      Vector3 GetRandom(Vector3 min, Vector3 max)
      {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
      }

      Quaternion GetRandom()
      {
        float y = Random.Range(0, 360.0f);
        float p = Random.Range(0, 360.0f);
        float r = Random.Range(0, 360.0f);
        return Quaternion.Euler(y, p, r);
      }

      public void Initialize(int numBoids)
      {
        // randomize the positions, rotation and speed
        // of each boid based on some constraints (min and max).
        for(int i = 0; i < numBoids; ++i)
        {
          Boid boid = new Boid();
          boid.mPosition = GetRandom(mBounds.min, mBounds.max);
          boid.mVelocity = GetRandom(Vector3.zero, Vector3.one);
          boid.mVelocity *= Random.Range(0.0f, mMaxSpeed);

          mBoids.Add(boid);
        }
      }

      public void Step(double dt)
      {
        for(int i = 0; i < mBoids.Count; ++i)
        {
          Boid b = mBoids[i];
          Vector3 v1 = Separation(i);
          Vector3 v2 = Alignment(i);
          Vector3 v3 = Cohesion(i);
          b.mVelocity = b.mVelocity + v1 + v2 + v3;
          b.mPosition = b.mPosition + b.mVelocity;

          if(i == 0)
          {
            Debug.Log(b.mVelocity.ToString());
          }
        }
      }

      Vector3 CalculateCenterOfMass(int index)
      {
        Vector3 total = Vector3.zero;
        for(int i = 0; i < mBoids.Count; ++i)
        {
          if(i != index)
            total += mBoids[i].mPosition;
        }
        return total / (mBoids.Count - 1);
      }

      Vector3 Separation(int index)
      {
        Boid b = mBoids[index];
        Vector3 centerOfMass = CalculateCenterOfMass(index);
        return (centerOfMass - b.mPosition) / 10000.0f;
      }

      Vector3 Alignment(int index)
      {
        Vector3 c = Vector3.zero;

        for(int i = 0; i < mBoids.Count; ++i)
        {
          if ((mBoids[i].mPosition - mBoids[index].mPosition).magnitude < 0.5f)
          {
            c = c - (mBoids[i].mPosition - mBoids[index].mPosition);
          }
        }
        return c;
      }

      Vector3 Cohesion(int index)
      {
        Vector3 pvJ = Vector3.zero;

        for (int i = 0; i < mBoids.Count; ++i)
        {
          if(i != index)
          {
            pvJ = pvJ + mBoids[i].mVelocity;
          }
        }

        pvJ = pvJ / (mBoids.Count - 1);

        return (pvJ - mBoids[index].mVelocity) / 8.0f;
      }
    }
  }
}
