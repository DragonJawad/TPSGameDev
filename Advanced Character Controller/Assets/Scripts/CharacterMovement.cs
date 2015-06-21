using UnityEngine;
using System.Collections;

public class CharacterMovement : MonoBehaviour {

	float moveSpeedMultiplier = 1;
	float stationaryTurnSpeed = 180;
	float movingTurnSpeed = 360;
	
	bool onGround;
	
	Animator anim;
	
	Vector3 moveInput;
	float turnAmount;
	float forwardAmount;
	Vector3 velocity;
	
	float jumpPower = 10;

	IComparer rayHitComparer;

	float autoTurnThreshold = 10;
	float autoTurnSpeed = 20;	
	bool aim;			
	Vector3 currentLookPos;

	Rigidbody rigidbody;

	float lastAirTime;
	Collider col;
	public PhysicMaterial highFriction;
	public PhysicMaterial lowFriction;

	// Use this for initialization
	void Start () {
		anim = GetComponentInChildren<Animator> ();
		rigidbody = GetComponent<Rigidbody> ();
		col = GetComponent<Collider> ();

		SetupAnimator();
	}
	
	public void Move(Vector3 move, bool aim, Vector3 lookPos) {
		if(move.magnitude > 1)
			move.Normalize();
		
		this.moveInput = move;
		this.aim = aim;
		this.currentLookPos = lookPos;
		
		velocity = rigidbody.velocity;
		
		ConvertMoveInput();

		if(!aim) {
			TurnTowardsCameraForward();
			ApplyExtraTurnRotation();
		}
		GroundCheck();
		SetFriction ();
		if (onGround) {
			HandleGroundVelocities ();
		} else {
			HandleAirborneVelocities();
		}
		UpdateAnimator();
	}
	
	void SetupAnimator() {
		anim = GetComponent<Animator>();
		
		// Something to do with being independent of exact avatar...
		foreach(Animator childAnimator in GetComponentsInChildren<Animator>()) {
			if(childAnimator != anim) {
				anim.avatar = childAnimator.avatar;
				Destroy(childAnimator);
				break; // May have more animators, but only want the first animator
			}
		}
	}
	
	void OnAnimatorMove() {
		if(onGround && Time.deltaTime > 0) {
			// Change in position - ie velocity - for this frame
			Vector3 v = (anim.deltaPosition * moveSpeedMultiplier)/Time.deltaTime;
			
			v.y = rigidbody.velocity.y; // Don't want to mess with this
			rigidbody.velocity = v;
		}
	}
	
	void ConvertMoveInput() {
		Vector3 localMove = transform.InverseTransformDirection(moveInput);

		if(!aim) {
			turnAmount = Mathf.Atan2(localMove.x, localMove.z);
			forwardAmount = localMove.z;
		}
		else {
			turnAmount = 0;
			forwardAmount = 0;
		}
	}
	
	void UpdateAnimator() {
		anim.applyRootMotion = true;

		if (!aim) {
			anim.SetFloat ("Forward", forwardAmount, 0.1f, Time.deltaTime);
			anim.SetFloat ("Turn", turnAmount, 0.1f, Time.deltaTime);
		}
		anim.SetBool("Aim", aim);

		anim.SetBool ("OnGround", onGround);
	}
	
	void ApplyExtraTurnRotation() {
		float turnSpeed = Mathf.Lerp(stationaryTurnSpeed, movingTurnSpeed, forwardAmount);
		transform.Rotate(0, turnAmount * turnSpeed * Time.deltaTime, 0); // Rotate at y
	}
	
	void GroundCheck() {
		Ray ray = new Ray(transform.position + Vector3.up * 0.5f, -Vector3.up);
		
		RaycastHit[] hits = Physics.RaycastAll(ray, 0.5f);
		rayHitComparer = new RayHitComparer();
		
		System.Array.Sort (hits, rayHitComparer); // Sort hits based on distance
		
		// If player is not jumping (or not able to jump or something)
		if(velocity.y < jumpPower * 0.5f) {
			onGround = false;
			// Assume in the air and falling...
			rigidbody.useGravity = true;
			
			foreach(var hit in hits) {
				if(!hit.collider.isTrigger) {
					// If haven't hit any hits that is a collider, stick to the ground
					if(velocity.y <= 0) {
						// Change rigidbody position to be at the hit point
						rigidbody.position = Vector3.MoveTowards(rigidbody.position, hit.point, Time.deltaTime*5);
					}
					
					onGround = true;
					rigidbody.useGravity = false;
					
					break;
				}
			}
		}

		if (!onGround) {
			lastAirTime = Time.time;
		}
	}

	void TurnTowardsCameraForward() {
		if(Mathf.Abs(forwardAmount) < .01f) {
			Vector3 lookDelta = transform.InverseTransformDirection(currentLookPos - transform.position);

			float lookAngle = Mathf.Atan2(lookDelta.x, lookDelta.z) * Mathf.Rad2Deg;

			if(Mathf.Abs (lookAngle) > autoTurnThreshold) {
				turnAmount += lookAngle * autoTurnSpeed * .001f;
			}
		}
	}

	void SetFriction() {
		if (onGround) {
			if (moveInput.magnitude == 0) {
				// No sliding when no movement
				col.material = highFriction;
			} else {
				col.material = lowFriction;
			}
		} else {
			// If in air, want low friction as well
			col.material = lowFriction;
		}
	}

	// Helps with no unintentional sliding
	void HandleGroundVelocities() {
		velocity.y = 0;

		// If no movement..
		if (moveInput.magnitude == 0) {
			velocity.x = 0;
			velocity.z = 0;
		}
	}

	void HandleAirborneVelocities() {
		// Only small changes to trajectory
		Vector3 airMove = new Vector3 (moveInput.x * 6, velocity.y, moveInput.z * 6);
		velocity = Vector3.Lerp (velocity, airMove, Time.deltaTime * 2);

		rigidbody.useGravity = true;

		Vector3 extraGravityForce = (Physics.gravity * 2);

		rigidbody.AddForce (extraGravityForce);
	}
	
	class RayHitComparer: IComparer {
		// Compares two raycasts, and gets the shorter one (or something)
		public int Compare(object x, object y) {
			return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
		}
	}
}
