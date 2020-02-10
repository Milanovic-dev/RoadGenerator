using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NRotator : MonoBehaviour
{
    public Vector3 Rotation;
    public bool StartRandom;

    private void Awake()
    {
        if (StartRandom)
        {
            transform.Rotate(Vector3.up * Random.Range(0f, 360f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Rotation * Time.deltaTime);
    }
}
