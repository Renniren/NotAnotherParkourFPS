using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDSubtitles : MonoBehaviour
{
    public CanvasGroup cg;
    public Text caption;

    bool showing;
    float time, show_time;

    void Start()
    {
        
    }

    public void ShowCaption(string body, float time = 3)
    {
        showing = true;
        caption.text = body;
        show_time = time;
        this.time = 0;
    }

    void Update()
    {
        if (showing)
        {
            cg.alpha = Mathf.Lerp(cg.alpha, 1, 12 * Time.unscaledDeltaTime);
            time += Time.unscaledDeltaTime;
            if (time >= show_time)
            {
                showing = false;
            }
        }
        else
        {
            cg.alpha = Mathf.Lerp(cg.alpha, 0, 12 * Time.unscaledDeltaTime);
        }
    }
}
