using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NimiMobilePack;
using PathCreation;
using PathCreation.Examples;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public Color color
    {
        get
        {
            return this.MeshRenderer.material.GetColor("_Color");
        }

        set
        {
            this.MeshRenderer.material.SetColor("_Color", value);
        }
    }

    public Color emmision
    {
        get
        {
            return this.MeshRenderer.material.GetColor("_EmissionColor");
        }

        set
        {
            this.MeshRenderer.material.SetColor("_EmissionColor", value);
        }
    }

    public float dist { get { return Dist; } set {  Dist = value; Car.position = Track.GetPointAtDistance(Dist - 4f); } }
    public bool isFewer { get { return NFloatUtility.CompareFloat(Speed, MaxSpeed + FewerBoost); } }
    public float speed { get { return Speed; } set { Speed = value; } }
    public bool doMove { get; set; }
    public Animator animator => Animator;

    public Transform Car;
    public TrailRenderer Trail;
    public float MaxSpeed;
    public float MinSpeed;
    public float Acceleration;
    public float InputSensitivity = 12f;
    public float SideSpeed = 20f;
    public float SideClamp = 3f;
    public float CameraRotationSmoothing = .005f;

    private float Speed;
    private float Dist;
    private float NextSpeedUp;
    private float FewerBoost;

    private VertexPath Track;
    private NCameraUtility CameraUtils;

    private NVirtualJoystick Joystick;
    private Vector3 Move;
    private Transform Player;
    private Vector3 PlayerPos;
    private MeshRenderer MeshRenderer;
    private Animator Animator;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Joystick = new NVirtualJoystick();
        CameraUtils = new NCameraUtility(Camera.main, transform);
        Track = LevelGenerator.path;
        MeshRenderer = GetComponentInChildren<MeshRenderer>();
        Animator = Car.GetComponentInChildren<Animator>();
        Player = transform.GetChild(0);
        Speed = MinSpeed;
        dist = 30f;
        SetPositionOnDistance();
        FewerBoost = 10f;
        doMove = true;
    }

    // Update is called once per frame
    void Update()
    {

        if (!doMove) return;

        if(NInput.TouchBegan())
        {
            Joystick.SetPivot();
            PlayerPos = Player.localPosition;
        }

        if(NInput.TouchEnded())
        {

        }

        if(NInput.IsScreenTouched())
        {
            Speed += Time.deltaTime * Acceleration;
            Joystick.Update();
        }
        else
        {
            Speed -= Time.deltaTime * Acceleration * 2f;
        }
        
        Speed = Mathf.Clamp(Speed, MinSpeed,!IsInFastLane() ? MaxSpeed : MaxSpeed + FewerBoost);
        Dist += Time.deltaTime * Speed;

        if(Dist >= Track.length)
        {
            dist = 30f;
        }

        SetPositionOnDistance();

        if(isFewer)
        {
            Trail.emitting = true;

            if(Time.time > NextSpeedUp)
            {
                FewerBoost += 10f;
                Speed += 10f;
                NextSpeedUp = Time.time + 2f;
            }

        }
        else
        {
            FewerBoost = 20f;
            Trail.emitting = false;
        }

    }

    public void FlipEnd_AnimationEvent()
    {
        Speed = MinSpeed;
        animator.Play("PlayerIdle");
        doMove = true;
    }

    private void SetPositionOnDistance()
    {
        float mag = Joystick.GetMagnitude() / InputSensitivity;
        

        Quaternion rot = Track.GetRotationAtDistance(Dist);
        Player.rotation = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y, 0f);
        transform.rotation = Quaternion.Lerp(transform.rotation, Track.GetRotationAtDistance(Dist), CameraRotationSmoothing * Time.deltaTime);
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x, transform.eulerAngles.y, 0f);

        Move = Track.GetPointAtDistance(Dist) + Vector3.up/10f;
        transform.position = Move;

        Move = PlayerPos + Vector3.right * mag;
        Player.localPosition = Vector3.MoveTowards(Player.localPosition,Move,SideSpeed * Time.deltaTime);
        Player.localPosition = Vector3.ClampMagnitude(Player.localPosition, 3.5f);

        
        Car.position = Vector3.MoveTowards(Car.position, Player.position, Time.deltaTime * Speed);
        Car.LookAt(Player.position);
    }

    public bool IsInFastLane()
    {
        return Player.localPosition.x < 0f;
    }

    private void LateUpdate()
    {
        //CameraUtils.FollowSmoothly(CameraFollowType.ALWAYS_BEHIND,0.05f);
        CameraUtils.Follow(CameraFollowType.NORMAL);
    }

    public static PlayerController GetInstance()
    {
        return instance;
    }
}
