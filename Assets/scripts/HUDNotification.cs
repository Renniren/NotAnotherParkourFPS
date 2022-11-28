using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDNotification : MonoBehaviour
{
    public Text body;
    public Transform show, hide;
    public float ShowTime = 3;

    float s;
    bool active;

    void Start()
    {
        
    }

    public void Show(string text, float time = 3)
    {
        s = 0;
        active = true;
        ShowTime = time;
        body.text = text;
    }

    void Update()
    {
        if (active)
        {
            s += Time.unscaledDeltaTime;
            if (s >= ShowTime)
            {
                s = 0;
                active = false;
            }

            transform.position = Vector3.Lerp(transform.position, show.position, 9 * Time.unscaledDeltaTime);
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, hide.position, 9 * Time.unscaledDeltaTime);
        }
    }
}
