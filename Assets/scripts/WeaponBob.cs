using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBob : MonoBehaviour
{
    public enum BobMode { Conventional, Quake }
    public BobMode bobMode;
    public PlayerMovement ply;
    public Rigidbody body;
    public float WaveAmplitude = 5;
    public float WaveLength = 33;
    public float UpWaveAmplitude = 5;
    public float UpWaveLength = 33;

    Vector3 n;
    float time;
    void Update()
    {
        if (ply.IsGrounded)
        {
            time += Time.deltaTime;
            switch (bobMode)
            {
                case BobMode.Quake:
                    n = Vector3.forward * (Mathf.Sin(time * WaveAmplitude) / WaveLength) *
                    Mathf.Clamp01(body.velocity.magnitude);
                    break;

                case BobMode.Conventional:
                    n = (Vector3.right * (Mathf.Sin(Time.time * WaveAmplitude) / WaveLength) *
                        Mathf.Clamp01(body.velocity.magnitude)) +
                        (Vector3.up * (Mathf.Sin(Time.time * UpWaveAmplitude) / UpWaveLength) *
                        Mathf.Clamp01(body.velocity.magnitude));
                    break;
            }

            transform.localPosition = Vector3.Lerp(transform.localPosition, n, 20 * Time.deltaTime);
        }
        else
        {
            time += Time.deltaTime;
            switch (bobMode)
            {
                case BobMode.Conventional:
                    transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, 20 * Time.deltaTime);
                    break;

                case BobMode.Quake:
                    n = Vector3.forward * (Mathf.Sin(time * WaveAmplitude) / WaveLength) *
                    Mathf.Clamp01(body.velocity.magnitude);
                    transform.localPosition = Vector3.Lerp(transform.localPosition, n, 20 * Time.deltaTime);
                    break;
            }
        }
    }
}
