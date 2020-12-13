using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowingOrbGFX : MonoBehaviour
{
    public Color color = Color.red;

    private ParticleSystem _circle;
    private ParticleSystem _particles;
    private ParticleSystem _smoke;

    private void Start()
    {
        _circle = transform.Find("Circle").GetComponent<ParticleSystem>();
        _particles = transform.Find("Particles").GetComponent<ParticleSystem>();
        _smoke = transform.Find("Smoke").GetComponent<ParticleSystem>();
        SetColor(color);
    }

    public void SetColor(Color c)
    {
        color = c;

        ParticleSystem.MainModule main = _circle.main;
        main.startColor = color;
        
        main = _particles.main;
        main.startColor = color;
        
        main = _smoke.main;
        main.startColor = color;
    }
}
