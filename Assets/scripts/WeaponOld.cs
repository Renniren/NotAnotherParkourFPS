using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class WeaponOld
    : MonoBehaviorExtended
{
    public enum FireMode { Semi, FullAuto, Burst }
    public enum FireType { Hitscan, Projectile }
    public enum ReloadType { Conventional, ShellByShell }
    public Animator GunAnimator;

    [Header("Weapon Settings")]
    public BulletTracerCreator tracer;
    public Vector3 Recoil;
    public Vector3 RecoilRandomness;
    public Transform Head;
    public Transform BulletOrigin;
    public Transform MuzzleflashOrigin;
    public GameObject WeaponModel;
    public FireMode Firemode = FireMode.FullAuto;
    public FireType Firetype;
    public ReloadType reloadType;
    public int Position;
    public int BulletsPerShot = 1;
    public int AmmoPerShot = 1;
    public int CurrentAmmo = 31;
    public int MagazineSize = 31;
    public int MaximumReserveAmmo = 256;
    public int Damage = 12;
    public int BulletForce = 90;
    public float TracerWidth = 0.3f;
    public float SpreadAmount = 0;
    public float EffectiveRange = 999;
    public float FireDelay = 0.1f;
    public float ReloadStartDelay;
    public float ReloadDelay = 0;
    public bool UsableWhileSprinting = true;
    public bool Reloadable;
    public bool HideMuzzleflash = true;
    public bool FireDelayRespectsTime = true;
    public bool UsesTracers;
    public string FireAnimation;
    public string DrawAnimation;
    public string SprintAnimation;
    public string SprintExitAnimation;
    public string ReloadAnimation;
    public string ReloadEnterAnimation;
    public string ReloadExitAnimation;
    public string VaultEnterAnimation;
    public string VaultExitAnimation;

    [Header("Weapon State")]
    public bool AttacksAllowed;
    public bool SafetyOn;
    public bool Active;
    public bool Reloading;
    public bool InBurst;

    private WeaponManager weaponManager;
    private float fire, reload, reloadstart;
    
    void Start()
    {
        fire = FireDelay;
        weaponManager = GetComponentInParent<WeaponManager>();
        //weaponManager.Weapons.Add(this);
    }

    void Fire() 
    {
        fire = 0;
        Vector3 newrec = Recoil;
        newrec.x += Random.Range(-RecoilRandomness.x, RecoilRandomness.x);
        newrec.y += Random.Range(-RecoilRandomness.y, RecoilRandomness.y);
        newrec.z += Random.Range(-RecoilRandomness.z, RecoilRandomness.z);
        weaponManager.recoil.AddRecoil(newrec);
        GunAnimator.ForcePlay(FireAnimation);
        LayerMask mask = LayerMask.GetMask("Default");

        for (int i = 0; i < BulletsPerShot; i++)
        {
            switch (Firetype)
            {
                case FireType.Hitscan:
                    Quaternion offset = Random.rotation;
                    Quaternion neutral = Head.rotation;
                   

                    RaycastHit hit;
                    Ray ray = new Ray(Head.position, 
                        Head.InverseTransformDirection
                        (Quaternion.RotateTowards(neutral, offset, SpreadAmount) * Head.forward));

                    Physics.Raycast(ray, out hit, EffectiveRange, mask);
                    (Vector3, Vector3) trace = (MuzzleflashOrigin.position, hit.point);
                    if (hit.collider)
                    {
                        Rigidbody body = hit.collider.gameObject.GetRigidbody();
                        Entity hitEntity = hit.collider.gameObject.GetEntity();

                        if (body)
                        {
                            body.AddForce((body.transform.position - transform.position) * BulletForce * body.mass);
                        }

                        if (hitEntity)
                        {
                            Entity.Hurt(Player, hitEntity, Damage, false, false);
                            if (hitEntity.CanBleed)
                            {

                            }
                            else
                            {
                                Spawn("BulletSparks", hit.point);
                            }
                        }
                        else
                        {
                            Spawn("BulletSparks", hit.point);
                        }


                    }

                    if (!hit.collider)
                    {
                        trace.Item2 = ray.GetPoint(EffectiveRange);
                    }

                    tracer.MakeTracer(trace.Item1, trace.Item2, TracerWidth);
                    break;

                case FireType.Projectile:

                    break;
            }
        }

        
        CurrentAmmo -= AmmoPerShot;
    }

    bool vaultCheck;
    bool SprintingNormally;
    bool sprintCheck;

    

    void Update()
    {
        SprintingNormally = (PlayerState.GetState("IsSprinting")) && !UsableWhileSprinting;
        Active = weaponManager.state == Position;
        AttacksAllowed =
            !SafetyOn &&
            Active &&
            CurrentAmmo > 0 &&
            fire >= FireDelay &&
            !Game.Paused &&
            !Reloading &&
            !SprintingNormally;
        
        WeaponModel.SetActive(Active);
        

        if (Active)
        {
            if(FireDelayRespectsTime)fire += Time.deltaTime;
            if(!FireDelayRespectsTime)fire += Time.unscaledDeltaTime;

            if (sprintCheck != SprintingNormally && !PlayerState.GetState("IsVaulting"))
            {
                sprintCheck = SprintingNormally;
                if (!sprintCheck)
                {
                    GunAnimator.Play(SprintExitAnimation);
                }
                else
                {
                    GunAnimator.Play(SprintAnimation);
                }
            }

            if (vaultCheck != PlayerState.GetState("IsVaulting"))
            {
                vaultCheck = PlayerState.GetState("IsVaulting");
                if (vaultCheck)
                {
                    GunAnimator.Play(VaultEnterAnimation);
                }
                else
                {
                    GunAnimator.Play(VaultExitAnimation);
                }
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                if (Reloadable && CurrentAmmo < MagazineSize)
                {
                    Reloading = true;
                    switch(reloadType)
                    {
                        case ReloadType.Conventional:
                            GunAnimator.ForcePlay(ReloadAnimation);
                            break;

                        case ReloadType.ShellByShell:
                            GunAnimator.ForcePlay(ReloadEnterAnimation);
                            break;
                    }
                }
            }

            if (Reloading)
            {
                switch (reloadType)
                {
                    case ReloadType.Conventional:
                        reload += Time.deltaTime;
                        if(reload >= ReloadDelay)
                        {
                            CurrentAmmo = MagazineSize;
                            reload = 0;
                            Reloading = false;
                        }

                        break;

                    case ReloadType.ShellByShell:
                        reload += Time.deltaTime;
                        reloadstart += Time.deltaTime;
                        if(reloadstart >= ReloadStartDelay)
                        {
                            if(reload >= ReloadDelay)
                            {
                                CurrentAmmo++;
                                GunAnimator.ForcePlay(ReloadAnimation);
                                reload = 0;
                            }

                            if(CurrentAmmo >= MagazineSize)
                            {
                                reloadstart = 0;
                                GunAnimator.ForcePlay(ReloadExitAnimation);
                                Reloading = false;
                            }
                        }

                        break;
                }
            }

            switch (Firemode)
            {
                case FireMode.Semi:
                    if(Input.GetKeyDown(KeyCode.Mouse0) && AttacksAllowed)
                    {
                        Fire();
                    }

                    break;

                case FireMode.FullAuto:
                    if (Input.GetKey(KeyCode.Mouse0) && AttacksAllowed)
                    {
                        Fire();
                    }

                    break;

                case FireMode.Burst:

                    break;
            }
        }
    }
}
