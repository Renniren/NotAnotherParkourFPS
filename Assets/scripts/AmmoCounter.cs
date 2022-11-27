using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using text = TMPro.TMP_Text;

public class AmmoCounter : MonoBehaviour
{
    public enum ACMode { Magazine, ReserveAmmo, WeaponName }
    public ACMode mode;
    public text Text;
	int track;

    void Update()
    {
        switch (mode)
        {
            case ACMode.Magazine:
                if(WeaponManager.CurrentBallisticWeapon)
                {
                    if (WeaponManager.CurrentBallisticWeapon.Active)
                    {
						if(track != WeaponManager.CurrentBallisticWeapon.CurrentAmmo)
						{
							Text.text = WeaponManager.CurrentBallisticWeapon.CurrentAmmo.ToString() + "/" + WeaponManager.CurrentBallisticWeapon.MagazineSize.ToString();
							track = WeaponManager.CurrentBallisticWeapon.CurrentAmmo;
						}
                        
                    }
                    else
                    {
						track = -1;
                        Text.text = ""; // Knives don't have ammo.
                    }
                }
                break;

            case ACMode.ReserveAmmo:
				if (WeaponManager.CurrentBallisticWeapon)
				{
					if(track != WeaponManager.CurrentBallisticWeapon.MaximumReserveAmmo)
					{
						Text.text = WeaponManager.CurrentBallisticWeapon.MaximumReserveAmmo.ToString();
						track = WeaponManager.CurrentBallisticWeapon.MaximumReserveAmmo;
					}
					
				}
                break;

            case ACMode.WeaponName:
                if(Text.text != WeaponManager.CurrentWeapon.Name)Text.text = WeaponManager.CurrentWeapon.Name;
                break;
        }
    }
}
