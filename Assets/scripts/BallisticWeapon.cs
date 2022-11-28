using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class BallisticWeapon : BaseWeapon
{
	public enum FireMode { Semi, FullAuto, Burst }
	public enum bulletType { Hitscan, Projectile }
	public enum ReloadType { Conventional, ShellByShell }
	public enum ReloadMode { TimerBased, AnimationEvents }

	[Header("Weapon Settings")]
	public Vector3 Recoil;
	public Vector3 MeleeForceDirection;
	public Vector3 RecoilRandomness;
	public Transform Head;
	public Transform BulletOrigin;
	public Transform MuzzleflashOrigin;
	[Header("Behavior")]
	public bulletType BulletType;
	public ReloadType reloadType;
	public FireMode Firemode = FireMode.FullAuto;
	public bool MeleeAllowed;
	public bool MeleeStuns;
	public bool MeleePushesEnemies;
	public bool TriggersExplosions;
	public bool ReloadOnEmpty = true;
	public bool BufferReloads = true;
	public bool PiercesArmor, IgnoresShields;
	public bool Suppressed = true;
	public float MeleeForceMultiplier = 1;
	public float Loudness = 50;
	public float SpreadAmount = 0;
	public float FireDelay = 0.1f;
	public float EquipTime = 0.3f;
	public float EmptyReloadDelay = 0.6f;
	public float MeleeDelay = 0.05f, MeleeLength = 0.6f;
	public float MeleeRange = 4;
	public int MeleeDamage = 14;
	public int BulletsPerShot = 1;
	public int AmmoPerShot = 1;
	public int CurrentAmmo = 31;
	public int MagazineSize = 31;
	public int MaximumReserveAmmo = 256;
	public int ArmorPiercingAmount = 0;
	public int Damage = 12;
	public float BulletForce = 90;
	[Header("Physical Bullet Settings")]
	public string BulletPrefab;
	public float BulletMass = 10;
	public float BulletSpeed = 12;
	public float BulletGravityInfluence = 1;
	public float BulletUpwardsVelocity = 5;
	public bool BulletRespectsGravity;
	[Header("Effects")]
	public AudioClip MeleeSound;
	public AudioClip FireSound;
	public float MeleeShakeAmount = 0.2f;
	public float MeleeRotationShakeAmount = 0.2f;
	public float MeleeSoundVolume = 0.8f;
	public float FireSoundVolume = 0.9f;
	public float PositionalShakeAmount = 0.4f;
	public float RotationalShakeAmount = 0.4f;
	public float TracerWidth = 0.3f;
	public float TracerFadeSpeed = 12;
	public float TracerFollowSpeed = 64;
	public bool UsesTracers;
	public bool TracerStartFollowsEnd = true;
	public Material TracerMaterial;
	[Header("Reloading")]
	public ReloadMode reloadMode;
	public float ReloadStartDelay;
	public float ReloadDelay = 0;
	public bool Reloadable = true;
	[Header("General")]
	public float EffectiveRange = 999;
	public bool HideMuzzleflash = true;
	public bool FireDelayRespectsTime = true;
	public bool UsableWhileSprinting = true;
	[Header("Animations")]
	public string FireAnimation;
	public string DrawAnimation;
	public string SprintAnimation;
	public string MeleeAnimation;
	public string SprintExitAnimation;
	public string ReloadAnimation;
	public string ReloadEnterAnimation;
	public string ReloadExitAnimation;
	public string VaultEnterAnimation;
	public string VaultExitAnimation;

	[Header("Weapon State")]
	public bool AttackBuffered;
	public bool AttacksAllowed;
	public bool SafetyOn;
	public bool InMelee;
	public bool Reloading;
	public bool InBurst;

	
	private float fire, reload, reloadstart, equip, emptyreload;
	
	void Start()
	{
		base.Start();
		fire = FireDelay;
		if (!Head)
		{
			Head = Camera.main.transform;
		}
	}
	
	public void ReloadWeapon()
	{
		CurrentAmmo = MagazineSize;
		reload = 0;
		Reloading = false;
		reloadBuffered = false;
		restartedReload = false;
		startedEmptyReload = false;
	}

	public void ReloadOnce()
    {
		CurrentAmmo++;
		WeaponAnimator.ForcePlay(ReloadAnimation);
		reload = 0;
		if (CurrentAmmo >= MagazineSize)
		{
			WeaponAnimator.ForcePlay(ReloadExitAnimation);
			reload = 0;
			reloadstart = 0;
			Reloading = false;
			reloadBuffered = false;
			restartedReload = false;
			startedEmptyReload = false;
		}
	}

	public void StartReload()
	{
		if (Reloading) return;
		Reloading = true;
		switch (reloadType)
		{
			case ReloadType.Conventional:
				WeaponAnimator.ForcePlay(ReloadAnimation);
				break;

			case ReloadType.ShellByShell:
				WeaponAnimator.ForcePlay(ReloadEnterAnimation);
				break;
		}
	}

	float meltime, melhurttime;
	void Melee()
	{
		if (!AttacksAllowed || !MeleeAllowed || InMelee) return;
		WeaponAnimator.ForcePlay(MeleeAnimation);
		InMelee = true;
		melhurttime = 0;
		meltime = 0;
	}

	public void Fire() 
	{
		fire = 0;
		//if(!Suppressed)NPC.MakeNoise(transform.position, Loudness);
		Vector3 newrec = new Vector3(-Recoil.x, Recoil.y, Recoil.z);
		newrec.x += Random.Range(-RecoilRandomness.x, RecoilRandomness.x);
		newrec.y += Random.Range(-RecoilRandomness.y, RecoilRandomness.y);
		newrec.z += Random.Range(-RecoilRandomness.z, RecoilRandomness.z);
		weaponManager.recoil.AddRecoil(newrec);
		WeaponAnimator.ForcePlay(FireAnimation);
		LayerMask mask = LayerMask.GetMask("Default", "Enemy", "EnemyBullets", "PlayerBullets", "PlayerHelperHitboxes", "Ragdolls");
		weaponManager.CameraShake.DoShake(PositionalShakeAmount);
		weaponManager.CameraShake.DoRotationalShake(RotationalShakeAmount);

		for (int i = 0; i < BulletsPerShot; i++)
		{
			switch (BulletType)
			{
				case bulletType.Hitscan:
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
						EnemyController cont = hit.collider.gameObject.GetEnemyController();
						Rigidbody body = hit.collider.gameObject.GetRigidbody();
						Entity hitEntity = hit.collider.gameObject.GetEntity();

						if (cont)
						{
							cont.AddForce(Head.forward * (BulletForce), AddForceMode.Set);
						}

						if (TriggersExplosions)
						{
							if (hit.collider.gameObject.GetHelperHitbox())
							{
								hit.collider.gameObject.GetHelperHitbox().Parent.TryGetComponent(out ExplosiveProjectile exp);
								if (exp)
								{
									if (exp.AllowMidairDetonation)
									{
										Explosion old = new Explosion();
										old.Copy(exp.exp);
										exp.exp.Damage *= 2;
										exp.exp.Force *= 0.5f;
										exp.Explode();
										exp.exp = old;
										TimeManager.Instance.DramaticHit(0.32f);
										PlaySound("crit", transform.position, true, 1);
										Spawn("EnemyWeaponReady", hit.point);
										CreateDebrisSpray(hit.collider.gameObject.GetHelperHitbox().Parent.transform.position, 40, 5);
									}
								}
							}
							else
							{
								hit.collider.gameObject.transform.TryGetComponent(out ExplosiveProjectile exp);

								if (exp)
								{
									if (exp.AllowMidairDetonation)
									{
										Explosion old = new Explosion();
										old.Copy(exp.exp);
										exp.exp.Damage *= 2;
										exp.exp.Force *= 0.5f;
										exp.Explode();
										exp.exp = old;
										TimeManager.Instance.DramaticHit(0.32f);
										PlaySound("crit", transform.position, true, 0.95f);
										Spawn("EnemyWeaponReady", hit.point);
										CreateDebrisSpray(hit.collider.gameObject.transform.position, 40, 5);
									}
								}
							}
						}

						if (body)
						{
							body.AddForce((body.transform.position - transform.position) * BulletForce * body.mass);
						}

						if (hitEntity)
						{
							Entity.Hurt(Player, hit.collider.gameObject, Damage + ArmorPiercingAmount, PiercesArmor, IgnoresShields);
							if (!hitEntity.CanBleed)
							{
								Spawn("BulletSparks", hit.point);
							}

							TryMakeHitEffects(hitEntity, Damage, hit.point);
						}
						else
						{
							Entity.Hurt(Player, hit.collider.gameObject, Damage + ArmorPiercingAmount, PiercesArmor, IgnoresShields);
							Spawn("BulletSparks", hit.point);
						}
					}

					if (!hit.collider)
					{
						trace.Item2 = ray.GetPoint(EffectiveRange);
					}

					if(UsesTracers)MakeTracer(trace.Item1, trace.Item2, TracerWidth, TracerFadeSpeed, TracerStartFollowsEnd, TracerFollowSpeed, TracerMaterial);
					break;

				case bulletType.Projectile:
					GameObject bullet = Spawn(BulletPrefab, BulletOrigin.position, BulletOrigin.rotation);
					bullet.transform.forward = Head.forward;
					bullet.transform.rotation = Quaternion.RotateTowards(bullet.transform.rotation, Random.rotation, SpreadAmount);
					bullet.TryGetComponent(out Bullet bul);
					bul.PiercesArmor = PiercesArmor;
					bul.IgnoresShields = IgnoresShields;
					bul.ArmorPenetration = ArmorPiercingAmount;
					bul.OnlyHasInitialVelocity = BulletRespectsGravity;
					bul.GravityInfluence = BulletGravityInfluence;
					bul.speed = BulletSpeed;
					bul.body.velocity = (bul.transform.forward * BulletSpeed) + (bul.transform.up * BulletUpwardsVelocity);
					bul.Damage = Damage;
					break;
			}
		}

		PlaySound(FireSound, transform.position, true, FireSoundVolume);
		CurrentAmmo -= AmmoPerShot;
		Events.InvokeWeaponFired(this);
	}

	bool reloadBuffered;
	bool restartedReload;
	bool playedEquip;
	bool startedEmptyReload;
	bool vaultCheck;
	bool didMelee;
	bool SprintingNormally;
	bool sprintCheck;

	private void OnDrawGizmos()
	{
		Vector3 melee = (Head.forward + MeleeForceDirection).normalized * MeleeForceMultiplier;
		Gizmos.DrawRay(Head.position, melee);
	}


	void Update()
	{
		SprintingNormally = (PlayerState.GetState("IsSprinting")) && !UsableWhileSprinting;
		base.Update();
		AttacksAllowed =
			!SafetyOn &&
			Active &&
			!InMelee &&
			CurrentAmmo > 0 &&
			fire >= FireDelay &&
			!Game.Paused &&
			equip >= EquipTime &&
			!WeaponManager.WeaponsLocked &&
			!Reloading &&
			!SafetyOn &&
			Active &&
			CurrentAmmo > 0 &&
			fire >= FireDelay &&
			!Game.Paused &&
			!Reloading &&
			!SprintingNormally;
		//If it was written 2 years ago, looks ugly, and it still works, don't touch it.


		CurrentAmmo = Mathf.Clamp(CurrentAmmo, 0, MagazineSize);

		if (Active)
		{
			WeaponManager.CurrentBallisticWeapon = this;
			if(FireDelayRespectsTime)fire += Time.deltaTime;
			if(!FireDelayRespectsTime)fire += Time.unscaledDeltaTime;

			if (!playedEquip)
			{
				if (CurrentAmmo <= 0 && BufferReloads) reloadBuffered = true;
				WeaponAnimator.ForcePlay(DrawAnimation);
				playedEquip = true;
			}

			if (sprintCheck != SprintingNormally && !PlayerState.GetState("IsVaulting"))
			{
				sprintCheck = SprintingNormally;
				if (!sprintCheck)
				{
					WeaponAnimator.Play(SprintExitAnimation);
				}
				else
				{
					WeaponAnimator.Play(SprintAnimation);
				}
			}

			if (vaultCheck != PlayerState.GetState("IsVaulting"))
			{
				vaultCheck = PlayerState.GetState("IsVaulting");
				if (vaultCheck)
				{
					WeaponAnimator.Play(VaultEnterAnimation);
				}
				else
				{
					WeaponAnimator.Play(VaultExitAnimation);
				}
			}

			equip += Time.deltaTime;
			if (equip < EquipTime && Input.GetKeyDown(KeyCode.Mouse0))
			{
				AttackBuffered = true;
			}

			if (equip >= EquipTime)
			{
				if ((Reloading || reloadBuffered) && !restartedReload)
				{
					WeaponAnimator.ForcePlay(ReloadAnimation);
					restartedReload = true;
				}
			}

			if (CurrentAmmo <= 0)
			{
				if (ReloadOnEmpty)
				{
					if(!startedEmptyReload)emptyreload += Time.deltaTime;
					if (emptyreload >= EmptyReloadDelay && !startedEmptyReload)
					{
						StartReload();
						emptyreload = 0;
						startedEmptyReload = true;
					}
				}
			}

			

			if (vaultCheck != PlayerState.GetState("IsVaulting"))
			{
				vaultCheck = PlayerState.GetState("IsVaulting");
				if (vaultCheck)
				{
					WeaponAnimator.Play(VaultEnterAnimation);
				}
				else
				{
					WeaponAnimator.Play(VaultExitAnimation);
				}
			}

			if (Input.GetKeyDown(KeyCode.R))
			{
				restartedReload = true;
				if (Reloadable && CurrentAmmo < MagazineSize && !Reloading)
				{
					StartReload();
				}
			}

			if (Input.GetKeyDown(KeyCode.V))
			{
				Melee();
			}

			if (InMelee)
			{
				meltime += Time.deltaTime;

				if (!didMelee)
				{
					melhurttime += Time.deltaTime;
					if (melhurttime >= MeleeDelay)
					{
						int mask = LayerMask.GetMask("Default", "Enemy", "EnemyBullets", "PlayerBullets", "PlayerHelperHitboxes");
						RaycastHit hit;
						Physics.Raycast(Head.position, Head.forward, out hit, MeleeRange, mask);
						if (hit.collider)
						{
							Entity hurt = hit.collider.gameObject.GetEntity();
							if (hurt)
							{
								Entity.Hurt(Player, hurt, MeleeDamage, false, false);
								PlaySound(MeleeSound, transform.position, true, MeleeSoundVolume);
								CameraShake.DoRotationalShake(MeleeRotationShakeAmount);
								CameraShake.DoShake(MeleeShakeAmount);
								if (MeleeStuns)
								{
									Enemy e = hurt.gameObject.GetComponent<Enemy>();
									if(e)e.Stun(3);
								}

								if (MeleePushesEnemies)
								{
									EnemyController cont = hurt.gameObject.GetEnemyController();
									if (cont)
									{
										Vector3 melee = (Player.transform.forward + MeleeForceDirection) * MeleeForceMultiplier;
										cont.AddForce(melee, AddForceMode.Set);
									}
								}
							}
						}

						didMelee = true;
					}
				}

				if (meltime >= MeleeLength)
				{
					InMelee = false;
					meltime = 0;
					melhurttime = 0;
					didMelee = false;
				}
			}

			if (Reloading)
			{
				switch (reloadType)
				{
					case ReloadType.Conventional:
						if (reloadMode == ReloadMode.TimerBased)
						{
							reload += Time.deltaTime;
							if(reload >= ReloadDelay)
							{
								CurrentAmmo = MagazineSize;
								reload = 0;
								Reloading = false;
							}
						}

						break;

					case ReloadType.ShellByShell:
						if(reloadMode == ReloadMode.TimerBased)
						{
							reload += Time.deltaTime;
							reloadstart += Time.deltaTime;
							if(reloadstart >= ReloadStartDelay)
							{
								if(reload >= ReloadDelay)
								{
									CurrentAmmo++;
									WeaponAnimator.ForcePlay(ReloadAnimation);
									reload = 0;
								}

								if(CurrentAmmo >= MagazineSize)
								{
									reloadstart = 0;
									WeaponAnimator.ForcePlay(ReloadExitAnimation);
									Reloading = false;
								}
							}
						}

						break;
				}
			}

			switch (Firemode)
			{
				case FireMode.Semi:
					if(Input.GetKeyDown(KeyCode.Mouse0) && AttacksAllowed && !reloadBuffered && !AttackBuffered)
					{
						Fire();
					}

					break;

				case FireMode.FullAuto:
					if (Input.GetKey(KeyCode.Mouse0) && AttacksAllowed && !reloadBuffered && !AttackBuffered)
					{
						Fire();
					}

					break;

				case FireMode.Burst:

					break;
			}

			if (AttacksAllowed && AttackBuffered)
			{
				Fire();
				AttackBuffered = false;
			}
		}
		else
		{
			equip = 0;
			playedEquip = false;
			AttackBuffered = false;
			restartedReload = false;
		}
	}
}
