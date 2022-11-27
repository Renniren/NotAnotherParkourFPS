using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using UnityEngine.UI;
using UnityEngine.Events;


//Generic class for all relevent player statistic displays.
//Lifted from Insurrection.
public class InfoBar : MonoBehaviorExtended
{
    public enum UIBarAttribute { Health, Stamina, Shield, Ammo, HardDamage, Ultimate }
    public float LerpSpeed = 12;
    public UIBarAttribute Tracks;
    public Image bar;
    public UnityEvent OnTrackedValueChanged, OnTrackedValueIncreased;
    float fval, target;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch (Tracks)
        {
            case UIBarAttribute.Health:
                target = PercentOf(PlayerEntity.Health, 100);
                if (fval != target)
                {
                    OnTrackedValueChanged?.Invoke();
					if (target > fval) OnTrackedValueIncreased?.Invoke();
                    fval = target;
                }

                bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, LerpSpeed * Time.deltaTime);
                
                break;

            /*case UIBarAttribute.HardDamage:
                Player.TryGetComponent(out PlayerHardDamageManager hd);
                target = PercentOf(hd.HardDamage, 100);
                if (fval != target)
                {
                    OnTrackedValueChanged?.Invoke();
                    fval = target;
                }

                bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, LerpSpeed * Time.deltaTime);
                break;*/

            /*			case UIBarAttribute.Ultimate:
                            Player.TryGetComponent(out PlayerUltimateAbility ult);
                            target = PercentOf(ult.ChargeProgress, 100);
                            bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, LerpSpeed * Time.deltaTime);
                            break;*/

            case UIBarAttribute.Ammo:
                if (WeaponManager.CurrentBallisticWeapon)
                {
                    target = PercentOf(WeaponManager.CurrentBallisticWeapon.CurrentAmmo, WeaponManager.CurrentBallisticWeapon.MagazineSize);
                    if (fval != target)
                    {
                        OnTrackedValueChanged?.Invoke();
                        fval = target;
                    }

                    bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, LerpSpeed * Time.deltaTime);
                }
                break;

            /*case UIBarAttribute.Stamina:
                Player.TryGetComponent(out PlayerStamina ps);
                target = PercentOf(PlayerStamina.Stamina, ps.MaximumStamina);
                if (fval != target)
                {
                    OnTrackedValueChanged?.Invoke();
                    fval = target;
                }

                bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, LerpSpeed * Time.deltaTime);
                break;*/

            case UIBarAttribute.Shield:
                target = PercentOf(PlayerEntity.Shields, 100);
                if (fval != target)
                {
                    OnTrackedValueChanged?.Invoke();
                    fval = target;
                }

                bar.fillAmount = Mathf.Lerp(bar.fillAmount, target, LerpSpeed * Time.deltaTime);
                break;
        }
    }
}
