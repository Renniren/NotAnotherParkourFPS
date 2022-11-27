using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float width;
    public float FadeSpeed;
    public float TracerSpeed;
    public bool StartFollowsEnd;
    public LineRenderer line;
    float w;

    [HideInInspector] public Vector3 start;
    [HideInInspector] public Vector3 end;

    void Update()
    {
        width = Mathf.Lerp(width, 0, FadeSpeed * Time.deltaTime);
        line.SetWidth(width, width);
        line.startWidth = width;
        line.endWidth = width;

        if (StartFollowsEnd)
        {
            start = Vector3.MoveTowards(start, end, TracerSpeed * Time.deltaTime);
            line.SetPosition(0, start);
        }
    }
}
