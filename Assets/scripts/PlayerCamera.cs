using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ParkourFPS;

public class PlayerCamera : MonoBehaviorExtended
{
  private float pitch;
  private float recoil;
  private float roll;
  public float DefaultFOV;
  public Camera cam;
    public Rigidbody PlayerBody;
    public float SlideRollMultiplier = 0.02f;
  public bool DoFOV;
  public bool DoSetFOV;
  public bool DoMouselook;
  public float maxRecoil = 1.2f;
  public float speedV = 2.0f;
  public GameObject body;

    void Awake()
    {
        cam = GetComponent<Camera>();
        
    }

    void Update()
    {
        if(!Game.Paused && !PlayerEntity.Dead && DoMouselook && !Game.InCutscene)
        {

            
            pitch -= speedV * Input.GetAxis("Mouse Y");
            pitch = Mathf.Clamp(pitch, -90, 90f);
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            recoil = Mathf.Lerp(recoil, 0, 3 * Time.deltaTime);
            roll = Mathf.Lerp(roll, 0, 0.1f);
            

            
            float extraX = transform.InverseTransformDirection(PlayerBody.velocity).x;
            extraX *= SlideRollMultiplier;
            //if (!PlayerComponent.IsSliding) extraX *= 0;
            transform.eulerAngles = new Vector3(pitch + y * 0.56f + recoil, body.transform.rotation.eulerAngles.y, body.transform.rotation.eulerAngles.z + -roll + -x * 4 + extraX * -3);
            if (Input.GetKeyDown(KeyCode.F1))
            {
                ScreenCapture.CaptureScreenshot("FUCK" + Random.Range(1, 999999) + ".png");
            }
        }
        if(DoSetFOV)
        {
            if (PlayerPrefs.HasKey("FOV"))
            {
                DefaultFOV = PlayerPrefs.GetInt("FOV");
            }
            else
            {
                DefaultFOV = 60;
            }
            
        }

        if(DoFOV)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, DefaultFOV, 0.09f);
        }
        
    }
    public void ApplyScreenPunch(float AddedRecoil = 0)
    {
        recoil = AddedRecoil * -1 + Random.Range(-1, 1);
    }
    public void ApplyCameraFOV(float AddedFOV = 0)
    {
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, cam.fieldOfView + AddedFOV, 0.3f);
    }
    public void ApplyCameraRoll(float AddedRoll = 0)
    {
      roll = Mathf.Lerp(roll, AddedRoll, 0.3f);
    }
}
