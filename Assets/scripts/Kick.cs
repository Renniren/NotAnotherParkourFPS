using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using ParkourFPS;

// Logic for Kicking lifted and ported from Insurrection.

public class Kick : MonoBehaviorExtended
{
    public Transform Head;
    public PlayerMovement player;
    public AudioClip AnticipationSound;
    public AudioClip KickSound;
    public LayerMask KickableObjects;
    public static AudioSource Sound;
    private int KickType;
    public float HitCheckRadius = 3;
    public float Range = 3;
    public float MidAirRange;
    private float ActualRange;
    public float ObjectForce = 50;
    public float ObjectUpwardForce = 20;
    public float PlayerForce = 15;
    public float PlayerUpwardsForce = 11;
    public int Damage = 25;
    public Animator anims;
    public UnityEvent KickEvent;
    public UnityEvent DropkickEvent;
    
    // Start is called before the first frame update
    void Start()
    {
        //???
        //Sound = KickSound;
    }

    private void OnDrawGizmo()
    {
        Gizmos.DrawLine(Head.position, Head.position + Head.forward * MidAirRange);
        Gizmos.DrawLine(Head.position, Head.position + Head.forward * Range);
        Gizmos.DrawWireSphere(transform.position, HitCheckRadius);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q) && !Game.Paused && !Game.InCutscene)
        {
            if(player.IsGrounded)
            {
                KickType = 1;
                ActualRange = Range;
                anims.Play("kick");
            }
            
            if(!player.IsGrounded)
            {
                KickType = 0;
                ActualRange = MidAirRange;
                anims.Play("dropkick");
            }
        }
    }

    void Anticipation()
    {
        DidScoreShit = false;
        PlaySound(AnticipationSound, transform.position, true);
    }
    bool DidScoreShit = false;
    void KickMethod()
    {
        RaycastHit hit;
        Rigidbody rb;
        Entity info;
        EnemyController cont;

        Physics.Raycast(Head.position, Head.forward, out hit, ActualRange, KickableObjects);
        int FinalDamage = Damage;
        FinalDamage = Mathf.Clamp(FinalDamage, Damage, 70);
        if (hit.collider)
        {
            CameraShake.DoShake(0.5f);
            TimeManager.Instance.DramaticHit(0.035f);
            PlaySound(KickSound, transform.position, true);
            Collider[] Colliders = Physics.OverlapSphere(hit.point, HitCheckRadius);
            if(hit.collider.gameObject != Player && !hit.collider.CompareTag("ExplosiveBarrels"))
            {    
                Entity.Hurt(Player, hit.collider.gameObject, FinalDamage);
            }
            
            player.IsGrounded = false;
            player.CancelDrag(transform.position);
            player.body.AddForce(Player.transform.up * PlayerUpwardsForce * player.body.mass);
            hit.collider.TryGetComponent(out rb);

            if (rb && !hit.collider.gameObject.IsEnemy())
            {
                rb.transform.LookAt(transform.position + Player.transform.forward);
                rb.velocity += -rb.transform.forward * ObjectForce;
                rb.velocity += rb.transform.up * ObjectUpwardForce;
            }
            
            foreach(Collider col in Colliders)
            {
                if(col.gameObject != Player)
                {
                    col.TryGetComponent(out info);
                    col.transform.root.TryGetComponent(out cont);

                    if (cont)
                    {
                        Vector3 force = (Head.forward * (ObjectForce)) + (Head.up * (ObjectUpwardForce ));
                        cont.transform.position += Vector3.up * 0.8f;
                        cont.isGrounded = false;
                        cont.AddForce(force, AddForceMode.Set);
                    }

                    if (info)
                    {
                        if (KickType == 0)
                        {
                            Entity.Hurt(Player, col.gameObject, FinalDamage * 2);
                        }

                        if (KickType == 1)
                        {
                            Entity.Hurt(Player, col.gameObject, FinalDamage);
                        }
                    }
                }
            }


            //???
            /*if(rb)
            {
                if (!rb.isKinematic && !rb.gameObject.IsEnemy(rb.gameObject))
                {
                    rb.transform.position += Vector3.up * 0.5f;
                    if (rb.CompareTag("ExplosiveBarrels"))
                    {
                        TimeManager.Instance.DramaticHit(0.1f);
                        rb.transform.LookAt(transform.position + Player.transform.forward);
                        rb.velocity += -rb.transform.forward * ObjectForce;
                        rb.velocity += rb.transform.up * ObjectUpwardForce;
                    }
                    else
                    { 
                        rb.transform.LookAt(transform.position + Player.transform.forward);
                        rb.velocity += -rb.transform.forward * ObjectForce;
                        rb.velocity += rb.transform.up * ObjectUpwardForce;
                    }
                }
            }*/
        }
    }
}
