using UnityEngine;
using System.Collections;

public class ParticleDirection : MonoBehaviour {

	public Transform weapon;

	void Update () {
		transform.position = weapon.TransformPoint(Vector3.zero);
		transform.forward = weapon.TransformDirection(Vector3.forward);
	}

	void OnParticleCollision(GameObject other) {
		Rigidbody otherRigidbody = other.GetComponent<Rigidbody>();
		if(otherRigidbody) {
			Vector3 direction = other.transform.position - transform.position;
			direction = direction.normalized;

			otherRigidbody.AddForce(direction * 50);
		}
	}
}
