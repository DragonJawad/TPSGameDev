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

	// Use this for initialization
	void Start () {
		SetupAnimator();
	}
	
	public void Move(Vector3 move) {
		if(move.magnitude > 1)
			move.Normalize();
		
		this.moveInput = move;
		
		velocity = GetComponent<Rigidbody>().velocity;
		
		ConvertMoveInput();
		ApplyExtraTurnRotation();
		GroundCheck();
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
			
			v.y = GetComponent<Rigidbody>().velocity.y; // Don't want to mess with this
			GetComponent<Rigidbody>().velocity = v;
		}
	}
	
	void ConvertMoveInput() {
		Vector3 localMove = transform.InverseTransformDirection(moveInput);
		
		turnAmount = Mathf.Atan2(localMove.x, localMove.z);
		forwardAmount = localMove.z;
	}
	
	void UpdateAnimator() {
		anim.applyRootMotion = true;
		
		anim.SetFloat("Forward", forwardAmount, 0.1f, Time.deltaTime);
		anim.SetFloat("Turn", turnAmount, 0.1f, Time.deltaTime);
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
			GetComponent<Rigidbody>().useGravity = true;
			
			foreach(var hit in hits) {
				if(!hit.collider.isTrigger) {
					// If haven't hit any hits that is a collider, stick to the ground
					if(velocity.y <= 0) {
						// Change rigidbody position to be at the hit point
						GetComponent<Rigidbody>().position = Vector3.MoveTowards(GetComponent<Rigidbody>().position, hit.point, Time.deltaTime*5);
					}
					
					onGround = true;
					GetComponent<Rigidbody>().useGravity = false;
					
					break;
				}
			}
		}
	}
	
	class RayHitComparer: IComparer {
		// Compares two raycasts, and gets the shorter one (or something)
		public int Compare(object x, object y) {
			return ((RaycastHit)x).distance.CompareTo(((RaycastHit)y).distance);
		}
	}
}
