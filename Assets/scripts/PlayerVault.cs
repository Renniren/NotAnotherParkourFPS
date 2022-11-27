using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVault : MonoBehaviour
{
    public PlayerMovement playerComponent;
    public LayerMask vaultable;
    public float VaultSpeed = 10;
    public float FeetCheckRange = 4;
    public float VaultCheckDepth = 3;
    public float VaultDistance = 5;
    public float HeightMult = 0.09f;
    public bool InVault;
    public Transform Feet;
    public Transform VaultCheck;

    Vector3 VaultPoint;
    Vector3 FootPoint;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(VaultCheck.position, -VaultCheck.up * VaultCheckDepth);
        Gizmos.DrawRay(Feet.position, Feet.forward * FeetCheckRange);
        Gizmos.DrawRay(transform.position, transform.forward * VaultDistance);
        //Gizmos.DrawWireSphere(center, 0.5f);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(FootPoint, Vector3.one * 0.5f);
        Gizmos.DrawWireSphere(VaultPoint, 1.5f);
    }

    Vector3 destination, center, fwd;
    void HandleVaulting()
    {
        RaycastHit feet, vault;
        Physics.Raycast(VaultCheck.position, -VaultCheck.up, out vault, VaultCheckDepth, vaultable);
        Physics.Raycast(Feet.position, Feet.forward, out feet, VaultCheckDepth, vaultable);

        bool VaultAllowed = playerComponent.IsGrounded &&
            vault.collider == null &&
            feet.collider != null &&
            !InVault &&
            PlayerState.GetState("IsOnGround") &&
            !Physics.Linecast(transform.position, VaultCheck.position);

        if (Input.GetKeyDown(KeyCode.Space) && VaultAllowed)
        {
            InVault = true;
            playerComponent.body.velocity = Vector3.zero;
            destination = transform.position + transform.forward * VaultDistance;
            center = transform.position + (destination - transform.position) / 2;
            fwd = transform.forward;

            VaultPoint = vault.point;
            FootPoint = feet.point;
        }

        if (InVault)
        {
            playerComponent.body.isKinematic = true;
            playerComponent.body.velocity = Vector3.zero;
            transform.position = Vector3.Lerp(transform.position, destination, VaultSpeed * Time.deltaTime);
            transform.position += Vector3.up * HeightMult * Vector3.Dot(fwd, center - transform.position);
            if (Vector3.Distance(transform.position, destination) < 1)
            {
                InVault = false;
                playerComponent.body.velocity += transform.forward * 2;
                playerComponent.body.isKinematic = false;
            }
        }

        PlayerState.SetState("IsVaulting", InVault);
    }

    void LateUpdate()
    {
        HandleVaulting();
    }
}
