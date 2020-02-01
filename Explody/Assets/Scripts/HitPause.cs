using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitPause : MonoBehaviour
{
    [Range(0f, 1.5f)]
    public float duration = 1f;
    
    bool _isFrozen = false;
    float _pendingHitPauseDuration = 0f;

    void Update()
    {
        if (_pendingHitPauseDuration < 0 && !_isFrozen)
        {
            StartCoroutine(DoHitPause());
		}
    }

    public void HitPauses()
    {
        float _pendingHitPauseDuration = duration;
	}

    IEnumerator DoHitPause(){
     _isFrozen = true;
     var original = Time.timeScale;
     Time.timeScale = 0f;

     yield return new WaitForSecondsRealtime(duration);

     Time.timeScale = original;
     _pendingHitPauseDuration = 0;
     _isFrozen = false;
	}
}
