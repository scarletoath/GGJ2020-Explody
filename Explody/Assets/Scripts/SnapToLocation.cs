using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnapToLocation : MonoBehaviour
{
    [SerializeField]
    float tolerance;

    Vector3 originalLocation;
    Quaternion originalRotation;
    bool lookForSnaps;

    bool positionIsFixed;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(lookForSnaps)
        {
            if(Vector3.Distance(originalLocation, transform.position) < tolerance)
            {
                lookForSnaps = false;
                positionIsFixed = true;
                transform.position = originalLocation;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // set snap location
            SetSnapLocation();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            //start tracking
            StartTracking();
        }

        if (GetPositionIsFixed())
        {
            GetComponent<Rigidbody2D>().simulated = false;
        }

    }

    public void SetSnapLocation()
    {
        Debug.Log("Setting snap location");
        originalLocation = transform.position;
        originalRotation = transform.rotation;
    }

    public void StartTracking()
    {
        Debug.Log("Starting to track for snaps");
        lookForSnaps = true;
    }

    public bool GetPositionIsFixed()
    {
        return positionIsFixed;
    }

    public float GetScore()
    {
        if(positionIsFixed)
        {
            float rotationScore = Quaternion.Angle(transform.rotation, originalRotation);
            return (0.5f + (rotationScore * 0.5f));
        }
        else
        {
            return 0;
        }
    }
}
