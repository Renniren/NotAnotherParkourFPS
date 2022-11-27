using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class FOVManager : MonoBehaviorExtended
{
    public Camera MainCamera;
    public Camera ViewmodelCamera;

    public float SmoothSpeed = 15;

    public float MainCameraDefaultFOV = 105;
    public float VMCameraDefaultFOV = 72;

    public float mc_fov = 105;
    public float vm_fov = 72;

    void Start()
    {
        mc_fov = MainCameraDefaultFOV;
        vm_fov = VMCameraDefaultFOV;

        Events.OnDefenseBroken += Events_OnDefenseBreakEnd;
    }

    private void Events_OnDefenseBreakEnd(GameObject target)
    {
        MainCamera.focalLength += 20;
        mc_fov += 20;
    }

    private void OnDestroy()
    {
        Events.OnDefenseBroken -= Events_OnDefenseBreakEnd;
    }

    bool in_dash;

    void Update()
    {
        float s = SmoothSpeed * Time.deltaTime;
        MainCamera.fieldOfView = Mathf.Lerp(MainCamera.fieldOfView, mc_fov, s);
        ViewmodelCamera.fieldOfView = Mathf.Lerp(ViewmodelCamera.fieldOfView, vm_fov, s);
    }

    private void LateUpdate()
    {
        /*
        if (PlayerDash.IsDashing)
        {
            if (!in_dash)
            {
                float m_dash_fov = MainCameraDefaultFOV + (10 * -Mathf.Clamp(
                    Player.transform.InverseTransformDirection(Player.GetRigidbody().velocity).z, -1, 1));
                float v_dash_fov = VMCameraDefaultFOV + 4;
                mc_fov = m_dash_fov;
                vm_fov = v_dash_fov;
                in_dash = true;
            }
        }
        else
        {
            if (!PlayerHotswitch.InDefenseBreak)
            {
                if (in_dash)
                {
                    mc_fov = MainCameraDefaultFOV;
                    vm_fov = VMCameraDefaultFOV;
                    in_dash = false;
                }

                float v_limit = 10;
                mc_fov = MainCameraDefaultFOV + -Mathf.Clamp(Player.GetRigidbody().velocity.y * 0.8f, -v_limit, v_limit);
            }
        }
        */

    }
}
