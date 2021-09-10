using UnityEngine;

public struct Pvector
{
  public const float PI = 3.141592635f;
  public float x;
  public float y;

  public Pvector(float a, float b)
  {
    x = a;
    y = b;
  }

  // Sets values of x and y for Pvector
  public void set(float i, float o)
  {
    x = i;
    y = o;
  }

  public void setMag(float m)
  {
    normalize();
    x *= m;
    y *= m;
  }

  public void add(Pvector v)
  {
    x += v.x;
    y += v.y;
  }

  // Adds to a Pvector by a constant number
  public void add(float s)
  {
    x += s;
    y += s;
  }

  // Subtracts 2 vectors
  public void sub(Pvector v)
  {
    x -= v.x;
    y -= v.y;
  }

  // Subtracts two vectors and returns the difference as a vector
  public static Pvector sub(Pvector v, Pvector v2)
  {
    return new Pvector(v.x - v2.x, v.y - v2.y);
  }

  // Adds to a Pvector by a constant number
  public void sub(float s)
  {
    x -= s;
    y -= s;
  }

  // Multiplies 2 vectors
  public void mult(Pvector v)
  {
    x *= v.x;
    y *= v.y;
  }

  // Adds to a Pvector by a constant number
  public void mult(float s)
  {
    x *= s;
    y *= s;
  }

  // Divides 2 vectors
  public void div(Pvector v)
  {
    x /= v.x;
    y /= v.y;
  }

  // Adds to a Pvector by a constant number
  public void div(float s)
  {
    x /= s;
    y /= s;
  }

  public void limit(float max)
  {
    float size = magnitude();

    if (size > max)
    {
      set(x / size, y / size);
    }
  }

  // Calculates the distance between the first Pvector and second Pvector
  public float dist(Pvector v)
  {
    float dx = x - v.x;
    float dy = y - v.y;
    float dist = Mathf.Sqrt(dx * dx + dy * dy);
    return dist;
  }

  // Calculates the distance between the first Pvector and second Pvector
  static public float dist(Pvector v1, Pvector v2)
  {
    float dx = v1.x - v2.x;
    float dy = v1.y - v2.y;
    float dist = Mathf.Sqrt(dx * dx + dy * dy);
    return dist;
  }

  // Calculates the dot product of a vector
  public float dotProduct(Pvector v)
  {
    float dot = x * v.x + y * v.y;
    return dot;
  }

  // Calculates magnitude of referenced object
  public float magnitude()
  {
    return Mathf.Sqrt(x * x + y * y);
  }

  public void setMagnitude(float x)
  {
    normalize();
    mult(x);
  }

  // Calculate the angle between Pvector 1 and Pvector 2
  public float angleBetween(Pvector v)
  {
    if (x == 0 && y == 0) return 0.0f;
    if (v.x == 0 && v.y == 0) return 0.0f;

    float dot = x * v.x + y * v.y;
    float v1mag = Mathf.Sqrt(x * x + y * y);
    float v2mag = Mathf.Sqrt(v.x * v.x + v.y * v.y);
    float amt = dot / (v1mag * v2mag); //Based of definition of dot product
                                       //dot product / product of magnitudes gives amt
    if (amt <= -1)
    {
      return PI;
    }
    else if (amt >= 1)
    {
      return 0;
    }
    float tmp = Mathf.Acos(amt);
    return tmp;
  }

  // normalize divides x and y by magnitude if it has a magnitude.
  public void normalize()
  {
    float m = magnitude();

    if (m > 0)
    {
      set(x / m, y / m);
    }
    else
    {
      set(x, y);
    }
  }

  // Creates and returns a copy of the Pvector used as a parameter
  public Pvector copy()
  {
    return new Pvector(x, y);
  }
}