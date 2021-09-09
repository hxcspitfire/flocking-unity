using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorDrawing : MonoBehaviour
{
  List<GameObject> mLines = new List<GameObject>();
  Boid mBoid;

  public float Distance = 0.0f;

  private LineRenderer GetOrCreateLine(int index)
  {
    if (index >= mLines.Count)
    {
      GameObject obj = new GameObject();
      obj.name = "line_" + index.ToString();
      obj.transform.SetParent(transform);
      obj.transform.position = new Vector3(0.0f, 0.0f, -1.0f);
      LineRenderer lr = obj.AddComponent<LineRenderer>();

      mLines.Add(obj);

      lr.material = new Material(Shader.Find("Sprites/Default"));

      lr.startColor = Color.green;
      lr.endColor = Color.green;
    }
    return mLines[index].GetComponent<LineRenderer>();
  }

  public void SetBoid(Boid boid)
  {
    mBoid = boid;
    Distance = mBoid.SeparationDistance;
  }

  private void LateUpdate()
  {
    Boid boid = mBoid;

    Vector3 a = new Vector3(
      boid.position.x,
      boid.position.y,
      -1.0f);

    Vector3 b = new Vector3(
      boid.position.x + Mathf.Cos(boid.angleInDegrees * Mathf.Deg2Rad) * Distance,
      boid.position.y + Mathf.Sin(boid.angleInDegrees * Mathf.Deg2Rad) * Distance,
      -1.0f);

    LineRenderer lr = GetOrCreateLine(0);
    lr.startWidth = 0.1f;
    lr.endWidth = 0.1f;
    lr.positionCount = 2;
    lr.SetPositions(
      new Vector3[]
      {
        a,
        b,
      });
  }
}
