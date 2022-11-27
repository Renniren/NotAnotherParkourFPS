using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProbeDebugBox : MonoBehaviour
{
    ReflectionProbe pr;

    // Start is called before the first frame update
    void Start()
    {
        
    }


    bool hasNoProbe;
    private void OnDrawGizmos()
    {
        if (hasNoProbe)
        {
            Gizmos.DrawWireCube(transform.position, transform.localScale);
        }

        if (hasNoProbe) return;
        if (!pr)
        {
            TryGetComponent(out pr);
            hasNoProbe = pr == null;
        }

        Gizmos.color = Color.grey;
        Gizmos.DrawWireCube(transform.TransformPoint(pr.center), pr.size);
    }

}
