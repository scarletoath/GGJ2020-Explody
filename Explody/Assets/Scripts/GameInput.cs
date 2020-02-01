using System;
using UnityEngine;
using UnityEngine.Events;

public class GameInput : MonoBehaviour
{

	[Serializable] private class BoolEvent : UnityEvent <bool> {}
	[Serializable] private class FloatEvent : UnityEvent <float> {}

	[SerializeField] private string GrabAction;
	[SerializeField] private BoolEvent OnGrab;

	[Space]

	[SerializeField] private string RotateAction;
	[SerializeField] private bool RotateContinuously = true;
	[SerializeField] private FloatEvent OnRotate;

	void Update ()
	{
		if ( Input.GetButtonDown ( GrabAction ) )
		{
			OnGrab.Invoke ( true );
			//Debug.Log ( "grab true" );
		}
		else if ( Input.GetButtonUp ( GrabAction ) )
		{
			OnGrab.Invoke ( false );
			//Debug.Log ( "grab false" );
		}

		float RotateDelta = RotateContinuously || ( Input.GetButtonDown ( RotateAction ) || Input.mouseScrollDelta.y != 0 ) ? Input.GetAxis ( RotateAction ) : 0;
		if ( RotateDelta != 0 )
		{
			OnRotate.Invoke ( RotateDelta );
			//Debug.Log ( $"rotate {RotateDelta}" );
		}
	}

}