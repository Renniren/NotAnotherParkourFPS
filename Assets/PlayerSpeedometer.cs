using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;
using Text = TMPro.TMP_Text;

public class PlayerSpeedometer : MonoBehaviorExtended
{
    public string Unit;
    public Text t;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    PlayerMovement mov;
    // Update is called once per frame
    void Update()
    {
        if (!mov)
        {
            mov = Player.GetPlayerMovement3D();
        }

        t.text = ((int)mov.body.velocity.magnitude).ToString() + Unit;
    }
}
