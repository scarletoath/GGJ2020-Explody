using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeSlow : MonoBehaviour
{
    [SerializeField] float initialSlowTime;

    [SerializeField] float timeToSlow;

    [SerializeField] float targetTimeScale;

    [SerializeField] float unSlowTime;

    float originalTimeScale;
    float startTime = -1;


    // Start is called before the first frame update
    void Start()
    {
        originalTimeScale = Time.timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    startTime = Time.time;
        //}

        if (startTime > 0)
        {
            // time is being slowed
            float timeSinceStart = Time.time - startTime;
            if (timeSinceStart < initialSlowTime)
            {
                // lerp to target
                Time.timeScale = Mathf.Lerp(originalTimeScale, targetTimeScale, (timeSinceStart) / (initialSlowTime));
            }
            else if (Time.time - startTime < initialSlowTime + timeToSlow)
            {
                Time.timeScale = targetTimeScale;
            }
            else if (Time.time - startTime < (initialSlowTime + timeToSlow + unSlowTime))
            {
                // lerp to target
                Time.timeScale = Mathf.Lerp(targetTimeScale, originalTimeScale, (timeSinceStart) / (initialSlowTime + timeToSlow + unSlowTime));
            }
            else
            {
                Time.timeScale = originalTimeScale;
                startTime = -1;
            }
        }
    }

    public void StartSlowDown()
    {
        startTime = Time.time;
    }
}
