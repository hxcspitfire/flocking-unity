using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleInputs : MonoBehaviour
{
  [SerializeField]
  Rigidbody2D Rb;

  // Start is called before the first frame update
  void Start()
  {

  }

  // Update is called once per frame
  void Update()
  {
    if(Input.GetMouseButtonDown(0))
    {
      Vector2 force = new Vector2();
      force.x = Random.Range(-1.0f, 1.0f);
      force.y = Random.Range(-1.0f, 1.0f);
      force.Normalize();
      force *= 2.0f;
      //Rb.AddForce(force, ForceMode2D.Impulse);
      Rb.AddRelativeForce(force, ForceMode2D.Impulse);
    }
  }
}
