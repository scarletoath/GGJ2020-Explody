using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CountdownController : MonoBehaviour
{
    public int countdownTime;
    public Text countdownDisplay;

	[Space]
	public Image countdownFill;
	public bool invertCountdownFill;

	[Space]
	public Scrollbar countdownSize;
	public RectTransform countdownSizeContainer;
	public RectTransform countdownSizeFillImage;
	public bool invertCountdownManualFill;
	private List <RectTransform> countdownSizeFillImages = new List <RectTransform> ();
	private float countdownSizeFillImageWidth;
	private float countdownSizeFillSpacing;
	
    //Start the coroutine for the timer
    private void Start()
    {
        //StartCoroutine(CountdownToStart());
		if ( countdownSizeFillImage != null )
		{
			countdownSizeFillImageWidth = countdownSizeFillImage.rect.width;
			countdownSizeFillImage.gameObject.SetActive ( false );
		}
		if ( countdownSizeContainer != null )
		{
			countdownSizeFillSpacing = countdownSizeContainer.GetComponent <HorizontalOrVerticalLayoutGroup> ().spacing;
		}
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

	public void SetElapsedRealTime ( float ElapsedRealTime )
	{
		bool IsValid = ElapsedRealTime >= 0;
		var fill = Mathf.Clamp01 ( ElapsedRealTime / countdownTime );

		if ( countdownDisplay != null ) 
			countdownDisplay.text = IsValid ? $"{countdownTime - ElapsedRealTime:F0}" : string.Empty;

		if ( countdownFill != null )
			countdownFill.fillAmount = IsValid ? ( invertCountdownFill ? ( 1 - fill ) : fill ) : 0;

		if ( countdownSize != null )
		{
			if ( countdownSize.gameObject.activeSelf != IsValid )
				countdownSize.gameObject.SetActive ( IsValid );
			float countdownSizeFill = IsValid ? ( invertCountdownManualFill ? ( 1 - fill ) : fill ) : 0;
			countdownSize.value = countdownSizeFill;
			
			if ( countdownSizeContainer != null && countdownSizeFillImage != null )
			{
				float GetXMax ( int Count )
				{
					float xMax = countdownSizeFillImageWidth + Mathf.Clamp ( Count - 1 , 0 , int.MaxValue ) * ( countdownSizeFillImageWidth + countdownSizeFillSpacing );
					xMax += fill < 0.8f ? 50f : Mathf.Lerp ( 50f , 20f , ( fill - 0.8f ) / 0.2f );
					return xMax;
				}

				countdownSizeContainer.anchorMax = new Vector2 ( countdownSizeFill , countdownSizeContainer.anchorMax.y );
				countdownSizeContainer.ForceUpdateRectTransforms ();
				float currentMax = countdownSizeContainer.rect.width;

				float lastMax = countdownSizeFillImages.Count > 0 ? GetXMax ( countdownSizeFillImages.Count ) : 20f;
				while ( lastMax < currentMax )
				{
					var imageInstance = Instantiate ( countdownSizeFillImage , countdownSizeContainer );
					imageInstance.gameObject.SetActive ( true );
					imageInstance.ForceUpdateRectTransforms ();
					var child = imageInstance.GetChild ( 0 ) as RectTransform;
					child.anchoredPosition = new Vector2 ( child.anchoredPosition.x , Random.Range ( -7 , 7 ) );
					countdownSizeFillImages.Add ( imageInstance );
					lastMax = GetXMax ( countdownSizeFillImages.Count );
				}
			}
			if ( !IsValid && countdownSizeFillImages.Count > 0 )
			{
				foreach ( var image in countdownSizeFillImages )
					DestroyImmediate ( image );
				countdownSizeFillImages.Clear ();
			}
		}
	}

}
