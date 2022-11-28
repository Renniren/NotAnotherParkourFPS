using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using Text = TMPro.TMP_Text;


public class ObjectiveDisplay : MonoBehaviorExtended
{
    public Text ObjectiveName;
    public Transform Show, Hide;
    public CanvasGroup cg;
    public float ShowTime;

    float t;
    bool s;

    // Start is called before the first frame update
    void Start()
    {
        Events.OnObjectiveActivated += Events_OnObjectiveActivated;
        transform.position = Hide.position;
    }

    private void Events_OnObjectiveActivated(Objective obj)
    {
        if (obj.Name == string.Empty) return;
        t = 0;
        s = true;
        cg.alpha = 1;
        transform.position = Hide.position;
        ObjectiveName.text = obj.Name;
    }

    // Update is called once per frame
    void Update()
    {
        if (s)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(transform.position, Show.position, 13.37f * Time.deltaTime);
            if (t >= ShowTime)
            {
                s = false;
                t = 0;
                cg.alpha = Mathf.Lerp(cg.alpha, 0, 6.9f * Time.deltaTime);
            }
        }

        if(!s) cg.alpha = Mathf.Lerp(cg.alpha, 0, 6.9f * Time.deltaTime);
    }
}
