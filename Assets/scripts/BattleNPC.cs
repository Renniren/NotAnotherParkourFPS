using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;
using ParkourFPS;

public enum BattleType { Conquest, TeamDeathmatch, Synthetik }
public enum soldierRole { Gunner, Mortarman, Rocketeer }

[System.Serializable]
public struct AIWeapon
{
    public string GunName;
    public string Projectile;
    public int Damage;
    public int MagazineSize;
    public int CurrentAmmo;
    public float BulletSpeed;
    public float ReloadTime;
    public float Spread;
}


public class BattleNPC : Enemy
{
	[Space]
	public Enemy enemy;
	public MeleeManager Melee;
    public EnemyController cont;
    public Transform BulletOrigin;
    public bool CanFireWhileMoving = true;
    public float AttackDistance = 200;
    public float SightDistance = 50;
    public float FireInterval = 0.04f;
    public soldierRole SoldierRole;
    public static List<BattleNPC> Combatants = new List<BattleNPC>();

    public Entity Target;

    private float CurrentFireInterval;
    public AIWeapon weapon;
    float reload;
    

    new void Start()
    {
        base.Start();
		weapon.CurrentAmmo = weapon.MagazineSize;
        weapon.ReloadTime += random(0, 0.4f);
		FireInterval += random(0, 0.07f);
        CurrentFireInterval = FireInterval * 0.99f;
        ControllerAgent.SetDestination(RandomPos);
    }

    void Fire()
    {
        BulletOrigin.LookAt(Target.transform.position);
        PlaySound("GenericGunFire", transform.position, false, 0.29f, 30, 5);
        GameObject b = Spawn("EnemyBullet", BulletOrigin.position, BulletOrigin.rotation);
        b.TryGetComponent(out bullet);
        b.transform.rotation = Quaternion.RotateTowards(b.transform.rotation, Random.rotation, weapon.Spread);
        bullet.Damage = weapon.Damage;
        bullet.owner = gameObject;
        bullet.speed = weapon.BulletSpeed;
        CurrentFireInterval = 0;
        weapon.CurrentAmmo--;
        if (weapon.CurrentAmmo <= 0) anti = false;
    }

    bool TargetAcquired;
    bool TargetValid;
    bool can_move;

    Bullet bullet;
	Vector3 strafevec, targetvec;
	float scattertime = 0.2f, sc;


    bool anti;

	void Update()
    {
        base.Update();

		if (ent.Dead) Die();
		if (enemy.IsStunned)
        {
            ControllerAgent.SetDestination(transform.position);
			weapon.CurrentAmmo = -1;
        }

		if (!ent.Dead && !enemy.IsStunned)
		{
			sc += Time.deltaTime;
            if (!Combatants.Contains(this)) Combatants.Add(this);

            Target = Entity.LookForEnemy(transform, ent, SightDistance);
			if (Target) TargetValid = Entity.IsValidTarget(Target);
            if (Target != null && TargetValid)
            {
				Melee.Target = Target;
                if (Vector3.Distance(transform.position, Target.transform.position) > AttackDistance || !Entity.IsObjectVisible(transform, Target.gameObject, AttackDistance, LayerMask.GetMask("NPCs", "Default", "Player")))
                {
                    if(cont.isGrounded) ControllerAgent.SetDestination(Target.transform.position);
                }
				else
				{
					if (sc >= scattertime)
                    {
                        ControllerAgent.destination += RandomPos;
						sc = 0;
                    }
				}
			

                if (!TargetAcquired)
                {
                    TargetAcquired = true;
                }

                if ((TargetValid) && Vector3.Distance(transform.position, Target.transform.position) < AttackDistance)
                {
                    CurrentFireInterval += Time.deltaTime;
                    targetvec.x = Target.transform.position.x;
                    targetvec.y = transform.position.y;
                    targetvec.z = Target.transform.position.z;
                    transform.LookAt(targetvec);
					if (sc >= scattertime)
                    {
						//ThisAgent.destination += RandomPos;
						sc = 0;
                    }



                    if (weapon.CurrentAmmo <= 0)
                    {
                        
                        reload += Time.deltaTime;

                        if (reload >= weapon.ReloadTime * 0.58f)
                        {
                            if (!anti)
                            {
                                //PlaySound("EnemyReload", transform.position, false, 0.5f, 120, 5);
                                //Spawn("EnemyWeaponReady", transform.position);
                                anti = true;
                            }
                        }

                        if (reload >= weapon.ReloadTime)
                        {
                            weapon.CurrentAmmo = weapon.MagazineSize;
                            reload = 0;
                        }
                    }


					if (CurrentFireInterval >= FireInterval && weapon.CurrentAmmo > 0)
					{
						Fire();
					}
				}
                else
                {
                    if (Target.Dead)
                    {
                        if (TargetAcquired)
                        {
                            TargetAcquired = false;
                        }
                        Target = null;
                    }
                }
            }
        }
    }
}
