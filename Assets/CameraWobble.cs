using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//I LOVE QUATERNIONS!!!!
//This script is incredibly messy. All you need to know is... Vector3 goes in, cool effect comes out.
public class CameraWobble : MonoBehaviour
{
    public float test = 10;
    public float MinimumShake = 0;
    public float SmoothAmount = 15;
    public float FalloffSmoothAmount = 15;
    public float RandomizationThreshold = 0.2f;
    public bool DoFallShake = true;
    public bool DoJumpEffect = true;
    public Vector3 direction;
    Quaternion gofuckyourself;


    // Start is called before the first frame update
    void Start()
    {
        gofuckyourself = transform.localRotation;
        if (DoFallShake)PlayerMovement.OnPlayerLand += PlayerMovement_OnPlayerLand;
        if(DoJumpEffect) PlayerMovement.OnPlayerJump += PlayerMovement_OnPlayerJump;
    }

    private void PlayerMovement_OnPlayerJump(Vector3 where)
    {
        AddFallImpulse(new Vector3(-20, 0, 0));
    }

    private void PlayerMovement_OnPlayerLand(Vector3 where)
    {
        if (PlayerState.GetState("IsVaulting") || PlayerState.GetState("IsWallrunning") || PlayerState.GetState("IsSliding")) return;
        AddImpulse(test);
    }

    float scale;
    Quaternion point;

    public void AddImpulse(float s = 1)
    {
        this.scale = s;
    }

    public void AddFallImpulse(Vector3 dir, float s = 1)
    {
        Vector3 fuck_off = dir * s;
        scale = 1;
        gofuckyourself = transform.localRotation;
        gofuckyourself.eulerAngles += dir;
        point = gofuckyourself;
    }

    void Update()
    {
        scale = Mathf.Lerp(scale, MinimumShake, FalloffSmoothAmount * Time.deltaTime);
        scale = Mathf.Clamp(scale, MinimumShake, float.MaxValue);
        if(DoFallShake)point = Random.rotation;


        if (scale <= 0.0000000000001f)
        {
            scale = 0;
            point = Quaternion.identity;
        }

        


        if (DoFallShake)
        {
            Vector3 fuck_off = Vector3.zero;
            float holy_shit_fuck_off = 0;
            point.ToAngleAxis(out holy_shit_fuck_off, out fuck_off);
            point = Quaternion.Euler(fuck_off * scale);
            transform.localRotation = Quaternion.SlerpUnclamped(transform.localRotation, point, SmoothAmount * Time.deltaTime);
        }
        else
        {
            Vector3 fuck_off = Vector3.zero;
            float holy_shit_fuck_off = 0;
            point.ToAngleAxis(out holy_shit_fuck_off, out fuck_off);
            point = Quaternion.Euler(fuck_off * scale);

            gofuckyourself = Quaternion.SlerpUnclamped(gofuckyourself, Quaternion.identity, SmoothAmount * Time.deltaTime);
            transform.localRotation = Quaternion.SlerpUnclamped(transform.localRotation, gofuckyourself, SmoothAmount * Time.deltaTime);
        }
        

        if (Input.GetKeyDown(KeyCode.T))
        {
            AddImpulse(test);
        }
    }
}
