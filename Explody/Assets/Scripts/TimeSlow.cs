using System;
using UnityEngine;
using UnityEngine.Events;

public class TimeSlow : MonoBehaviour
{
    [SerializeField] float initialSlowTime; // duration - transition to slow

    [SerializeField] float timeToSlow; // duration - maintain slow

    [SerializeField] float targetTimeScale; // timescale when slow

    [SerializeField] float unSlowTime; // duration - transition to original

    float originalTimeScale;
    float startTime = -1;
    bool playedSound;

	[Serializable] private class FloatEvent : UnityEvent <float> {}

	[SerializeField] private FloatEvent OnRealTimeElapsed;

	// Start is called before the first frame update
    void Start()
    {
        playedSound = false;
        originalTimeScale = Time.timeScale;
    }

    // Update is called once per frame
    void Update()
    {
        //if(Input.GetKeyDown(KeyCode.Space))
        //{
        //    startTime = Time.time;
        //}

		// [ initialSlowTime -> timeToSlow -> unSlowTime -> (end) ]
        if (startTime > 0)
        {
            // time is being slowed
			float currentRealTime = Time.realtimeSinceStartup;
            float elapsedRealTime = currentRealTime - startTime;
            if (elapsedRealTime < initialSlowTime) // transition to slow
            {
                Time.timeScale = Mathf.Lerp(originalTimeScale, targetTimeScale, elapsedRealTime / initialSlowTime);
            }
            else if ( elapsedRealTime < initialSlowTime + timeToSlow) // maintain slow
            {
                if(!playedSound)
                {
                    AkSoundEngine.PostEvent("timeSlow", gameObject);
                    playedSound = true;
                }
                Time.timeScale = targetTimeScale;
            }
            else if ( elapsedRealTime < initialSlowTime + timeToSlow + unSlowTime) // transition to original
            {
                Time.timeScale = Mathf.Lerp(targetTimeScale, originalTimeScale, elapsedRealTime / (initialSlowTime + timeToSlow + unSlowTime));
            }
            else // end
            {
                Debug.Log( "TIME'S UP" );
                GameController.Instance.OnComplete();
                StopSlowDown ();
				return;
			}

			OnRealTimeElapsed.Invoke ( elapsedRealTime );
		}
    }

    public void StartSlowDown()
    {
        startTime = Time.realtimeSinceStartup;
    }

	public void StopSlowDown ()
	{
		startTime = -1;
        playedSound = false;
		Time.timeScale = originalTimeScale;
		OnRealTimeElapsed.Invoke ( -1 );
	}
}
