using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour {

    public bool walkByDefault = false;

    private CharacterMovement charMove;
    private Transform cam; // Camera
    private Vector3 camForward;
    private Vector3 move;

	//Camera
	// float cameraForward;
	public bool aim;
	public float aimingWeight;
	//float cameraSpeedOffset

    void Start()
    {
        if (Camera.main != null) {
            cam = Camera.main.transform;
        }

        charMove = GetComponent<CharacterMovement>();
    }

	void LateUpdate() {
		aim = Input.GetMouseButton(1);
	
		aimingWeight = Mathf.MoveTowards(aimingWeight, (aim)? 1.0f : 0.0f, Time.deltaTime * 5);

		Vector3 normalState = new Vector3(0,0,0f);
		Vector3 aimingState = new Vector3(0.5f,0,-0.5f);
		Vector3 pos = Vector3.Lerp (normalState, aimingState, aimingWeight);

		cam.transform.localPosition = pos;
	}
    
	void FixedUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (cam != null) // if there is a camera
        {
			// Take the forward vector of the camera (from its transform) and
			// eliminate the y component
			// scale the camera forward with the mask (1, 0, 1) to eliminate y and normalize it
            camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;

            // Move towards where camera is looking
            move = vertical * camForward + horizontal * cam.right;
        }
        else {
            // Just in case there is no camera in the scene...
			// use the global forward (+z) amd right (+x)
            move = vertical * Vector3.forward + horizontal * Vector3.right;
        }

        if (move.magnitude > 1)
            move.Normalize();

        bool walkToggle = Input.GetKey(KeyCode.LeftShift) || aim; // check for walking input

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
