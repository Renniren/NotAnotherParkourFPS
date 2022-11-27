using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class BulletTracerCreator : MonoBehaviorExtended
{
    public float FadeSpeed;
    public LineRenderer tracer;
    static readonly int MAXIMUM_TRACERS = 32;
    int tracerArrayOffset = 0;
    float w;
    void Start()
    {
        tracer.SetPositions(new Vector3[MAXIMUM_TRACERS]);
    }

    public void MakeTracer(Vector3 start, Vector3 end, float width, float fadeSpeed = 12)
    {
        Spawn("TracerObject", start).TryGetComponent(out LineRenderer line);
        line.TryGetComponent(out BulletTracer bt);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.SetWidth(w, w);
        line.endWidth = w;
        line.startWidth = w;
        bt.FadeSpeed = fadeSpeed;
        bt.width = width;

        /*w = width;
        FadeSpeed = fadeSpeed;
        (Vector3, Vector3) trace = (start, end);
        Vector3[] tracerPoints = { trace.Item1, trace.Item2 };
        if (tracerArrayOffset % 2 == 0)
        {
            tracer.SetPosition(0 + tracerArrayOffset, trace.Item1);
            tracerArrayOffset++;
        }

        if (tracerArrayOffset % 2 != 0)
        {
            tracer.SetPosition(0 + tracerArrayOffset, trace.Item2);
            tracerArrayOffset++;
        }

        if (tracerArrayOffset > MAXIMUM_TRACERS)
        {
            tracerArrayOffset = 0;
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
