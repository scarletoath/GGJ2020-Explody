using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragInput : MonoBehaviour
{

    GameObject hoveredObject;

    GameObject draggedObject;
    Vector3 dragOffset;

    float rotationValue;
    [SerializeField] float rotationSpeed;

    Vector2 draggedVelocity;

    [SerializeField] float maxHoldTime;
    float startHoldTime = -1;


    // Update is called once per frame
    void Update()
    {
        if (draggedObject != null)
        {
            // check if it has snapped to position
            if (draggedObject.GetComponent<SnapToLocation>().GetPositionIsFixed())
            {
                // snapped, let go
                Debug.Log("Snapped");
                draggedObject = null;
                dragOffset = Vector3.zero;
            }
            else if (startHoldTime != -1 && (Time.time - startHoldTime) > maxHoldTime)
            {
                Debug.Log("Ran out of time");
                draggedObject.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
                draggedObject.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                draggedObject.gameObject.GetComponent<Rigidbody2D>().AddForce(draggedVelocity * 50);
                draggedObject = null;
                dragOffset = Vector3.zero;
                startHoldTime = -1;
            }
            else
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 oldPosition = draggedObject.transform.position;
                draggedObject.transform.position = new Vector3(mousePos.x, mousePos.y, draggedObject.transform.position.z) + dragOffset;

                if (rotationValue != 0)
                {
                    Vector3 prevRot = draggedObject.transform.rotation.eulerAngles;
                    prevRot.z += rotationValue * rotationSpeed;
                    draggedObject.transform.rotation = Quaternion.Euler(prevRot);
                    rotationValue = 0;
                }

                draggedVelocity = ((Vector2)draggedObject.transform.position - oldPosition) / Time.deltaTime;
            }
        }

        HandleHover();

        if (hoveredObject != null && draggedObject == null)
        {
            if (rotationValue != 0)
            {
                Vector3 prevRot = hoveredObject.transform.rotation.eulerAngles;
                prevRot.z += rotationValue * rotationSpeed;
                hoveredObject.transform.rotation = Quaternion.Euler(prevRot);
                rotationValue = 0;
            }
        }
    }

    public void HandleGrab(bool down)
    {
        if (down)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
            if (hit)
            {
                if (hit.transform.CompareTag("Piece") && !hit.transform.gameObject.GetComponent<SnapToLocation>().GetPositionIsFixed()) // subject to change
                {
                    Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    dragOffset = hit.transform.position - mousePos;
                    dragOffset.z = 0;
                    draggedObject = hit.transform.gameObject;
                    draggedObject.gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                    startHoldTime = Time.time;
                    // Wwise Audio Event
                    PlayRandomSFX ();
                }
            }
        }
        else if (draggedObject != null)
        {
            Debug.Log("Voluntarily let go");
            draggedObject.gameObject.GetComponent<Rigidbody2D>().isKinematic = false;
            draggedObject.gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            draggedObject.gameObject.GetComponent<Rigidbody2D>().AddForce(draggedVelocity * 70);
            draggedObject = null;
            dragOffset = Vector3.zero;
            startHoldTime = -1;
        }
    }

    public void HandleHover()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);
        if (hit)
        {
            if (hit.transform.CompareTag("Piece") && !hit.transform.gameObject.GetComponent<SnapToLocation>().GetPositionIsFixed()) // subject to change
            {
                hoveredObject =  hit.transform.gameObject;
            }
        }
        else if ( hoveredObject != null )
        {
            hoveredObject = null;

        }
    }

public void HandleRotation(float rotationVal)
    {
        rotationValue = rotationVal;
    }

public void PlayRandomSFX()
    {
        string[] eventNames =
        {
            "clickShardArp",
            "clickShardBass",
            "ClickShardCello",
            "ClickShardMangled",
            "ClickShardPizz",
            "ClickShardScrubs",
            "ClickShardSwells",
            "ClickShardViola"
        };
        AkSoundEngine.PostEvent(eventNames[Random.Range(0, eventNames.Length)], gameObject);
    }

}
