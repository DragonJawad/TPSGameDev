using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour {

    public bool walkByDefault = false;

    private CharacterMovement charMove;
    private Transform cam; // Camera
    private Vector3 camForward;
    private Vector3 move;

    void Start()
    {
        if (Camera.main != null) {
            cam = Camera.main.transform;
        }

        charMove = GetComponent<CharacterMovement>();
    }

    void FixedUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (cam != null)
        {
            camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;

            // Move towards where camera is looking
            move = vertical * camForward + horizontal * cam.right;
        }
        else {
            // Just in case there is no camera in the scene...
            move = vertical * Vector3.forward + horizontal * Vector3.right;
        }

        if (move.magnitude > 1)
            move.Normalize();

        bool walkToggle = Input.GetKey(KeyCode.LeftShift);

        float walkMultiplier = 1;

        // Multiply the move vector as necessary, as 1 is run by default in animator
        if (walkByDefault)
        {
            if (walkToggle)
            {
                walkMultiplier = 1;
            }
            else
            {
                walkMultiplier = 0.5f;
            }
        }
        else {
            if (walkToggle)
            {
                walkMultiplier = 0.5f;
            }
            else {
                walkMultiplier = 1;
            }
        }

        move *= walkMultiplier;

        charMove.Move(move);
    }
}
