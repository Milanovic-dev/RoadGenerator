using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NimiMobilePack;

public class PlayerControl : MonoBehaviour
{
    public static PlayerControl instance { get; set; }

    public float GravityScale;
    public CarManager CarManager;
    private Rigidbody Rb;
    private NCameraUtility CameraUtil;
    private NVirtualJoystick Joystick;
    private TrailRenderer Trail;

    private float TimeSinceLanded;

    private Vector3 PlayerPos;
    private bool Input;

    private void Awake()
    {
        instance = this;
        Physics.gravity = Vector3.up * GravityScale;
        Trail = GetComponentInChildren<TrailRenderer>();
        Rb = GetComponent<Rigidbody>();
        CameraUtil = new NCameraUtility(Camera.main, transform);
        Joystick = new NVirtualJoystick();
    }

    // Start is called before the first frame update
    void Start()
    {
        CarManager.Init(true);
        Rb.MovePosition(LevelGenerator.path.GetPointAtDistance(30f));
        Rb.velocity = Vector3.zero;
    }


    private void Update()
    {
        if (NInput.TouchBegan())
        {
            Joystick.SetPivot();
            PlayerPos = Rb.position;
            Input = true;
        }

        CarManager.CountFlips();

        if (NInput.TouchEnded())
        {
            Input = false;
        }

        UIManager.Progress(transform.position.z / LevelGenerator.path.end.z);
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        bool isTouchingGround = CarManager.CheckGround(2.5f);

        if(isTouchingGround)
        {
            TimeSinceLanded += Time.deltaTime;
        }
        else
        {
            TimeSinceLanded = 0f;
        }

        if (Input)
        {
            Joystick.Update();

            float mag = Joystick.GetMagnitude() / 20f;
            float roadWidth = LevelGenerator.instance.RoadWidth / 2f;

            CarManager.MoveSideways(PlayerPos.x + mag);

            if(isTouchingGround)
            {
                CarManager.Torque = 0f;

                if(CarManager.Flips > 0 && TimeSinceLanded < 1.5f) 
                {
                    StartCoroutine(BoostPlayer());
                }

                CarManager.Flips = 0;
                CarManager.MoveForward();               
            }
            else
            {
                CarManager.Rotate();
            }
            
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Road"))
        {
            NLevelManager.RestartScene();
        }
    }

    private IEnumerator BoostPlayer()
    {
        Trail.emitting = true;
        CarManager.Boost();
        yield return new WaitForSecondsRealtime(2f);
        Trail.emitting = false;
    }
  
    private void LateUpdate()
    {
        CameraUtil.Follow(CameraFollowType.ZY_AXIS);
    }
}
