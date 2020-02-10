using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
public class EntityMover : MonoBehaviour
{
    public VertexPath path { get; set; }
    public float direction { get; set; }
    public float speed { get; set; }
    public float dist { get; set; }
    public float accelerationRate { get; set; }
    public Vector3 offset { get; set; }
    public bool doMove { get; set; }

    private Quaternion rot;
    private float nextSpeedUp;
    private float baseSpeed;

    private void Start()
    {
        doMove = true;
    }

    void Update()
    {

        if (!doMove) return;

        if(Time.time > nextSpeedUp)
        {
            if(speed != baseSpeed)
            {
                speed = baseSpeed;
            }
            else
            {
                speed += Random.Range(-5f, 15f);
            }
            nextSpeedUp = Time.time + accelerationRate;
        }

        dist += Time.deltaTime * speed * direction;
        transform.position = path.GetPointAtDistance(dist) + offset;
        rot = path.GetRotationAtDistance(dist);
        transform.rotation = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y , 0f);
    }

    public void Init(VertexPath path,float dist, Vector3 offset, float direction, float speed, float rate)
    {
        this.path = path;
        this.offset = offset;
        this.dist = dist;
        this.direction = direction;
        this.speed = speed;
        this.baseSpeed = speed;
        this.accelerationRate = rate;
    }
}
