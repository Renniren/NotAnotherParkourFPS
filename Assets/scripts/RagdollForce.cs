using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS; 

public class RagdollForce : MonoBehaviorExtended
{
    public Vector3 InheritedVelocity;
    public bool Dismembered;
    public Rigidbody[] Rigidbodies;
    CharacterJoint joint = null, joint2 = null;
    bool AppliedMostForceTo = false;
    float t;

    private int coin;
    // Start is called before the first frame update
    void Start()
    {
        
        foreach (Rigidbody body in Rigidbodies)
        {
            if(body != null)
            {
                body.isKinematic = false;
                body.velocity = InheritedVelocity;
                if (body.CompareTag("NoDismember"))
                {
                    
                    body.TryGetComponent(out joint);
                    if(joint != null)
                    {
                        joint.breakForce = Mathf.Infinity;
                    }
                }
                if (Dismembered && !body.gameObject.CompareTag("NoDismember"))
                {
                    body.TryGetComponent(out joint);
                    if (joint != null)
                    {
                        joint.breakForce = 0;
                        body.AddForce(Vector3.Cross(body.transform.right * Random.Range(-1, 2), body.transform.up) * 200 * body.mass);
                    }
                }
                coin = Random.Range(0, 3);
                if(coin > 1)
                {
                    if(!AppliedMostForceTo)
                    {
                        body.AddForce(-transform.forward * 460 * body.mass);
                        body.AddForce(Vector3.up * 90 * body.mass);
                        AppliedMostForceTo = true; 
                    }
                    else
                    {
                        body.AddForce(-body.transform.forward * 280 * body.mass);
                    }
                }
                else
                {
                    body.AddForce(-transform.forward * 250 * body.mass);
                }
                body.AddForce(Vector3.up * 220 * body.mass); 
            }
            }
            
            
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (Rigidbodies.AverageVelocity().magnitude > 60 && t >= 0.1f && !Dismembered)
        {
            //Spawn("SmokePuff", Rigidbodies[RandomIndex(Rigidbodies)].position);
            t = 0;
        }
    }
}
