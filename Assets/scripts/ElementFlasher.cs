using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ElementFlasher : MonoBehaviour
{
    public CanvasGroup group;
    public float FadeSpeed = 0.01f;
    private Image ChildImage;

    void Start()
    {
        ChildImage = GetComponentInChildren<Image>();
    }

    void Update()
    {
        if (group.alpha < 0.01f)
        {
            if(ChildImage)
            {
                ChildImage.enabled = false;
            }
        }
        else
        {
            if (ChildImage)
            {
                ChildImage.enabled = true;
            }
        }

        group.alpha = Mathf.Lerp(group.alpha, 0, FadeSpeed * Time.unscaledDeltaTime);
    }

    public void Flash()
    {
        group.alpha = 2;
    }
}
