using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using UnityEngine.AI;

public enum AddForceMode { Additive, Set }

public class EnemyController : Entity
{
    public static readonly float UnreasonableVelocity = 800;
    
    public Rigidbody body;
    public NavMeshAgent ControllerAgent;
    public float gravity = -9.81f;
    public float drag = 0.5f;
    public float offset = 0;
    float range = 0.7f;
    public float VelocityToLaunch = 10;
    public Vector3 velocity;
    public bool controllerEnabled = true;
    public bool EnableFallDamage = true;
    public bool InRigidbodyState;
    public bool isGrounded;

    static float CheckTime = 0.1337f;
    float current;

    protected void Start()
    {
        
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        isGrounded = true;
    }

    Vector3 dir;
    protected bool grounded()
    {
        RaycastHit hit;
        dir = transform.up;
        Physics.Raycast(transform.position, -dir, out hit, range, LayerMask.GetMask("Default", "NoGoZone", "InvisibleToCamera"));
        if (hit.collider)
        {
            isGrounded = true;
            return true;
        }

        return false;
    }

    protected bool groundcheck()
    {
        return Physics.Linecast(transform.position, transform.position + (-transform.up * 3.5f), LayerMask.GetMask("Default", "NoGoZone", "InvisibleToCamera"));
    }

    Vector3 OldVelocity;
    Collision col;
    void ManageVelocity()
    {
        if (velocity.magnitude >= VelocityToLaunch || !groundcheck())
        {
            if (velocity != Vector3.zero)
            {
                body.velocity = velocity + ControllerAgent.velocity;
                velocity = Vector3.zero;
            }

            if (ControllerAgent.enabled)
            {
                ControllerAgent.enabled = false;
            }
            isGrounded = false;
        }

        if (isGrounded)
        {
            body.velocity = Vector3.zero;
            body.isKinematic = true;
            if (Mathf.Abs(OldVelocity.y) > 69)
            {
                Hurt(gameObject, 920, true, true);
            }


            if (!ControllerAgent.enabled)
            {
                Physics.SyncTransforms();
                
                body.velocity = Vector3.zero;
                ControllerAgent.enabled = true;
            }
        }
        else
        {
            body.isKinematic = false;
            body.AddForce(transform.up * body.mass * gravity, ForceMode.Acceleration);
        }
        if (!isGrounded)
        {
            OldVelocity = body.velocity;
        }
    }

    protected void OnDisable()
    {
        base.OnDisable();
        body.velocity = Vector3.zero;
        velocity = Vector3.zero;
        OldVelocity = Vector3.zero;
    }

    void DoDrag()
    {
        Vector3 dragvel = -body.velocity;
        dragvel.y = 0;
        body.AddForce(dragvel * drag * body.mass);
    }

    public void AddForce(Vector3 force, AddForceMode mode = AddForceMode.Additive)
    {
        switch (mode)
        {
            case AddForceMode.Additive:
                velocity += force;
                break;

            case AddForceMode.Set:
                velocity = force;
                body.velocity = force;
                break;
        }
        current = 0;
    }

    protected void FixedUpdate()
    {
        if (!controllerEnabled) return;
        body.WakeUp();
        if (body.velocity.magnitude > UnreasonableVelocity)
            body.velocity = new Vector3(UnreasonableVelocity * Mathf.Clamp(body.velocity.x, -1, 1),
                UnreasonableVelocity * Mathf.Clamp(body.velocity.y, -1, 1),
                UnreasonableVelocity * Mathf.Clamp(body.velocity.x, -1, 1));
        
        ManageVelocity();
    }

    protected void Update()
    {
        base.Update();
        if (!controllerEnabled) return;
        body.WakeUp();
        current += Time.deltaTime;
        DoDrag();
        if (current >= CheckTime) grounded();
    }

    Rigidbody b;
    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.IsEnemy(collision.gameObject))
        {
            col = collision;
            collision.gameObject.TryGetComponent(out b);

            if (b)
            {
                if (body.mass > b.mass && OldVelocity.y > 35)
                {
                    Hurt(b.gameObject, 400);
                }
            }
        }

        if (current < CheckTime) return;
        foreach(ContactPoint contact in collision.contacts)
        {
            if (transform.position.y > contact.point.y && Physics.Linecast(transform.position, transform.position + (-transform.up * 2)))
            {
                isGrounded = true;
            }
            else
            {
                body.AddForce(-body.velocity * 0.2f);
            }
        }

        
    }

    protected void OnCollisionStay(Collision collision)
    {
        
        if (current < CheckTime) return;
        foreach (ContactPoint contact in collision.contacts)
        {
            if (transform.position.y > contact.point.y && Physics.Linecast(transform.position, transform.position + (-transform.up * 2)))
            {
                isGrounded = true;
            }
            else
            {
                body.AddForce(-body.velocity * 0.6f);
            }
        }
    }
}