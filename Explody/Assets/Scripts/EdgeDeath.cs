using UnityEngine;

public class EdgeDeath : MonoBehaviour
{
    public Transform particle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // do explosion effects
        if (col.CompareTag("Piece"))
		{
			Debug.Log ( $"destroy {col}" );
			GameController.Instance.UnregisterSnap ( col.GetComponent <SnapToLocation> () );
            Destroy(col.gameObject);
        }
    }

}
