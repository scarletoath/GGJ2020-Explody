using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ExplosionForce : MonoBehaviour {
	public float force = 50;
	public float radius = 5;
	public float upliftModifer = 5;

	[SerializeField] private bool debug;
	private List <(Rigidbody2D r , Vector3 force)> appliedForces = new List <(Rigidbody2D r , Vector3 force)> ();
	
    /// <summary>
    /// create an explosion force
    /// </summary>
    /// <param name="position">location of the explosion</param>
	public void doExplosion(Vector3 position){
		transform.localPosition = position;
		StartCoroutine(waitAndExplode());
	}

    /// <summary>
    /// exerts an explosion force on all rigidbodies within the given radius
    /// </summary>
    /// <returns></returns>
	private IEnumerator waitAndExplode(){
		Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position,radius).Where ( c => c.attachedRigidbody != null ).ToArray ();
     
        yield return new WaitForFixedUpdate ();

		appliedForces.Clear ();

		foreach ( Collider2D coll in colliders){
			AddExplosionForce ( coll.attachedRigidbody, force, transform.position, radius, upliftModifer);
		}
	}

    /// <summary>
    /// adds explosion force to given rigidbody
    /// </summary>
    /// <param name="body">rigidbody to add force to</param>
    /// <param name="explosionForce">base force of explosion</param>
    /// <param name="explosionPosition">location of the explosion source</param>
    /// <param name="explosionRadius">radius of explosion effect</param>
    /// <param name="upliftModifier">factor of additional upward force</param>
    private void AddExplosionForce(Rigidbody2D body, float explosionForce, Vector3 explosionPosition, float explosionRadius, float upliftModifier = 0)
	{
		var dir = (body.transform.position - explosionPosition);	
		float wearoff = 1 - (dir.magnitude / explosionRadius)*0; // TODO : Dunno why this works, but it works to explode outwards with the *0, otherwise it pulls inwards
        Vector3 baseForce = dir.normalized * explosionForce * wearoff;
        baseForce.z = 0;
		body.AddForce(baseForce);
		var appliedForce = ( body , baseForce );

        if (upliftModifer != 0)
        {
            float upliftWearoff = 1 - upliftModifier / explosionRadius;
            Vector3 upliftForce = Vector2.up * explosionForce * upliftWearoff;
            upliftForce.z = 0;
            body.AddForce(upliftForce);

			appliedForce.baseForce += upliftForce;
		}

		appliedForces.Add ( appliedForce );

	}

#if UNITY_EDITOR
	private void OnDrawGizmos ()
	{
		Handles.DrawWireDisc ( transform.position , Vector3.back , radius );
		Handles.DrawLine ( transform.position , transform.position + Vector3.up * upliftModifer );

		if ( debug )
			using ( new Handles.DrawingScope ( Color.red ) )
				foreach ( var AppliedForce in appliedForces )
					Handles.DrawLine ( AppliedForce.r.position , ( Vector3 ) AppliedForce.r.position + AppliedForce.force );
	}
#endif

}
