using System.Linq;
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
	}

	public void ExplodeAtMousePosition ()
	{
		Explode ( Camera.ScreenToWorldPoint ( Input.mousePosition ) );
	}

}