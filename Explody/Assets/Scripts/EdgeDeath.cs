using System.Collections;
using UnityEngine;

public class EdgeDeath : MonoBehaviour
{

	[SerializeField] private GameObject fxPrefab;
	[SerializeField] private float fxLifetime;

    void OnTriggerEnter2D(Collider2D col)
    {
        // do explosion effects
        if (col.CompareTag("Piece"))
		{
			Debug.Log ( $"destroy {col}" );
			GameController.Instance.UnregisterSnap ( col.GetComponent <SnapToLocation> () );
			StartCoroutine ( DestroyFxAfterTime ( Instantiate ( fxPrefab ).transform , col.gameObject ) );
            col.gameObject.SetActive( false );
        }
    }

	private IEnumerator DestroyFxAfterTime ( Transform fxInstance , GameObject fxSource )
	{
		fxInstance.position = fxSource.transform.position;
		var fxSourceRigidbody = fxSource.GetComponent <Rigidbody2D> ();
		if ( fxSourceRigidbody != null )
		{
			fxInstance.rotation = Quaternion.LookRotation ( -fxSourceRigidbody.velocity );
		}

		yield return new WaitForSecondsRealtime ( fxLifetime );
		Destroy ( fxInstance.gameObject );
	}

}
