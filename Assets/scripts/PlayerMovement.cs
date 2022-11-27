using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class PlayerMovement : MonoBehaviour
{
    public enum lockMovementMode { LockX, LockZ, LockAll, LockNone }

    [SerializeField] public lockMovementMode LockMovementMode = lockMovementMode.LockNone;
    public enum MouselookLockMode { None, LockPitch, LockYaw, LockBoth }
    public MouselookLockMode MouseLockMode = MouselookLockMode.None;

    [Header("References")]
    public Rigidbody body;
    public Collider BodyCollider;
    public BoxCollider DeadCollider;
    public Transform head;
    public Transform GroundCheck;
    public Entity entity;

    [Header("Extra")]
    public Vector3 InitialVelocity;
    public bool EnableJumpBuffering = true;
    public float GroundFriction = 15;
    public float AirstrafeMultiplier = 1;
    public float JumpBufferDistanceWindow = 1;

    [Header("Scalars")]
    public float Sensitivity = 2;
    public float MovementScalar = 1;
    public float GravityMultiplier = 1;
    public float JumpBoostMultiplier = 0.5f;
    public float CheckRadius = 0.15f;

    [Header("Movement")]
    public int Jumps = 0;
    public float JumpForce = 8;
    public float Acceleration = 90;
    public float AerialAcceleration = 9;
    public float FastAerialAcceleration;
    public string[] GroundLayers;

    [Header("Drag")]
    public float AerialDrag = 0.45f;
    public float GroundedDrag = 20;
    public float FastAerialDrag = 0.4f;
    public float DragVelocityLimit = 30;

    [Header("Debug")]
    public float CurrentDrag;
    public float DistanceFromGround;
    public float TimeSinceLeavingGround;
    public float TimeSinceHittingGround;

    public int CurrentJumps = 0;
    public bool EnableDrag = true;
    public bool MovementLocked;
    public bool VelocityLocked;
    public bool IsGrounded;
    public Vector3 MovePure;
    public Vector3 move;
    public Vector3 moveUnclamped;
    public float yaw, pitch;
    float speed;

    public delegate void onPlayerJumpInMidair(Vector3 where, int RemainingJumps);
    public delegate void onPlayerJump(Vector3 where);
    public delegate void onPlayerLand(Vector3 where);
    public static event onPlayerJumpInMidair OnPlayerJumpInMidair;
    public static event onPlayerJump OnPlayerJump;
    public static event onPlayerLand OnPlayerLand;
    public static PlayerMovement current;
    float c;
    bool landed;
    Vector2 mouse;

    void Start()
    {
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 2);
        entity.Team = Entity.PLAYER_TEAM;
        MovementLocked = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        yaw = transform.eulerAngles.y;
        current = this;
        Invoke(nameof(SetInitialVelocity), 0.069f);
    }

    void SetInitialVelocity()
    {
        body.velocity = InitialVelocity;
    }

    Collider[] CollidersOnGround;

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(GroundCheck.position, CheckRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, asf_v3);
        Gizmos.color = Color.red;
    }

    Vector3 point;
    public void CancelDrag(Vector3 fromWhere)
    {
        CancellingDrag = true;
        point = fromWhere;
    }

    public void StartDrag()
    {
        CancellingDrag = false;
        point = transform.position;
    }

    public void TryJump()
    {
        if ((!IsGrounded && CurrentJumps == 0) || MovementLocked) return;
        if (IsGrounded) OnPlayerJump?.Invoke(transform.position);
        if (!IsGrounded) OnPlayerJumpInMidair?.Invoke(transform.position, Jumps);
        c = 0;
        IsGrounded = false;
        JumpBuffered = false;
        body.velocity = new Vector3(body.velocity.x, JumpForce, body.velocity.z) + (move * JumpBoostMultiplier);
        CurrentDrag = AerialDrag;
        if (!IsGrounded) CurrentJumps -= 1;
    }

    public void ForceJump()
    {
        if (IsGrounded) OnPlayerJump?.Invoke(transform.position);
        if (!IsGrounded) OnPlayerJumpInMidair?.Invoke(transform.position, Jumps);
        c = 0;
        IsGrounded = false;
        JumpBuffered = false;
        body.velocity = new Vector3(body.velocity.x, JumpForce, body.velocity.z) + move;
    }

    bool CancellingDrag;
    Vector3 DragVelocity;
    void DoDrag()
    {
        if (!EnableDrag) return;
        if (TimeManager.SlowingTime) body.AddForce(-DragVelocity * (CurrentDrag * 1.3f), ForceMode.Acceleration);
        if (!TimeManager.SlowingTime) body.AddForce(-DragVelocity * CurrentDrag, ForceMode.Acceleration);
    }

    private RaycastHit Slopehit;
    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, -transform.up, out Slopehit, ((CapsuleCollider)BodyCollider).height / 2 + 2.2f))
        {
            if (Slopehit.normal != Vector3.up)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        return false;
    }

    Vector3 SlopeMove;
    void OnGroundLoop()
    {
        TimeSinceHittingGround += Time.deltaTime;
        CurrentJumps = Jumps;
        if (OnSlope())
        {
            CurrentDrag = GroundedDrag * 1.4f;
            body.AddForce(-Slopehit.normal * 200, ForceMode.Force);
            SlopeMove = Vector3.ProjectOnPlane(move, Slopehit.normal);
        }
        else
        {
            // body.AddForce(-transform.up * 200, ForceMode.Force);
        }

        if (TimeManager.SlowingTime)
        {
            if (!MovementLocked)
            {
                if (OnSlope())
                {
                    body.AddForce(SlopeMove * ((Acceleration * MovementScalar) * 3), ForceMode.Acceleration);
                }
                else
                {
                    body.AddForce(move * ((Acceleration * MovementScalar) * 3), ForceMode.Acceleration);
                }
            }
        }
        else
        {
            if (!MovementLocked)
            {
                body.AddForce(move * (Acceleration * MovementScalar), ForceMode.Acceleration);
            }
        }
    }

    Vector3 asf_v3;
    bool DoingNormalAirMovement, DoingFastAirMovement;
    void InAirLoop()
    {
        TimeSinceLeavingGround += Time.deltaTime;
        if (DragVelocity.magnitude < DragVelocityLimit && !CancellingDrag)
        {
            if (!MovementLocked)
            {
                DoingNormalAirMovement = true;
                DoingFastAirMovement = false;
                body.AddForce(((move + asf_v3) * (AerialAcceleration * MovementScalar)), ForceMode.Acceleration);
            }
        }

        if (DragVelocity.magnitude > DragVelocityLimit || CancellingDrag)
        {
            if (!MovementLocked)
            {
                DoingNormalAirMovement = false;
                DoingFastAirMovement = true;
                body.AddForce((move * (FastAerialAcceleration)), ForceMode.Acceleration);
            }
        }



        if (CancellingDrag)
        {
            CurrentDrag = FastAerialDrag;
        }
    }

    public void SetMouselookLockmode(MouselookLockMode mode)
    {
        MouseLockMode = mode;
    }

    public void LockMovement()
    {
        MovementLocked = true;
    }

    public void LockMovement(lockMovementMode mode)
    {
        LockMovementMode = mode;
    }

    public void UnlockMovement()
    {
        MovementLocked = false;
        LockMovementMode = lockMovementMode.LockNone;
    }

    public void AddImpulse(Vector3 force)
    {
        body.velocity += force;
        if (VelocityWouldPlaceInAir(force))
        {
            IsGrounded = false;
        }

        if (body.velocity.magnitude >= DragVelocityLimit)
        {
            CancelDrag(transform.position);
        }
    }

    public void SetRotation(float x, float y)
    {
        yaw = y;
        pitch = x;
        transform.eulerAngles = new Vector3(x, y, transform.eulerAngles.z);
    }

    Vector3 GetAirstrafeVelocity()
    {
        // project the velocity onto the movevector
        Vector3 projVel = Vector3.Project(body.velocity, move);

        // check if the movevector is moving towards or away from the projected velocity
        bool isAway = Vector3.Dot(move, projVel) <= 0f;

        // only apply force if moving away from velocity or velocity is below MaxAirSpeed
        if (projVel.magnitude < DragVelocityLimit || isAway)
        {
            // calculate the ideal movement force
            Vector3 vc = move.normalized * AirstrafeMultiplier;

            // cap it if it would accelerate beyond MaxAirSpeed directly.
            if (!isAway)
            {
                vc = Vector3.ClampMagnitude(vc, DragVelocityLimit - projVel.magnitude);
            }
            else
            {
                vc = Vector3.ClampMagnitude(vc, DragVelocityLimit + projVel.magnitude);
            }

            // Apply the force
            return vc;
        }
        return Vector3.zero;
    }

    bool JumpBuffered;
    void FixedUpdate()
    {
        if (!entity.Dead)
        {
            if (c >= 0.1f) body.AddForce(transform.up * -9.81f * GravityMultiplier * body.mass);


            if (IsGrounded)
            {
                DoDrag();
                CancellingDrag = false;
                BodyCollider.sharedMaterial.dynamicFriction = 0.2f;
                BodyCollider.sharedMaterial.staticFriction = 0.2f;
                CurrentDrag = Mathf.Lerp(CurrentDrag, GroundedDrag, GroundFriction * Time.unscaledDeltaTime);

                if (!landed)
                {
                    DoDrag();
                    TimeSinceHittingGround = 0;
                    TimeSinceLeavingGround = 0;
                    OnPlayerLand?.Invoke(transform.position);
                    landed = true;
                }

                DoingFastAirMovement = false;
                DoingNormalAirMovement = false;
                OnGroundLoop();
            }
            else
            {
                if (DragVelocity.magnitude < DragVelocityLimit && !CancellingDrag) DoDrag();
                BodyCollider.sharedMaterial.dynamicFriction = 0.0f;
                BodyCollider.sharedMaterial.staticFriction = 0.0f;
                CurrentDrag = AerialDrag;

                InAirLoop();
                if (landed)
                {
                    TimeSinceHittingGround = 0;
                    TimeSinceLeavingGround = 0;
                }
                landed = false;
            }
        }

    }

    void CheckDistanceFromGround()
    {
        Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, Mathf.Infinity, LayerMask.GetMask(GroundLayers));
        if (hit.collider)
        {
            if (!IsGrounded) DistanceFromGround = Vector3.Distance(transform.position, hit.point);
            if (IsGrounded) DistanceFromGround = Vector3.Distance(transform.position, hit.point);
        }
    }

    void CheckIfGrounded()
    {
        CollidersOnGround = Physics.OverlapSphere(GroundCheck.position, CheckRadius, LayerMask.GetMask(GroundLayers));
        if (CollidersOnGround.Length >= 1)
        {
            IsGrounded = true;
        }

        if (CollidersOnGround.Length < 1)
        {
            IsGrounded = false;
        }


        if (TimeSinceHittingGround > 0.1f)
        {
            PlayerState.SetState("IsOnGround", true);
        }
        else
        {
            PlayerState.SetState("IsOnGround", false);
        }
    }

    bool VelocityWouldPlaceInAir(Vector3 vel)
    {
        Collider[] test = Physics.OverlapSphere(GroundCheck.position + vel, CheckRadius, LayerMask.GetMask(GroundLayers));
        return test.Length >= 1;
    }

    void ManageMouse()
    {
        if (MouseLockMode != MouselookLockMode.LockYaw && MouseLockMode != MouselookLockMode.LockBoth && !Game.Paused)
            yaw += Sensitivity * Input.GetAxis("Mouse X");

        if (MouseLockMode != MouselookLockMode.LockBoth && MouseLockMode != MouselookLockMode.LockBoth && !Game.Paused)
            pitch -= Sensitivity * Input.GetAxis("Mouse Y");

        mouse.x = Sensitivity * Input.GetAxis("Mouse X");
        mouse.y = Sensitivity * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -90, 90);
        if (MouseLockMode != MouselookLockMode.LockBoth)
        {
            head.localEulerAngles = new Vector3(pitch, 0.0f, 0.0f);
            transform.eulerAngles = new Vector3(0.0f, yaw, 0.0f);
        }
    }

    void Update()
    {
        speed = body.velocity.magnitude;
        PlayerState.SetState("IsGrounded", IsGrounded);
        move = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        moveUnclamped = transform.forward * Input.GetAxis("Vertical") + transform.right * Input.GetAxis("Horizontal");
        move.y = 0;
        moveUnclamped.y = 0;
        move = Vector3.ClampMagnitude(move, 1);
        MovePure.x = Input.GetAxis("Horizontal");
        MovePure.z = Input.GetAxis("Vertical");

        //Since Unity likes to break physics materials after around 5 minutes of use.
        BodyCollider.material.dynamicFriction = 0;
        BodyCollider.material.staticFriction = 0;

        if (Game.Paused)
        {
            Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 2);
        }

        if (!entity.Dead)
        {
            /*            if (speed > HighVelocityCutoff &&
                            TimeSinceHittingGround >= 0.09f &&
                            PreventHighVelocity) body.velocity =
                                new Vector3(Vector3.ClampMagnitude(body.velocity, HighVelocityCutoff).x, body.velocity.y, Vector3.ClampMagnitude(body.velocity, HighVelocityCutoff).z);
            */

            DragVelocity = new Vector3(body.velocity.x, 0, body.velocity.z);
            CheckIfGrounded();
            CheckDistanceFromGround();
            ManageMouse();

            c += Time.deltaTime;
            CurrentJumps = Mathf.Clamp(CurrentJumps, 0, Jumps);

            if (DoingNormalAirMovement)
            {
                //asf_v3 = ((transform.right) * AirstrafeMultiplier * Mathf.Abs(mouse.x + body.velocity.z / 10) * MovePure.x) * Mathf.Abs(MovePure.z);
            }
            asf_v3 = (GetAirstrafeVelocity() * AirstrafeMultiplier) + ((transform.right) * AirstrafeMultiplier * Mathf.Abs(mouse.x + body.velocity.z / 10) * MovePure.x) * Mathf.Abs(MovePure.z);

            if (!IsGrounded)
            {
                if (EnableJumpBuffering)
                {
                    if (Input.GetKeyDown(KeyCode.Space))
                    {
                        if (DistanceFromGround <= JumpBufferDistanceWindow)
                        {
                            JumpBuffered = true;
                        }
                    }
                }
            }

            if (Input.GetKeyDown(KeyCode.Space) || JumpBuffered)
            {
                TryJump();
            }


            if (CancellingDrag)
            {
                if (Vector3.Distance(transform.position, point) >= 10) CancellingDrag = false;
            }

            switch (LockMovementMode)
            {
                case lockMovementMode.LockX:
                    move.x = 0;
                    break;

                case lockMovementMode.LockZ:
                    move.z = 0;
                    break;

                case lockMovementMode.LockAll:
                    move.z = 0;
                    move.x = 0;
                    break;
            }

        }
        else
        {
            body.freezeRotation = false;
        }
    }

    private void LateUpdate()
    {
        if (VelocityLocked && !entity.Dead) body.velocity = Vector3.zero;
    }
}
