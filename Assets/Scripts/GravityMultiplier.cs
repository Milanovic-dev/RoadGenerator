using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityMultiplier : MonoBehaviour
{
    public float Multiplier = 1.5f;
    private Rigidbody Rb;

    // Start is called before the first frame update
    void Awake()
    {
        Rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Rb.velocity.y < 0f)
        {
            Rb.velocity += Vector3.up * Physics.gravity.y * (Multiplier - 1) * Time.deltaTime;
        }
    }
}
