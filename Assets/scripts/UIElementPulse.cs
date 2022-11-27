using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIElementPulse : MonoBehaviour
{
    public float LerpSpeed = 0.3f;
    public float AppearScale = 1;
    public float NormalScale = 0.7f;
    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * NormalScale, LerpSpeed * Time.deltaTime);
    }

    public void Show()
    {
        transform.localScale = Vector3.one * AppearScale;
    }
}
