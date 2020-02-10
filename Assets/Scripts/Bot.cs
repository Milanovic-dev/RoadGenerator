using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bot : MonoBehaviour
{
    public CarManager CarManager;

    private bool isTouchingGroundLastFrame;
    private bool Rotate;
    private float DesiredX;
    private float LaneSwitchSpeed;

    void Start()
    {
        CarManager.Init(false);
        LaneSwitchSpeed = 30f;
        DesiredX = 6.5f;
    }

    void Update()
    {
        bool isTouchingGround = CarManager.CheckGround(3f);

        if(isTouchingGroundLastFrame != isTouchingGround)
        {
            if(isTouchingGround)
            {
                if(CarManager.Flips >= 1)
                {
                    CarManager.Boost();
                }

                //Choose to rotate (30% Chance)
                Rotate = Random.Range(0, 3) == 0;

                //Choose lane (50% Chance)
                int rand = Random.Range(0, 2);

                if(rand == 0)
                {
                    DesiredX = -6.5f;
                }
                else
                {
                    DesiredX = 6.5f;
                }
            }
        }

        if(isTouchingGround)
        {
            CarManager.MoveSideways(DesiredX, LaneSwitchSpeed);
            CarManager.MoveForward();
        }
        else
        {
            if(Rotate)
            {
                CarManager.Rotate();
            }
        }

        isTouchingGroundLastFrame = isTouchingGround;
    }
}
