using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour {

	public List<GameObject> WeaponList = new List<GameObject>();
	public WeaponControl ActiveWeapon;
	int weaponNumber = 0; // active weapon's number

	public bool aim;

	public enum WeaponType {
		Pistol,
		Rifle
	}

	public WeaponType weaponType;

	Animator anim; // of the player

	float IKweight;

	void Start () {
		// Assuming player starts with a weapon, get that weapon
		ActiveWeapon = WeaponList [weaponNumber].GetComponent<WeaponControl> ();
		ActiveWeapon.equip = true;

		anim = GetComponent<Animator>();

		foreach (GameObject go in WeaponList) {
			go.GetComponent<WeaponControl>().hasOwner = true;
		}
	}

	void Update () {
		IKweight = Mathf.MoveTowards (IKweight, (aim) ? 1.0f : 0.0f, Time.deltaTime * 5); 

		ActiveWeapon = WeaponList [weaponNumber].GetComponent<WeaponControl> ();
		ActiveWeapon.equip = true;

		weaponType = ActiveWeapon.weaponType;

		switch (weaponType) {
		case WeaponType.Pistol:
			anim.SetInteger ("Weapon", 0);
			break;
		case WeaponType.Rifle:
			anim.SetInteger("Weapon", 1);
			break;
		}
	}

	void OnAnimatorIK() {
		anim.SetIKPositionWeight (AvatarIKGoal.LeftHand, IKweight);
		anim.SetIKRotationWeight (AvatarIKGoal.LeftHand, IKweight);

		Vector3 pos = ActiveWeapon.HandPosition.transform.TransformPoint (Vector3.zero);

		anim.SetIKPosition (AvatarIKGoal.LeftHand, ActiveWeapon.HandPosition.transform.position);
		anim.SetIKRotation (AvatarIKGoal.LeftHand, ActiveWeapon.HandPosition.transform.rotation);
	}

	public void FireActiveWeapon() {
		if (ActiveWeapon != null) {
			ActiveWeapon.Fire ();
		}
	}

	// Change weapon on mousewheel (mousewheel can go ascending or descending)
	public void ChangeWeapon(bool Ascending) {
		if (WeaponList.Count > 1) {
			ActiveWeapon.equip = false;

			if(Ascending) {
				if(weaponNumber < WeaponList.Count - 1) {
					weaponNumber++;
				}
				else {
					weaponNumber = 0;
				}
			}
			else {
				if(weaponNumber > 0) {
					weaponNumber--;
				}
				else {
					weaponNumber = WeaponList.Count - 1;
				}
			}
		}
	}
}
