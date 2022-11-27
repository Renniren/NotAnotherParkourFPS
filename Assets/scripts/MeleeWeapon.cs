using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ParkourFPS;


/// <summary>
/// Class for Melee Weapons.
/// </summary>
public class MeleeWeapon : BaseWeapon
{
	[Space]
	public MeleeWeaponType WeaponType;
	public MeleeWeaponMaterial weaponMaterial;
	public GameObject ThrownWeapon;

	public float ThrowForce = 20;
	public float MeleeForce = 0, MeleeEnemyForce = 1;
	public float Range = 5.0f;
	public int Damage = 10;
	public int ArmorPenetration = 5;
	[Space]
	public float AttackHitDelay = 0;
	public float AttackDelay = 0.5f;
	public float EquipDelay = 0.3f;
	[Space]
	public bool BufferAttacks = true;
	public bool UsableWhileSprinting = true;
	public bool Throwable = true;

	[Header("Effects")]
	public float PositionalShakeAmount = 0.4f;
	public float RotationalShakeAmount = 0.4f;

	// This seems (and feels) super convoluted, but this is so that we can have a universal definition of what melee animation names 
	// should be, especially in the event of procedurally generated/vasts amounts of melee weapons.

	public List<MeleeAnimation> AttackAnimations;
	public MeleeAnimation SprintAnimation, SprintExitAnimation, EquipAnimation;

	[Header("State")]
	public bool SafeMode;
	public bool CanAttack;

	bool buffered;
	bool SprintingNormally;
	bool sprintCheck;

	float attack, draw;
	Transform head;

	string FistsLeftSwing;
	string FistsRightSwing;

	Timer attackHitDelay;


	void Start()
	{
		base.Start();
		attack = AttackDelay + 1;
		draw = EquipDelay + 1;
		head = weaponManager.head;
		attackHitDelay = new Timer(AttackHitDelay, true);
		Invoke(nameof(SetupAnimations), 0.001f);
	}

	void SetupAnimations()
    {
		GetAttackAnimations(AnimationContext.Swing, this.WeaponType);
		switch (WeaponType)
		{
			case MeleeWeaponType.OneHandedBlade:
				WeaponAnimator.Play("onehandedsharp");
				break;

			case MeleeWeaponType.OneHandedBlunt:
				WeaponAnimator.Play("onehanded");
				break;

			case MeleeWeaponType.TwoHandedBlade:
				WeaponAnimator.Play("twohandedsharp");
				break;

			case MeleeWeaponType.TwoHandedBlunt:
				WeaponAnimator.Play("twohanded");
				break;

			case MeleeWeaponType.Throwing:
				WeaponAnimator.Play("throwing");
				break;

			case MeleeWeaponType.Knife:
				WeaponAnimator.Play("knife");
				break;

			case MeleeWeaponType.Fists:
				WeaponAnimator.Play("fists");
				break;

			case MeleeWeaponType.None:

				break;
		}
	}

	public void Hit()
	{
		LayerMask mask = LayerMask.GetMask("Default", "Enemy", "EnemyBullets", "PlayerBullets", "PlayerHelperHitboxes", "Ragdolls");
		RaycastHit hit;
		Physics.Raycast(head.position, head.forward, out hit, Range, mask);

		if (hit.collider)
		{
			if (WouldKill(Damage + ArmorPenetration, hit.collider.gameObject))
            {
				TimeManager.Instance.DramaticHit(0.08f);
			}
			else
            {
				TimeManager.Instance.DramaticHit(0.031f);
			}

			Entity.Hurt(Player, hit.collider.gameObject, Damage + ArmorPenetration, false, false);
			PlaySound("GenericHit" + random(1, 4), transform.position, true);
			
			
			Rigidbody body = hit.collider.gameObject.GetRigidbody();
			EnemyController cont = hit.collider.gameObject.GetEnemyController();
			if (cont)
			{
				cont.AddForce(head.forward * (MeleeForce), AddForceMode.Set);
			}

			if (body && !cont) body.velocity = (body.transform.position - head.position) * MeleeForce;
			
			CameraShake.DoShake(PositionalShakeAmount);
			CameraShake.DoRotationalShake(RotationalShakeAmount);
		}
	}

	public void Throw()
	{
		if (!Throwable) return;
		GameObject thrownWeapon = Instantiate(ThrownWeapon, head.position, head.rotation);
		WeaponPickup pickup;
		Rigidbody body;

		thrownWeapon.layer = 16;
		thrownWeapon.transform.forward = head.forward;

		pickup = thrownWeapon.GetComponent<WeaponPickup>();
		pickup.pickupActive = false;
		pickup.lethal = true;

		body = thrownWeapon.GetRigidbody();
		body.velocity = (head.forward * ThrowForce) + (Vector3.up * 3);

		GameManager.ClearWeapon(gameObject, true);

		weaponManager.state = 1;
		weaponManager.Weapons[0].WeaponModel.SetActive(true);
		weaponManager.Weapons[0].WeaponAnimator.ForcePlay("throw");
		weaponManager.cameraWobble.AddFallImpulse(new Vector3(0, -24, 2), 4.5f);
		CameraShake.DoRotationalShake(RotationalShakeAmount);
		PlaySound("whoosh", transform.position, true);
	}

	//Gets all of the weapon's necessary animations.
	void GetAttackAnimations(AnimationContext ctx, MeleeWeaponType weaponType)
	{
		List<MeleeAnimation> anims = new List<MeleeAnimation>();

		if (WeaponType != MeleeWeaponType.Fists)
        {
			for (int i = 0; i < GameManager.MeleeAnimations.Length; i++)
			{
				if (GameManager.MeleeAnimations[i].context == ctx && GameManager.MeleeAnimations[i].weaponType == weaponType)
				{
					anims.AddOnce(GameManager.MeleeAnimations[i]);
				}

				if (GameManager.MeleeAnimations[i].weaponType == weaponType)
                {
					if (GameManager.MeleeAnimations[i].context == AnimationContext.SprintEnter)
                    {
						SprintAnimation = GameManager.MeleeAnimations[i];
					}

					if (GameManager.MeleeAnimations[i].context == AnimationContext.SprintExit)
                    {
						SprintExitAnimation = GameManager.MeleeAnimations[i];
					}

					if (GameManager.MeleeAnimations[i].context == AnimationContext.Equip)
                    {
						EquipAnimation = GameManager.MeleeAnimations[i];
					}
                }
			}
        }
		if (WeaponType == MeleeWeaponType.Fists)
		{
			for (int i = 0; i < GameManager.MeleeAnimations.Length; i++)
			{
				if (GameManager.MeleeAnimations[i].context == AnimationContext.Punch && 
					GameManager.MeleeAnimations[i].weaponType == weaponType)
				{
					anims.AddOnce(GameManager.MeleeAnimations[i]);
				}

				if (GameManager.MeleeAnimations[i].weaponType == weaponType)
				{
					if (GameManager.MeleeAnimations[i].context == AnimationContext.SprintEnter)
					{
						SprintAnimation = GameManager.MeleeAnimations[i];
					}

					if (GameManager.MeleeAnimations[i].context == AnimationContext.SprintExit)
					{
						SprintExitAnimation = GameManager.MeleeAnimations[i];
					}

					if (GameManager.MeleeAnimations[i].context == AnimationContext.Equip)
					{
						EquipAnimation = GameManager.MeleeAnimations[i];
					}
				}
			}
		}

		AttackAnimations = anims;
		
		/*EquipAnimation = GameManager.MeleeAnimations.First<MeleeAnimation>(ma => ma.context == AnimationContext.Equip 
			&& ma.weaponType == weaponType);
		
		SprintAnimation = GameManager.MeleeAnimations.First<MeleeAnimation>(ma => ma.context == AnimationContext.SprintEnter 
			&& ma.weaponType == weaponType);
		
		SprintExitAnimation = GameManager.MeleeAnimations.First<MeleeAnimation>(ma => ma.context == AnimationContext.SprintExit 
			&& ma.weaponType == weaponType);
		I couldn't use Linq because it'd block parts of the function from running, should the weapon not have an animation for a certain action.
*/ 
/*
		if (ret.context == AnimationContext.None) Debug.LogWarning("No Melee Animation for the context: " + ctx);
		if (ret.weaponType == MeleeWeaponType.None) Debug.LogWarning("No Melee Animation for the weapont type: " + weaponType);
*/
		/*MeleeAnimation ret = GameManager.MeleeAnimations.First<MeleeAnimation>(ma => ma.context == ctx && ma.weaponType == weaponType);
		if (ret.context == AnimationContext.None) Debug.LogWarning("No Melee Animation for the context: " + ctx);
		if (ret.weaponType == MeleeWeaponType.None) Debug.LogWarning("No Melee Animation for the weapont type: " + weaponType);*/
	}

	bool playedEquip;
	bool InAttack;

	void Update()
	{
		base.Update();

		SprintingNormally = (PlayerState.GetState("IsSprinting")) && !UsableWhileSprinting;

		CanAttack =
			!Game.Paused &&
			!SafeMode &&
			!SprintingNormally &&
			attack >= AttackDelay &&
			!InAttack &&
			draw >= EquipDelay;

		if (Active)
		{
			attack += Time.deltaTime;
			draw += Time.deltaTime;

			switch (WeaponType)
			{
				case MeleeWeaponType.OneHandedBlade:
					WeaponAnimator.Play("onehandedsharp");
					break;

				case MeleeWeaponType.OneHandedBlunt:
					WeaponAnimator.Play("onehanded");
					break;

				case MeleeWeaponType.TwoHandedBlade:
					WeaponAnimator.Play("twohandedsharp");
					break;

				case MeleeWeaponType.TwoHandedBlunt:
					WeaponAnimator.Play("twohanded");
					break;

				case MeleeWeaponType.Throwing:
					WeaponAnimator.Play("throwing");
					break;

				case MeleeWeaponType.Knife:
					WeaponAnimator.Play("knife");
					break;

				case MeleeWeaponType.Fists:
					WeaponAnimator.Play("fists");
					break;

				case MeleeWeaponType.None:

					break;
			}

			if (InAttack)
            {
				attackHitDelay.Update();
				if (attackHitDelay.IsComplete())
                {
					Hit();
					attackHitDelay.Stop();
					attackHitDelay.Reset();
					InAttack = false;
				}
            }

			if (Input.GetKeyDown(KeyCode.Mouse0) && CanAttack)
            {
				if (!InAttack)
                {
					attack = 0;
					attackHitDelay.Reset();
					attackHitDelay.Start();
					string anim = AttackAnimations.GetRandomElement().Name;
					WeaponAnimator.ForcePlay(anim);

					// This is to determine which direction the camera should be nudged by the weapon.
					// This solution is questionable, but it'll do for now.
					Vector3 left = new Vector3(3, -8, 1), right = new Vector3(4, 6, 1);
					float scale = 1.8f;
					left *= scale;
					right *= scale;
					if (anim[anim.Length - 1] == '1') 
                    {
						weaponManager.cameraWobble.AddFallImpulse(left, 1);
                    }

					if (anim[anim.Length - 1] == '2')
                    {
						weaponManager.cameraWobble.AddFallImpulse(right, 4.5f);
					}

					PlaySound("whoosh", transform.position, true);

					InAttack = true;
                }
			}

			if (Input.GetKeyDown(KeyCode.Mouse1) && Throwable && CanAttack)
            {
				Throw();
			}



			if (WeaponType != MeleeWeaponType.Fists)
            {
				if (draw >= 0.02)
                {
					if (!playedEquip)
					{
						WeaponAnimator.ForcePlay(EquipAnimation.Name);
						playedEquip = true;
					}
                }
            }

			if (sprintCheck != SprintingNormally && !PlayerState.GetState("IsVaulting"))
			{
				sprintCheck = SprintingNormally;
				if (!sprintCheck)
				{
					WeaponAnimator.ForcePlay(SprintExitAnimation.Name);
				}
				else
				{
					WeaponAnimator.ForcePlay(SprintAnimation.Name);
				}
			}
		}
		else
        {
			if (playedEquip)
            {
				playedEquip = false;
				attackHitDelay.Stop();
				attackHitDelay.Reset();
			}
        }
	}
}
