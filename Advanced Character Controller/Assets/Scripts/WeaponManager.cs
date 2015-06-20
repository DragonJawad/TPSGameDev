using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WeaponManager : MonoBehaviour {

	public List<GameObject> WeaponList = new List<GameObject>();
	public WeaponControl ActiveWeapon;
	int weaponNumber = 0; // active weapon's number

	public enum WeaponType {
		Pistol,
		Rifle
	}

	public WeaponType weaponType;

	Animator anim; // of the player

	public IKTargetPos ikTargetPos;
	[System.Serializable] public class IKTargetPos {
		[Header("Targets")]
		public Transform HandPlacement;
		public Transform ElbowPlacement;

		[Header("Elbow Positions")]
		public Vector3 elbowPistolPos = new Vector3(-2.30f, 0.9f, 2.78f);
		public Vector3 elbowRiflePos = new Vector3(-2.30f, 0.9f, 2.78f);

		public bool DebugIK;
	}

	void Start () {
		// Assuming player starts with a weapon, get that weapon
		ActiveWeapon = WeaponList [weaponNumber].GetComponent<WeaponControl> ();
		ikTargetPos.HandPlacement = ActiveWeapon.HandPosition.transform;
		ikTargetPos.ElbowPlacement = new GameObject ().transform;
		ActiveWeapon.equip = true;

		ikTargetPos.ElbowPlacement.parent = transform;

		anim = GetComponent<Animator>();

		foreach (GameObject go in WeaponList) {
			go.GetComponent<WeaponControl>().hasOwner = true;
		}
	}

	void Update () {
		ActiveWeapon = WeaponList [weaponNumber].GetComponent<WeaponControl> ();
		ikTargetPos.HandPlacement = ActiveWeapon.HandPosition.transform;
		ActiveWeapon.equip = true;

		if (!ikTargetPos.DebugIK) {
			switch(ActiveWeapon.weaponType) {
				case WeaponType.Pistol:
					anim.SetInteger("Weapon", 0);
					ikTargetPos.ElbowPlacement.localPosition = ikTargetPos.elbowPistolPos;

					break;
				case WeaponType.Rifle:
					anim.SetInteger("Weapon", 1);
					ikTargetPos.ElbowPlacement.localPosition = ikTargetPos.elbowRiflePos;

					break;
			}
		}
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
