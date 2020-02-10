using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class Traffic : MonoBehaviour
{
    public static Traffic instance { get; set; }

    public GameObject CarPrefab;

    private void Awake()
    {
        instance = this;
    }

    public static void Create(VertexPath path, int density)
    {
        instance.Init(path, density);
    }
    
    private void Init(VertexPath path, int density)
    {
        float length = path.length;

        float distInterval = length / density;

        for(int i = 0; i < density; i++)
        {
            int rand = Random.Range(0, 2);
            float dir = LevelGenerator.instance.RoadWidth / 2.5f;
            if(rand == 1)
            {
                dir = -dir;
            }

            Vector3 offset = new Vector3(dir, 0.2f, 0f);
            Vector3 pos = path.GetPointAtDistance(distInterval * i) + offset;
            Quaternion rot = path.GetRotationAtDistance(distInterval * i);
            GameObject go = Instantiate(CarPrefab, pos, Quaternion.Euler(rot.eulerAngles.x,rot.eulerAngles.y,0f));
            go.AddComponent<EntityMover>().Init(path, distInterval * i, offset, Mathf.Sign(dir), Random.Range(20f, 30f), Random.Range(2f, 6f));
            go.AddComponent<HitTrigger>();
        }
    }
}
