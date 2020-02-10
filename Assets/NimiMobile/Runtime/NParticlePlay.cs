using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NParticlePlay : MonoBehaviour
{
    [Header("Particle Systems")]
    public ParticleSystem[] Particles;
    [Header("Colors"), Space]
    public Color[] Colors;

    public void Play()
    {
        for (int i = 0; i < Particles.Length; i++)
        {
            Particles[i].Stop();
            Particles[i].Clear();
            Particles[i].Play();
        }
    }


    [System.Obsolete]
    public void Play(Vector3 Position, bool randomColor = false)
    {
        transform.position = Position;
        for (int i = 0; i < Particles.Length; i++)
        {
            if (randomColor)
            {
                Particles[i].startColor = GetRandomColor();
            }
            Particles[i].Stop();
            Particles[i].Clear();
            Particles[i].Play();
        }
    }


    Color GetRandomColor()
    {
        return Colors[Random.Range(0, Colors.Length)];
    }
}
