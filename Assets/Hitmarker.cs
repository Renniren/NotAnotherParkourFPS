using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ParkourFPS;

public class Hitmarker : MonoBehaviour
{
    public float ShakeAmount = 4;
    public float RShakeAmount = 4;
    public UIElementPulse pulse;
    public ViewShake ElementShake;
    public Sprite WeakDamage, NormalDamage, HeavyDamage;
    public Image HitmarkerImage;
    public Color NormalColor, FadedColor, CriticalColor;

    // Start is called before the first frame update
    void Start()
    {
        Events.OnEntityDamaged += Events_OnEntityDamaged;
    }

    float show = 0.085f, s;

    private void Events_OnEntityDamaged(Entity ent, int amt, int final, GameObject from)
    {
        if (from.IsPlayer())
        {
            float HeavyDamagePercent = 0.69f;
            float NormalDamagePercent = 0.3f;
            float WeakDamagePercent = 0.12f;

            HitmarkerImage.color = NormalColor * 15;
            if (final >= ent.MaximumHealth * HeavyDamagePercent)
            {
                HitmarkerImage.sprite = HeavyDamage;
                HitmarkerImage.color = CriticalColor * 15;
            }

            if (final >= ent.MaximumHealth * NormalDamagePercent && final <= ent.MaximumHealth * HeavyDamagePercent)
            {
                HitmarkerImage.sprite = NormalDamage;
            }

            if (final <= ent.MaximumHealth * NormalDamagePercent)
            {
                HitmarkerImage.sprite = WeakDamage;
            }

            s = 0;
            ElementShake.DoShake(ShakeAmount);
            ElementShake.DoRotationalShake(RShakeAmount);
            pulse.Show();
        }
    }

    // Update is called once per frame
    void Update()
    {
        s += Time.deltaTime;
        if (s >= show)
        {
            HitmarkerImage.color = Color.Lerp(HitmarkerImage.color, FadedColor, 33 * Time.deltaTime);
        }
    }
}
