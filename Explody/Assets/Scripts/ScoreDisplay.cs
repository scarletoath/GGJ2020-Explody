using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{

	[SerializeField] private Text ScoreText;

	[Space]

	[SerializeField] private float RampDuration = 2.5f; // seconds
	[SerializeField] private AnimationCurve RampCurve = AnimationCurve.EaseInOut ( 0 , 0 , 1 , 1 );

	private float CurrentScore;
	private float TargetScore;
	private float ElapsedRampDuration;

	public bool IsRamping => CurrentScore < TargetScore;

	private void Update ()
	{
		if ( CurrentScore < TargetScore )
		{
			ElapsedRampDuration += Time.unscaledDeltaTime;
			CurrentScore = RampCurve.Evaluate ( ElapsedRampDuration / RampDuration ) * TargetScore;
			ScoreText.text = CurrentScore.ToString ( "F0" );
		}
	}

	public void ShowScore ( float Score )
	{
		ElapsedRampDuration = 0;
		TargetScore = Score;
		gameObject.SetActive ( true );
	}

	public void Hide ()
	{
		gameObject.SetActive ( false );
	}

}