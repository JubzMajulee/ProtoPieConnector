using UnityEngine;

/// <summary>
/// Rotates a GameObject around its Y-axis based on horizontal mouse or touch drag.
/// </summary>
public class DragRotator : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("How fast the object spins. Adjust higher for touch screens if it feels sluggish.")]
    public float rotationSpeed = 150f;

    [Tooltip("If true, dragging right spins the object right. If false, it acts like you are spinning a globe.")]
    public bool invertDirection = true;

    void Update()
    {
        float dragDeltaX = 0f;

        // 1. Check for Touch Input (Mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            // Only register movement while the finger is actively sliding
            if (touch.phase == TouchPhase.Moved)
            {
                dragDeltaX = touch.deltaPosition.x;
            }
        }
        // 2. Fallback to Mouse Input (PC / Editor)
        else if (Input.GetMouseButton(0))
        {
            // Input.GetAxis("Mouse X") gets the horizontal mouse movement between frames.
            // We multiply it by 10 to roughly match the scale of touch delta pixels.
            dragDeltaX = Input.GetAxis("Mouse X") * 10f;
        }

        // 3. Apply the Rotation
        if (dragDeltaX != 0f)
        {
            // Determine direction based on the invert toggle
            float direction = invertDirection ? -1f : 1f;

            // Rotate around the global Y (Up) axis. 
            // Using Space.World prevents the object from wobbling if it is already tilted.
            transform.Rotate(Vector3.up, dragDeltaX * rotationSpeed * direction * Time.deltaTime, Space.World);
        }
    }
}