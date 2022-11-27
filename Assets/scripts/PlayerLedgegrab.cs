using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerLedgegrab : MonoBehaviour
{
    public bool IsLedgegrabbing;
    public bool Lerp;
    public bool CanGrabWhileMovingUp = true;
    public float LedgecheckRange;
    public float LedgegrabSpeed;
    public float BodyRadius;
    public Vector3 BodyCheckOffset;
    public PlayerMovement playerComponent;
    public Rigidbody body;
    public Transform LedgeCheck;
    public UnityEvent Ledgegrabbed;
    
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(LedgeCheck.position, -LedgeCheck.up * LedgecheckRange);
        Gizmos.DrawRay(transform.position + transform.InverseTransformVector(BodyCheckOffset),  transform.forward * BodyRadius);

    }

    private void Update()
    {
        HandleLedgegrabbing();
    }

    private void FixedUpdate()
    {
        if(!IsLedgegrabbing)HandleLedgegrabbing();
    }

    private void LateUpdate()
    {
        if (!IsLedgegrabbing) HandleLedgegrabbing();
    }

    Vector3 Above;
    bool AppliedOffset;
    void HandleLedgegrabbing()
    {
        bool StopGrabbingYourselfFuckingRetard(RaycastHit hit, RaycastHit hit2)
        {
            if (hit.collider.gameObject.layer != 12 && 
                hit.collider.gameObject.layer != 8 && 
                hit.collider.gameObject.layer != 16 && 
                hit.collider.gameObject.layer != 15 && 
                hit.collider.gameObject.layer != gameObject.layer && 
                hit2.collider.gameObject.layer != gameObject.layer && 
                !hit.collider.gameObject.CompareTag("Enemy") && 
                playerComponent.MovePure.z > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool g = body.velocity.y < 0;
        if (CanGrabWhileMovingUp) g = true;

        if (g && !playerComponent.IsGrounded && !IsLedgegrabbing && !Physics.Linecast(transform.position, transform.position + transform.up * 3.7f))
        {
            RaycastHit hit;
            RaycastHit hit2;
            Physics.Raycast(LedgeCheck.position, -LedgeCheck.up, out hit, LedgecheckRange);
            Physics.Raycast(transform.position + transform.InverseTransformVector(BodyCheckOffset), transform.forward, out hit2, BodyRadius);
            if (Physics.Linecast(LedgeCheck.position + -LedgeCheck.forward * 1.4f, LedgeCheck.position)) return;
            if (hit.collider != null && hit2.collider != null)
            {
                if (StopGrabbingYourselfFuckingRetard(hit, hit2))
                {
                    IsLedgegrabbing = true;
                }
            }
            else
            {
                IsLedgegrabbing = false;
            }
        }

        if (IsLedgegrabbing)
        {
            body.velocity = Vector3.zero;

            if (!AppliedOffset)
            {
                body.isKinematic = true;
                Above = transform.position + (Vector3.up * 3.7f);
                Ledgegrabbed.Invoke();
                AppliedOffset = true;
            }

            if (Vector3.Distance(transform.position, Above) < 1.2f)
            {
                IsLedgegrabbing = false;
                body.isKinematic = false;
                body.velocity = transform.forward * 4.9f;
            }

            if(!Lerp)transform.position = Vector3.MoveTowards(transform.position, Above, LedgegrabSpeed * Time.deltaTime);
            if(Lerp)transform.position = Vector3.Lerp(transform.position, Above, LedgegrabSpeed * Time.deltaTime);
            
        }
        else
        {
            AppliedOffset = false;
        }
    }
}
