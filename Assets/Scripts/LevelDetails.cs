using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;

public class LevelDetails : MonoBehaviour
{
    public static LevelDetails instance;
    public GameObject Tree;

    private void Awake()
    {
        instance = this;
    }

    //Creates Trees beside the road with "density".
    public static void Create(VertexPath path, float density)
    {
        float length = path.length;

        float distInterval = length / density;

        for (int i = 0; i < density; i++)
        {
            Vector3 offset = Vector3.right * 28f;
            Vector3 pos = path.GetPointAtDistance(distInterval * i) + Vector3.forward * Random.Range(-5f,5f);

            int rand = Random.Range(1, 5);
            for (int j = 0; j < rand; j++)
            {
                offset.x += j * 10f;
                pos += offset;
                Instantiate(instance.Tree, pos, Quaternion.identity);
            }

            rand = Random.Range(1, 5);
            offset = Vector3.left * 28f;
            pos = path.GetPointAtDistance(distInterval * i) + Vector3.forward * Random.Range(-5f, 5f);

            for (int j = 0; j < rand; j++)
            {
                offset.x += -j * 10f;
                pos += offset;
                Instantiate(instance.Tree, pos, Quaternion.identity);
            }
        }
    }
}
