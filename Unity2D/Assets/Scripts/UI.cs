using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
  public Text textNumBoids;
  public Text textNumEnemies;

  public FlockBehaviour flockBehaviour;

  void Start()
  {
    StartCoroutine(Coroutine_UpdateText());
  }

  IEnumerator Coroutine_UpdateText()
  {
    while(true)
    {
      textNumBoids.text = "Boids: " + flockBehaviour.flocks[0].mAutonomous.Count.ToString();
      textNumEnemies.text = "Predators: " + flockBehaviour.flocks[1].mAutonomous.Count.ToString();
      yield return new WaitForSeconds(0.5f);
    }
  }
}
