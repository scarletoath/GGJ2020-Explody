using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Eruption : MonoBehaviour
{
    [SerializeField] float eruptionForce;

    // Start is called before the first frame update
    //void Start()
    //{

    //}

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("Erupting");
            StartEruption();
        }
    }

    public void StartEruption()
    {
        GameObject[] Pieces = GameObject.FindGameObjectsWithTag("Piece");
        foreach (GameObject obj in Pieces)
        {
            obj.GetComponent<SnapToLocation>().SetSnapLocation();
            Vector2 force = (obj.transform.position - transform.position).normalized * eruptionForce;
            obj.GetComponent<Rigidbody2D>().AddForce(force);
        }
        StartCoroutine(SetSnappables());
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