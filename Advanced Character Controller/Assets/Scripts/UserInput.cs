using UnityEngine;
using System.Collections;

public class UserInput : MonoBehaviour {

    public bool walkByDefault = false;

    private CharacterMovement charMove;
    private Transform cam; // Camera
    private Vector3 camForward;
    private Vector3 move;
	
	public bool aim;
	public float aimingWeight;

	public bool lookInCameraDirection;
	Vector3 lookPos;

	Animator anim;

	WeaponManager weaponManager;

	public bool debugShoot;
	WeaponManager.WeaponType weaponType;

	CapsuleCollider col;
	float startHeight; 

	// Ik stuff - specific to character/rig
	[SerializeField] public IK ik;
	[System.Serializable] public class IK {
		public Transform spine;
		public float aimingZ = 213.64f;
		public float aimingX = -65.93f;
		public float aimingY = 20.1f;
		public float point = 30f;
		public bool DebugAim;
	}

    void Start()
    {
        if (Camera.main != null) {
            cam = Camera.main.transform;
        }

        charMove = GetComponent<CharacterMovement>();

		anim = GetComponent<Animator>();

		weaponManager = GetComponent<WeaponManager> ();

		col = GetComponent<CapsuleCollider> ();
		startHeight = col.height;
    }

	void CorrectIK() {
		weaponType = weaponManager.weaponType;

		if (!ik.DebugAim) {
			switch(weaponType) {
			case WeaponManager.WeaponType.Pistol:
				ik.aimingZ = 221.4f;
				ik.aimingX = -71.5f;
				ik.aimingY = 20.6f;
				break;
			case WeaponManager.WeaponType.Rifle:
				ik.aimingZ = 212.19f;
				ik.aimingX = -66.1f;
				ik.aimingY = 14.1f;
				break;
			}
		}
	}

	void AdditionalInput() {
		if (anim.GetFloat ("Forward") > 0.5f) {
			if (Input.GetButtonDown ("Crouch")) {
				anim.SetTrigger ("Vault");
			}
		}
	}

	void HandleCurves() {
		float sizeCurve = anim.GetFloat ("ColliderSize");
		float newYcenter = 0.3f;

		float lerpCenter = Mathf.Lerp (1, newYcenter, sizeCurve);

		col.center = new Vector3 (0, lerpCenter, 0);

		col.height = Mathf.Lerp (startHeight, 0.5f, sizeCurve);
	}

	void Update() {
		CorrectIK ();

		// Only get aim from mouse button if not debugging
		if(!ik.DebugAim)
			// Get whether or not the player wants to aim
			aim = Input.GetMouseButton(1);

		weaponManager.aim = aim;

		if(aim) {
			// If the weapon can't fire more than once at a time...
			if(!weaponManager.ActiveWeapon.CanBurst) {
				// Fire only once per click
				if(Input.GetMouseButtonDown(0) || debugShoot) {
					anim.SetTrigger("Fire");
					weaponManager.FireActiveWeapon();
				}
			}
			else {
				// Fire as long as mouse button is held down
				if(Input.GetMouseButton(0) || debugShoot) {
					anim.SetTrigger("Fire");
					weaponManager.FireActiveWeapon();
				}
			}
		}

		// If mouse scrollwheel is actually getting moved...
		if(Input.GetAxis("Mouse ScrollWheel") != 0) {
			// If mousewheel is scrolled down...
			if(Input.GetAxis("Mouse ScrollWheel") < -0.1f) {
				weaponManager.ChangeWeapon(false);
			}
			
			// If mousewheel is scrolled up...
			if(Input.GetAxis("Mouse ScrollWheel") > 0.1f) {
				weaponManager.ChangeWeapon(true);
			}
		}
		
		// For ease of testing without a mouse, do the same thing with a key...
		if(Input.GetKeyDown(KeyCode.E)) {
			Debug.Log("Manually changing weapons...");
			weaponManager.ChangeWeapon(true);
		}

		AdditionalInput ();
		HandleCurves ();
	}

	void LateUpdate() {	
		aimingWeight = Mathf.MoveTowards(aimingWeight, (aim)? 1.0f : 0.0f, Time.deltaTime * 5);

		Vector3 normalState = new Vector3(0,0,0);
		Vector3 aimingState = new Vector3(0.5f,-0.5f,-0.9f);
		Vector3 pos = Vector3.Lerp (normalState, aimingState, aimingWeight);

		cam.transform.localPosition = pos;

		if(aim) {
			Vector3 eulerAngleOffset = Vector3.zero;

			eulerAngleOffset = new Vector3(ik.aimingX, ik.aimingY, ik.aimingZ);

			Ray ray = new Ray(cam.position, cam.forward);

			Vector3 lookPosition = ray.GetPoint(ik.point);

			ik.spine.LookAt(lookPosition);
			ik.spine.Rotate (eulerAngleOffset, Space.Self);
		}
	}
    
	void FixedUpdate() {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

		if(!aim) {
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
		}
		else {
			move = Vector3.zero;

			Vector3 dir = lookPos - transform.position;
			dir.y = 0;

			transform.rotation = Quaternion.Slerp (transform.rotation, Quaternion.LookRotation(dir), 20*Time.deltaTime);

			anim.SetFloat("Forward", vertical);
			anim.SetFloat("Turn", horizontal);
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

		lookPos = lookInCameraDirection && cam != null ? transform.position + cam.forward * 100
			: transform.position + transform.forward*100;

        move *= walkMultiplier;

        charMove.Move(move, aim, lookPos);
    }
}
