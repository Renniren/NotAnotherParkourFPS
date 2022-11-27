using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;


/// <summary>
/// Base class for all Weapons in the game.
/// </summary>
public class BaseWeapon : MonoBehaviorExtended
{
    [Header("Base Weapon Settings")]
    public Animator WeaponAnimator;
    public GameObject WeaponModel;
    public string Name;
    public string InternalName;
    public int Position;
    public bool AllowAutoSwap = true;
    public bool CanBeCleared = true;
    public float ViewmodelFOV = 69;

    public bool Active;
    [HideInInspector]public WeaponManager weaponManager;
    bool initialized;

    protected void Start()
    {
        weaponManager = GetComponentInParent<WeaponManager>();
        weaponManager.Weapons.Add(this);
        foreach (var item in weaponManager.Weapons)
        {
            if (item.Position == Position && item != this)
            {
                //item.Position++;
            }
        }
        initialized = true;
    }

    public void re_initialize()
    {
        if (!initialized)
        {
            weaponManager = GetComponentInParent<WeaponManager>();
            weaponManager.Weapons.Add(this);
            foreach (var item in weaponManager.Weapons)
            {
                if (item.Position == Position && item != this)
                {
                    //item.Position++;
                }
            }
        }
    }

    // Really weird code for hot-swapping to weapons.
    // I don't remember the specifics of why or how this works, but it works, so Don't Touch It, Kiddo.

	int old;
	bool in_switch;
    bool weapon_was_out;
	public void InitiateSwitch()
	{
		if (Active && !in_switch) weapon_was_out = true;
		if (!Active) weapon_was_out = false;

		if (in_switch) return;
		if (weaponManager.state != Position && !weapon_was_out && ! WeaponManager.InSwap) old = 
				WeaponManager.CurrentWeapon.Position;
		weaponManager.state = Position;
		in_switch = true;
		Active = true;
	}

	public void CompleteSwitch()
	{
		if (weapon_was_out)
        {
			WeaponManager.InSwap = false;
			return;
        }

		in_switch = false;
		WeaponManager.InSwap = false;
		weaponManager.state = old;
		Active = false;
	}

	public void ForceUpdate()
    {
        Update();
    }

    bool force_hiding_vm;
    public void HideViewmodel()
    {
        force_hiding_vm = true;
        if (WeaponModel) WeaponModel.SetActive(false);
    }

    public void ShowViewmodel()
    {
        force_hiding_vm = false;
        if(WeaponModel) WeaponModel.SetActive(true);
    }

    protected void Update()
    {
        Position = Mathf.Clamp(Position, -999, weaponManager.Weapons.Count);
        Active = weaponManager.state == Position && !PlayerState.GetState("ControllingObject");
        if (WeaponModel)
        {
            if (!force_hiding_vm)
            {
                WeaponModel.SetActive(Active);
            }
            else
            {
                WeaponModel.SetActive(false);
            }
        }
        if (Active)
        {
            if (WeaponAnimator) WeaponAnimator.enabled = true;
			if(!WeaponManager.InSwap && !in_switch)WeaponManager.CurrentWeapon = this;
            weaponManager.CameraFOVManager.vm_fov = ViewmodelFOV;
        }
		else
		{
			if(WeaponAnimator)WeaponAnimator.enabled = false;
		}

		if (!Active && in_switch)
		{
			in_switch = false;
		}
	}
}
