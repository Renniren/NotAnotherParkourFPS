using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class TimeManager : MonoBehaviour
{
    public bool SlowmoAllowed = true;
    public float TimeScaleRecoveryTime = 0.05f;
    bool OverridingTimescale;
    public static bool SlowingTime;
    public static TimeManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void DramaticHit(float Pausetime)
    {
        StartCoroutine(PauseTime(Pausetime));
    }

    private IEnumerator PauseTime(float HowLong)
    {
        OverridingTimescale = true;
        Physics.autoSimulation = false;
        Time.timeScale = 0.0f;
        yield return new WaitForSecondsRealtime(HowLong);
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.02f;
        Physics.autoSimulation = true;
        OverridingTimescale = false;
    }

    public void PunchTimescale(float DesiredTimescale, float RecoveryTime, bool ModifySound = false)
    {
        Time.timeScale = DesiredTimescale;
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
        TimeScaleRecoveryTime = RecoveryTime;
        OverridingTimescale = false;
    }

    void Update()
    {
        if (!Game.Paused)
        {
            if (!OverridingTimescale)
            {
                if (SlowmoAllowed)
                {
                    if (Input.GetKey(KeyCode.C))
                    {
                        Time.timeScale = Mathf.Lerp(Time.timeScale, 0.22f, TimeScaleRecoveryTime);
                        SlowingTime = true;
                    }
                    else
                    {
                        SlowingTime = false;
                        Time.timeScale = Mathf.Lerp(Time.timeScale, 1, TimeScaleRecoveryTime);
                    }
                }
                else
                {
                    Time.timeScale = Mathf.Lerp(Time.timeScale, 1, TimeScaleRecoveryTime);
                }

                Time.fixedDeltaTime = Time.timeScale * 0.02f;
            }

            if (OverridingTimescale)
            {
                Time.fixedDeltaTime = 0.2f;
            }
        }
        else
        {
            Time.timeScale = 0;
        }
        Time.fixedDeltaTime = Mathf.Clamp(Time.timeScale * 0.02f, 0.001f, 1);
    }
}
