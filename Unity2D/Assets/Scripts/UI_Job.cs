using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Job : MonoBehaviour
{
  public Text textNumBoids;
  public Text textNumEnemies;

  void Start()
  {
    //StartCoroutine(Coroutine_UpdateText());
  }

  public void SetBoidCount(int count)
  {
    textNumBoids.text = "Boids: " + count.ToString();
  }

  IEnumerator Coroutine_UpdateText()
  {
    while(true)
    {
      //textNumBoids.text = "Boids: " + flockBehaviour.flocks[0].mAutonomous.Count.ToString();
      //textNumEnemies.text = "Predators: " + flockBehaviour.flocks[1].mAutonomous.Count.ToString();
      yield return new WaitForSeconds(0.5f);
    }
  }
}
