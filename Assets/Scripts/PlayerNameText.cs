using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerNameText : MonoBehaviour
{
    public Transform FollowTarget;
    public string Text;
    public TextMeshPro TextMesh;

    private void Start()
    {
        TextMesh.text = Text;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = FollowTarget.position + Vector3.up * 8f;
    }
}
