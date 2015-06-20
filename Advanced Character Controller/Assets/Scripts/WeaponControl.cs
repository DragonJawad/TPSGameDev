using UnityEngine;
using System.Collections;

public class WeaponControl : MonoBehaviour {

	// Determines whether or not this weapon is currently equipped
	public bool equip;
	// Determines what type of weapon this object is
	public WeaponManager.WeaponType weaponType;

	public int MaxAmmo;
	public int MaxClipAmmo = 30;
	public int curAmmo;
	public bool CanBurst; // If weapon has burst fire

	public GameObject HandPosition;
	public GameObject bulletPrefab;
	public Transform bulletSpawn;
	GameObject bulletSpawnGO;
	ParticleSystem bulletPart;
	WeaponManager parentControl;

	bool fireBullet;
	AudioSource audioSource;
	Animator weaponAnim;

	[Header("Positions")]
	public bool hasOwner; // Override position depending on ownership (ie, weapon may be on ground or someone dropped and died, etc)
	public Vector3 EquipPosition;
	public Vector3 EquipRotation;
	public Vector3 UnEquipPosition;
	public Vector3 UnEquipRotation;
	// Debug Scale
	Vector3 scale;

	public RestPosition restPosition;
	public enum RestPosition {
		RightHip,
		Waist
	}

	void Start () {
		curAmmo = MaxClipAmmo;
		bulletSpawnGO = Instantiate (bulletPrefab, transform.position, Quaternion.identity) as GameObject;
		bulletSpawnGO.AddComponent<ParticleDirection> ();
		bulletSpawnGO.GetComponent<ParticleDirection> ().weapon = bulletSpawn;
		bulletPart = bulletSpawnGO.GetComponent<ParticleSystem> ();

		audioSource = GetComponent<AudioSource> ();
		weaponAnim = GetComponent<Animator> ();
		scale = transform.localScale;
	}

	void Update () {
		transform.localScale = scale;

		if (equip) {
			transform.parent = transform.GetComponentInParent<WeaponManager> ()
				.transform.GetComponent<Animator> ()
					.GetBoneTransform (HumanBodyBones.RightHand);
			transform.localPosition = EquipPosition;
			transform.localRotation = Quaternion.Euler (EquipRotation);

			if (fireBullet) {
				if (curAmmo > 0) {
					curAmmo--;
					bulletPart.Emit (1);
					audioSource.Play ();
					//	weaponAnim.SetTrigger("Fire");
					fireBullet = false;
				}

				else {
					if (MaxAmmo >= MaxClipAmmo) {
						curAmmo = MaxClipAmmo;
						MaxAmmo -= MaxClipAmmo;
					} else {
						curAmmo = MaxAmmo;
						MaxAmmo = 0;
					}

					fireBullet = false;
					Debug.Log ("Reload");
				}
			}
		} // if equip
		else {
			if(hasOwner) {
				switch (restPosition) {
					case RestPosition.RightHip:
						transform.parent = transform.GetComponentInParent<WeaponManager> ()
							.transform.GetComponent<Animator> ()
								.GetBoneTransform (HumanBodyBones.RightUpperLeg);

						break;

					case RestPosition.Waist:
						transform.parent = transform.GetComponentInParent<WeaponManager> ()
							.transform.GetComponent<Animator> ()
								.GetBoneTransform (HumanBodyBones.Spine);

						break;
				}

				transform.localPosition = UnEquipPosition;
				transform.localRotation = Quaternion.Euler(UnEquipRotation);
			}
		}
	}

	public void Fire() {
		fireBullet = true;
	}
}
