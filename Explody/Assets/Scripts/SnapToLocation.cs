using UnityEngine;

public class SnapToLocation : MonoBehaviour
{
    [SerializeField] float tolerance;

    Vector3 originalLocation;
    Quaternion originalRotation;
    public bool lookForSnaps;

    bool positionIsFixed;

    public ParticleSystem lockedInAchieved;

    // Start is called before the first frame update
    void Start()
    {
        SetSnapLocation();
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
                AkSoundEngine.PostEvent("playSnap", gameObject);// Wwise Audio Event @ekampa SNAP
                CreateLockedInAchieved();
				GameController.Instance.UnregisterSnap ( this );
			}
        }

        if (GetPositionIsFixed())
        {
            GetComponent<Rigidbody2D>().simulated = false;
        }

    }

    public void SetSnapLocation()
    {
        Debug.Log("Setting snap location " + transform.position);
        originalLocation = transform.position;
        originalRotation = transform.rotation;
    }

    public void StartTracking()
    {
        Debug.Log($"Starting to track for snaps {name}");
        lookForSnaps = true;
		//GameController.Instance.RegisterSnap ( this );
	}

    public bool GetPositionIsFixed()
    {
        return positionIsFixed;
    }

    public float GetScore()
    {
        if(positionIsFixed)
		{
			float rotationScore = 1 - Mathf.Abs ( Quaternion.Angle ( transform.rotation , originalRotation ) ) / 180f;
			return (0.5f + (rotationScore * 0.5f));
		}
        else
        {
            return 0;
        }
    }

    public void SetTolerance(float tol)
    {
        tolerance = tol;
    }

    void CreateLockedInAchieved()
    {
        lockedInAchieved.Play();
    }
}
