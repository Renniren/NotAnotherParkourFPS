using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
public class Enemy : EnemyController
{
	[Header("Enemy Settings")]
	public GameObject ragdoll;
	public bool IsStunned;
	public float StunTime = 3;
	public int Rank;
	float stun;

	float limit = 200;
	protected Vector3 RandomPos
	{
		get
		{
			return new Vector3(Random.Range(-limit, limit), Random.Range(-limit, limit), Random.Range(-limit, limit));
		}
	}


	public void Stun(float length = 3)
	{
		stun = 0;
		StunTime = length;
		IsStunned = true;
	}

	protected void OnEnable()
	{
		ControllerAgent.enabled = true;
	}

	public void Die(bool forceGib = false)
	{
		if (ragdoll)
        {
			if(ent.GetKillingDamage().Damage >= ent.GetGibbingDamage() || forceGib)		
			{
				if (isGrounded)
				{
					BecomeGibbedRagdoll(ragdoll, transform, ControllerAgent.velocity * 1.5f);
				}
				else
				{
					BecomeGibbedRagdoll(ragdoll, transform, body.velocity * 1.5f);
				}

				gameObject.SetActive(false);
				return;
			}

			if (ent.GetKillingDamage().Damage < ent.GetGibbingDamage())
			{
				if (isGrounded)
				{
					BecomeRagdoll(ControllerAgent.velocity * 1.5f, ragdoll, transform);
				}
				else
				{
					BecomeRagdoll(body.velocity * 1.5f, ragdoll, transform);
				}
				gameObject.SetActive(false);
			}
        }
		else
        {
			gameObject.SetActive(false);
        }
	}

	protected void OnDisable()
	{
		base.OnDisable();
		stun = StunTime + 1;
		IsStunned = false;
	}

	protected void Update()
	{
		base.Update();

		if (Dead)
		{
			Die();
			return;
		}

		if (IsStunned)
        {
			stun += Time.deltaTime;
			if (stun >= StunTime)
			{
				IsStunned = false;
			}
        }
	}
}
