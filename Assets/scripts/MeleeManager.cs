using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using UnityEngine.AI;

public class MeleeManager : MonoBehaviorExtended
{
	public bool InMelee;
	public Animator animator;
	public GameObject Target;
	public NavMeshAgent agent;
	public MeleeSettings Melee;

	Enemy enemy;

	private void Start()
	{
		enemy = gameObject.GetEnemy();
	}

	void ResetStateAfterMelee()
	{
		agent.isStopped = false;
		PreparedMelee = false;
		InMelee = false;
	}

	float meltime;
	bool PreparedMelee;
	
	float melclosetime;

	void DoMelee()
	{
		if (distance(transform.position, Target.transform.position) > Melee.Range || gameObject.GetEntity().Dead) return;
		melclosetime = 0;

		if (Target.IsPlayer())
        {
			Entity.Hurt(gameObject, Player, Melee.Damage);
			PlaySound("Hit", transform.position, true, 1.1f);
			Player.transform.position += Player.transform.up * 1.1f;
			Player.GetPlayerMovement3D().CancelDrag(Player.transform.position);
			Player.GetPlayerMovement3D().body.velocity = (Vector3.up * (Melee.UpwardsKnockback));
			Player.GetPlayerMovement3D().body.velocity += ((PlayerPosition - transform.position) * Melee.Knockback);
			Player.GetPlayerMovement3D().IsGrounded = false;
			CameraShake.DoShake(0.6f);
        }
		else
        {
			Entity.Hurt(gameObject, Target, Melee.Damage);
			PlaySound("Hit", transform.position, false, 1.1f);
			if (Target.GetEnemyController())
            {
				Target.GetEnemyController().AddForce(((Vector3.up * (Melee.UpwardsKnockback)) +
				((Target.transform.position - transform.position) * Melee.Knockback)));
            }
			else
            {
				Target.GetRigidbody().velocity = (Vector3.up * (Melee.UpwardsKnockback) +
					((Target.transform.position - transform.position) * Melee.Knockback));
            }
		}
	}

	void PrepareMelee()
	{
		Invoke(nameof(ResetStateAfterMelee), Melee.CharacterStopTime);
		agent.isStopped = true;
		meltime = 0;
		Invoke(nameof(DoMelee), Melee.HurtDelay);
		animator.Play(Melee.Animation);
	}


	void ManageMelee()
	{
		if (!Melee.Allowed) return;
		if (enemy) if (enemy.IsStunned) return; 
		meltime += Time.deltaTime;

		if (Vector3.Distance(transform.position, Target.transform.position) < Melee.Range && meltime >= Melee.Cooldown)
		{
			melclosetime += Time.deltaTime;

			if (melclosetime >= Melee.Delay)
			{
				if (!PreparedMelee)
				{
					PrepareMelee();
					InMelee = true;
					PreparedMelee = true;
				}
			}
		}
		else
		{
			melclosetime = 0;
		}
	}

	// Update is called once per frame
	void Update()
    {
		if (Target)
		{
			ManageMelee();
			if (!Entity.IsValidTarget(Target.GetEntity())) Target = null;
		}

    }
}
