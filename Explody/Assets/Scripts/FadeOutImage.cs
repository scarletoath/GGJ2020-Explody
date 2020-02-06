using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutImage : MonoBehaviour
{
    [SerializeField] float fadeOutTime;
    float startFadeTime;

    bool isFading;

    Image image;
    Color originalColor;
    Color targetColor;

    // Start is called before the first frame update
    void Start ()
    {
        startFadeTime = -1;
        image = GetComponent<Image>();
        originalColor = image.color;
        isFading = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(startFadeTime != -1)
        {
            float timeSinceStart = Time.time - startFadeTime;
            if (timeSinceStart < fadeOutTime)
            {
                image.color = Color.Lerp(originalColor, targetColor, timeSinceStart / fadeOutTime);
            }
            else
            {
                // done fading
                isFading = false;
                image.color = targetColor;
                startFadeTime = -1;
                // set this to inactive after finished fading
                gameObject.SetActive(false); 
            }
        }
    }

    public void StartFadeOut()
    {
        if (!isFading)
        {
            isFading = true;
            originalColor = image.color;
            targetColor = image.color;
            targetColor.a = 0;
            startFadeTime = Time.time;
        }
    }
}
