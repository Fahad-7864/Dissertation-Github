using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * The CameraController manages the camera's movement based on user input.
 * It supports both mouse-based interactions on computers and touch-based interactions 
 * on devices with touch screens. Users can pan the camera by dragging with 
 * either the mouse or their finger.
 * Touch screen will not be intregrated in the final project,
 */
public class CameraController : MonoBehaviour
{
    // Speed of the camera when being panned
    public float panSpeed = 20f;
    // Thickness of the edge of the screen for panning the camera
    public float panBorderThickness = 10f;  

    private Vector3 lastPanPosition;
    // Touch mode only
    private int panFingerId;
    // Touch mode only
    private bool wasDragging; 

    // Update is called once per frame
    void Update()
    {
        // Handle mouse
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            PanCamera(Input.mousePosition);
        }

        // Handle touch
        if (Input.touchCount == 1)
        {
            wasDragging = true;
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = touch.position;
                panFingerId = touch.fingerId;
            }
            else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
            {
                PanCamera(touch.position);
            }
        }
        else
        {
            wasDragging = false;
        }
    }

    void PanCamera(Vector3 newPanPosition)
    {
        // Determine how much to move the camera
        Vector3 offset = Camera.main.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * panSpeed, offset.y * panSpeed, 0);

        // Perform the movement
        transform.Translate(move, Space.World);

        // Ensure the camera remains within bounds.
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(transform.position.x, -panBorderThickness, panBorderThickness);
        pos.y = Mathf.Clamp(transform.position.y, -panBorderThickness, panBorderThickness);
        transform.position = pos;

        // Cache the position
        lastPanPosition = newPanPosition;
    }
}
