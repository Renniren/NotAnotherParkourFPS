using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using System.Linq;

public class Entity : BaseEntity
{
    public Entity ent
    {
        get
        {
            return this;
        }
    }

    public static int NOTARGET_TEAM = -55;
    public static int NO_TEAM = -1;
    public static int PLAYER_TEAM = -5;
    public static int ENEMY_TEAM = 1;

    [Header("Entity")]
    public int Team;
    public int Health = 100;
    public int Shields = 0;
    public int MaximumHealth = 100;
    public int MaximumExtraShields = 100;
    public int Armor = 0;
    public int DamageThreshold = 0;
    public int MinimumRecievedDamage = 1;
    public bool Invulnerable;
    public bool Dead;
    public bool Robotic;
    public bool IsOnFire;
    public bool CanBleed = true;
    public bool Flammable = true;
    public bool Submerged;
    public List<string> tags = new List<string>();
    public List<DamageInformation> DamageLog = new List<DamageInformation>();

    bool d;
    protected void OnDisable()
    {
        if (ActiveEntities.Contains(this)) ActiveEntities.Remove(this);
        Health = MaximumHealth;
        IsOnFire = false;
        Dead = false;
        Submerged = false;
        d = true;
    }

    protected void Update()
    {
        Dead = Health <= 0;
        Health = Mathf.Clamp(Health, int.MinValue, MaximumHealth);
        if (d)
        {
            DamageLog.Clear();
            d = false;
        }
        if (!ActiveEntities.Contains(this)) ActiveEntities.Add(this);
    }

    #region Class Definitions
    public static implicit operator GameObject(Entity e)
    {
        if (!e) return null;
        return e.gameObject;
    }

    public static implicit operator Transform(Entity e)
    {
        return e.transform;
    }



    [Space]
    public static List<Entity> ActiveEntities = new List<Entity>();

    [System.Serializable]
    public struct DamageInformation
    {
        public GameObject From;
        public int Damage;
        public bool WasFatal;

        public DamageInformation(int damage, GameObject from, bool fatal)
        {
            this.From = from;
            this.Damage = damage;
            this.WasFatal = fatal;
        }
    }

    public DamageInformation GetKillingDamage()
    {
        var result = DamageLog.Find(dmg => dmg.WasFatal);
        return result;
    }

    public DamageInformation GetLastDamage()
    {
        if (DamageLog.Count > 0) return DamageLog[DamageLog.Count - 1];
        return new DamageInformation();
    }

    public int GetGibbingDamage()
    {
        return Mathf.Abs(((int)(ent.MaximumHealth * 0.69f)));
    }

    public static int GetFinalDamage(Entity ent, int damage) { return Mathf.Clamp(damage - ent.Armor, ent.MinimumRecievedDamage, int.MaxValue); }

    public static bool PenetratesArmor(Entity ent, int damage)
    {
        return GetFinalDamage(ent, damage) > ent.Armor;
    }

    public bool HasArmor() { return Armor != 0; }

    public static Entity LookForEnemy(Transform eyes, Entity searcher, float sightdist)
    {
        Entity tgt = null;
        for (i = 0; i < Entity.ActiveEntities.Count; i++)
        {
            if (Entity.ActiveEntities[i])
            {
                if (tgt)
                {
                    if (ActiveEntities[i] == tgt) continue;
                }
                if (ActiveEntities[i] == searcher || ActiveEntities[i].Team == searcher.Team ||
                    ActiveEntities[i].ent.Dead || ActiveEntities[i].Team == NO_TEAM || ActiveEntities[i].Team == NOTARGET_TEAM) continue;
                if (!IsObjectVisible(eyes, Entity.ActiveEntities[i].gameObject, sightdist, LayerMask.GetMask("NPCs", "Default", "Player", "Enemy"))) continue;

                tgt = ActiveEntities[i];
            }
        }

        return tgt;
    }

    public static bool IsValid(Entity query)
    {
        if (query)
        {
            if (!query.Dead && query.gameObject.activeInHierarchy)
            {
                return true;
            }
        }

        if (!query) return false;
        if (!query.gameObject.activeInHierarchy) return false;

        return true;
    }
    public static bool IsValidTarget(Entity query)
    {
        return IsValid(query) && query.Team != NOTARGET_TEAM && query.Team != NO_TEAM && !query.Dead;
    }

    static int i;
    public static void Hurt(GameObject from, GameObject victim, int damage, bool IgnoreArmor = false, bool IgnoreShields = false)
    {
        int init_dmg = damage;
        if (!victim)
        {
            return;
        }

        Events.OnDamageAttemptedInvoke(victim, ref damage, from);
        victim.TryGetComponent(out Entity ent);
        if (!ent)
        {
            if (victim.gameObject.GetHitbox())
            {
                ent = victim.gameObject.GetHitbox().Parent;
            }
        }

        if (!ent) return;

        if (ent.Invulnerable) return;
        if (!IgnoreArmor)
        {
            if (ent.Armor > 0)
            {
                damage -= ent.Armor;
            }
        }

        damage = Mathf.Clamp(damage, ent.MinimumRecievedDamage, int.MaxValue);
        if (damage < ent.DamageThreshold) return;
        ent.DamageLog.Add(new DamageInformation(damage, from, damage >= ent.Health));
        for (i = 0; i <= damage; i++)
        {
            if (ent.Shields > 0)
            {
                ent.Shields--;
            }
            if (ent.Shields <= 0)
            {
                ent.Health--;
            }
        }

        Events.OnEntityDamagedInvoke(victim.GetEntity(), init_dmg, damage, from);
    }

    public static void Heal(GameObject obj, int amt)
    {
        obj.TryGetComponent(out Entity ent);
        if (!ent) return;
        ent.Health += Mathf.Abs(amt);
    }

    public static void Overheal(GameObject obj, int amt)
    {
        obj.TryGetComponent(out Entity ent);
        if (!ent) return;
        ent.Shields += Mathf.Abs(amt);
    }

    public static bool HasTag(Entity ent, params string[] query)
    {
        foreach (var item in query)
        {
            if (ent.tags.Contains(item)) return true;
        }
        return false;
    }

    public static bool IsObjectVisible(Transform from, GameObject query, float range = Mathf.Infinity, int mask = 0)
    {
        if (mask == 0) mask = LayerMask.GetMask("Default", "Player", "Invisible", "Enemy");
        RaycastHit hit;
        Physics.Raycast(from.position, query.transform.position - from.position, out hit, range, mask);

        if (hit.collider)
        {
            return hit.collider.gameObject == query;
        }


        return false;
    }

    public static void Explode(Vector3 where, float radius, float power, int damage, bool AffectsPlayer = true)
    {
        Collider[] colliders = Physics.OverlapSphere(where, radius);
        Rigidbody body;
        foreach (Collider col in colliders)
        {
            if (!AffectsPlayer)
            {
                if (col.gameObject.IsPlayer()) continue;
            }
            if (col.gameObject == null) continue;
            body = col.gameObject.GetRigidbody();
            Hurt(null, col.gameObject, damage);
            if (col.gameObject.IsPlayer())
            {
                col.gameObject.GetPlayerMovement3D().CancelDrag(col.gameObject.transform.position);
            }
            if (body)
            {
                if (body.gameObject.IsEnemy())
                {
                    if (body.gameObject.GetEnemyController())
                    {
                        body.gameObject.GetEnemyController().AddForce((body.position - where).normalized * power, AddForceMode.Set);
                    }
                }
                body.AddExplosionForce(power * body.mass, where, radius);
            }
        }

        Events.OnExplosionInvoke(where, radius, power, damage, colliders);
    }

    public void Kill(bool gib = false)
    {
        if (gib)
        {
            ent.Health = Mathf.RoundToInt(-MaximumHealth * 1.5f);
            ent.Dead = true;
        }
        else
        {
            ent.Health = -1;
            ent.Dead = true;
        }
    }

    public static void Kill(Entity ent, bool gib = false)
    {
        if (!ent) return;
        if (gib)
        {
            ent.Health -= ent.GetGibbingDamage() + 49;
        }
        else
        {
            ent.Health = -1;
        }

        ent.Dead = true;
    }

    public static void Kill(GameObject victim)
    {
        victim.TryGetComponent(out Entity ent);
        if (!ent) return;
        ent.Health = -1;
        ent.Dead = true;
    }
    #endregion
}
