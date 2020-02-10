using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CarManager 
{
    public Transform transform => Rb.transform;
    public bool isInFastLane { get { return Rb.position.x < -0.6f; } }
    public float acceleration { get { return isInFastLane ? FastLaneAcceleration : SlowLaneAcceleration; } }
    public bool isTouchingGround { get; set; }


    public Rigidbody Rb;
    public float FastLaneAcceleration;
    public float SlowLaneAcceleration;
    public float MaxTorque;
    public float MinVelocity;
    public float MaxVelocity;
    public float Ray1Pos;
    public float Ray2Pos;

    [HideInInspector]
    public int Flips;
    [HideInInspector]
    public float Torque;
    private float FlipMax;
    private float FlipCountMultiplier;
    private float FlipCounterReference;

    private Vector3 Move;
    private bool IsPlayer;

    public void Init(bool isPlayer)
    {
        FlipMax = 300;
        FlipCountMultiplier = 300;
        IsPlayer = isPlayer;
    }

    public void MoveSideways(float x)
    {
        float roadWidth = LevelGenerator.instance.RoadWidth / 2f;
        x = Mathf.Clamp(x, -roadWidth, roadWidth);
        Move.Set(x, Rb.position.y, Rb.position.z);
        Rb.MovePosition(Move);
    }

    public void MoveSideways(float x, float LerpSpeed)
    {
        float roadWidth = LevelGenerator.instance.RoadWidth / 2f;
        x = Mathf.Clamp(x, -roadWidth, roadWidth);
        Move.Set(x, Rb.position.y, Rb.position.z);
        Rb.MovePosition(Vector3.MoveTowards(Rb.position, Move, Time.deltaTime * LerpSpeed));
    }

    public void MoveForward()
    {
        Rb.AddForce(transform.forward.normalized * acceleration * 30f * Time.deltaTime);
    }

    public void Rotate()
    {
        Torque = Mathf.MoveTowards(Torque, MaxTorque, 450f * Time.deltaTime);
        Rb.angularVelocity = Vector3.right * Torque * Time.deltaTime;
    }

    public void Boost()
    {
        if(Rb.velocity.magnitude < 5f)
        {
            Rb.velocity *= 10f;
        }
        Rb.velocity = (Rb.velocity * Flips) * 0.8f;
    }

    public bool CheckGround(float distance)
    {
        RaycastHit hit1;
        RaycastHit hit2;
        Ray ray1 = new Ray(Rb.position + transform.forward.normalized * Ray1Pos, -transform.up.normalized);
        Ray ray2 = new Ray(Rb.position - transform.forward.normalized * Ray2Pos, -transform.up.normalized);

        bool flag = true;

        if (!Physics.Raycast(ray1, out hit1, distance))
        {
            if (hit1.collider != null)
            {
                if (!hit1.collider.CompareTag("Road"))
                {
                    flag = false;
                }
            }
            else
            {
                flag = false;
            }
        }

        if (!Physics.Raycast(ray2, out hit2, distance))
        {
            if (hit2.collider != null)
            {
                if (!hit2.collider.CompareTag("Road"))
                {
                    flag = false;
                }
            }
            else
            {
                flag = false;
            }
        }

        return flag;
    }

    public void CountFlips()
    {
        if (Rb.angularVelocity.x < -1f)
        {
            FlipCounterReference += Time.deltaTime * FlipCountMultiplier;

            if (FlipCounterReference > FlipMax)
            {
                FlipCounterReference = 0f;
                Flips++;

                if(IsPlayer)
                {
                    UIManager.NumOfFlips(Flips);
                }
            }

        }
    }


}
