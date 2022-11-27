using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

/// <summary>
/// Manages weapons owned by the Player. 
/// All Weapons will not function without the Weapon Manager present in the Player's hierarchy.
/// Must be a child of the Player, and weapons must be a child of the Weapon Manager.
/// </summary>

public class WeaponManager : MonoBehaviour
{
	public int state;
	public static bool AllowSwitching;
	public static bool WeaponsLocked;
	public Camera ViewmodelCamera;
	public FOVManager CameraFOVManager;
	public ViewShake CameraShake;
	public CameraRecoil recoil;
	public CameraWobble cameraWobble;
	public Transform head;
	public List<BaseWeapon> Weapons = new List<BaseWeapon>();
	public static BaseWeapon CurrentWeapon;
	public static BallisticWeapon CurrentBallisticWeapon;
	public static bool InSwap;

	// Start is called before the first frame update
	void Start()
	{
		AllowSwitching = true;
		InSwap = false;
	}

	public void HideAllViewmodels()
	{
		for (int i = 0; i < Weapons.Count; i++)
		{
			Weapons[i].HideViewmodel();
		}
	}

	public void ShowAllViewmodels()
	{
		for (int i = 0; i < Weapons.Count; i++)
		{
			Weapons[i].ShowViewmodel();
		}
	}

	public void RefreshWeapons()
    {
		BaseWeapon b = null;
		foreach (Transform t in transform)
        {
			if (!t.gameObject.activeInHierarchy) continue;
			t.TryGetComponent(out b);
			if (b)
            {
				Weapons.Add(b);
            }
        }
    }

	public void ChangeWeapon(int to)
    {
		if (to != state)
        {
			state = to;
			Events.InvokeWeaponChanged();

			for (int i = 0; i < Weapons.Count; i++)
			{
				Weapons[i].ForceUpdate();
			}

			Events.InvokeOnWeaponSwapped(CurrentWeapon);
        }
    }

	void Update()
	{
		if (!InSwap && AllowSwitching)
		{
			if (Input.GetKeyDown(KeyCode.Alpha1))
			{
				ChangeWeapon(1);
			}

			if (Input.GetKeyDown(KeyCode.Alpha2))
			{
				ChangeWeapon(2);
			}

			if (Input.GetKeyDown(KeyCode.Alpha3))
			{
				ChangeWeapon(3);
			}

			if (Input.GetKeyDown(KeyCode.Alpha4))
			{
				ChangeWeapon(4);
			}

			if (Input.GetKeyDown(KeyCode.Alpha5))
			{
				ChangeWeapon(5);
			}

			if (Input.GetKeyDown(KeyCode.Alpha6))
			{
				ChangeWeapon(6);
			}
		}

		state = Mathf.Clamp(state, -20, Weapons.Count + 1);
	}
}
