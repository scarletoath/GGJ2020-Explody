using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplodeAtPoint : MonoBehaviour
{

	[SerializeField] private Camera Camera;

	[Space]

	[SerializeField] private ExplosionForce ExplosionForce;

	private void Awake ()
	{
		Debug.Assert ( ExplosionForce != null , "ExplosionForce == null" );
	}

	private void OnValidate ()
	{
		ExplosionForce = ExplosionForce == null ? GetComponent <ExplosionForce> () : ExplosionForce;
	}

	private void Update()
	{
		if(Input.GetKeyDown(KeyCode.E))
		{
			ExplodeAtThisPoint();
		}
	}

	public void Explode ( Vector3 Position , float Radius = 0.1f )
	{
		var Colliders = Physics2D.OverlapCircleAll ( Position , Radius );
		var Explodables = Colliders
								   .Where ( c => c.attachedRigidbody != null )
								   .Select ( c => c.GetComponent <Explodable> () )
								   .Where ( ex => ex != null ).ToArray ();

		foreach ( var Explodable in Explodables )
		{
			Explodable.explode ();
		}
		ExplosionForce.doExplosion ( Position );
		GameObject.FindGameObjectWithTag("TimeSlow").GetComponent<TimeSlow>().StartSlowDown();
		StartCoroutine(SetSnappables());
	}

	public void ExplodeAtMousePosition ()
	{
		Explode ( Camera.ScreenToWorldPoint ( Input.mousePosition ) );
	}

	public void ExplodeAtThisPoint()
	{
		Explode(transform.position);
	}

	IEnumerator SetSnappables()
	{
		yield return new WaitForSeconds(.5f);
		GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Piece");
		foreach (GameObject obj in Pieces)
		{
			obj.GetComponent<SnapToLocation>().StartTracking();
		}
	}

}