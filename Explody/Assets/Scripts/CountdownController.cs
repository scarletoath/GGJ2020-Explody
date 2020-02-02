using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    public int countdownTime;
    public Text countdownDisplay;

    //Start the coroutine for the timer
    private void Start()
    {
        StartCoroutine(CountdownToStart());
    }

    IEnumerator CountdownToStart()
    {
     while(countdownTime > 0) //If it's greater than or equal to 1
     {
      countdownDisplay.text = countdownTime.ToString();

      yield return new WaitForSeconds(1f);

      countdownTime--;
	 }
     
     GameController.Instance.OnComplete(); //Calls the game manager and all text will clear itself up on complete.

     yield return new WaitForSeconds(1f);

     //countdownDisplay.gameObject.setAQctive(false);
    }
}
