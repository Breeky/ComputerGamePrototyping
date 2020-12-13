using System;
using System.Collections.Generic;
using UnityEngine;

public class CircleSegmentAnimation : MonoBehaviour
{
    public CameraController cameraController;
    
    private string _currentAnimation;
    private CircleSegment _circleSegment;

    [Header("Animation : scaleUp&Down")] 
    public float maxScale;
    public float durationScale;
    private Vector3 _initialScale;
    private Vector3 _targetScale;
    private float _directionScale;

    [Header("Animation : explode")] 
    public float particleSize = 0.2f;

    private void Start()
    {
        _circleSegment = GetComponent<CircleSegment>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (_currentAnimation)
        {
            // Animation when a color change
            // Scale is bumped up
            case "scaleUp&Down":
            {
                if (transform.localScale.x > _targetScale.x)
                {
                    _directionScale = -1f;
                    transform.localScale = _targetScale;
                }
                
                transform.localScale = transform.localScale + _directionScale * ((_targetScale.x - _initialScale.x) * Time.deltaTime /
                    (durationScale / 2)) * Vector3.one;

                // Stopping condition
                if (transform.localScale.x < _initialScale.x && _targetScale.x > _initialScale.x)
                {
                    _currentAnimation = "";
                    transform.localScale = _initialScale;
                    GetComponent<CircleSegment>().EnableCollider(true);
                } 
                break;
            }
        }
    }

    public void PlayAnimation(string animationName)
    {
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
        }
        
        if (animationName == "scaleUp&Down")
        {
            _initialScale = transform.localScale;
            _targetScale = _initialScale * maxScale;
            _circleSegment.EnableCollider(false);
            _directionScale = 1f;
            // Play sound
            FindObjectOfType<AudioManager>().PlaySound("Wobble");
            // Start animation next frame
            _currentAnimation = "scaleUp&Down";
        }

        if (animationName == "explode")
        {
            // Get particle systems
            List<ParticleSystem> systems = new List<ParticleSystem>();
            for (int i = 0; i < transform.childCount; i++)
            {
                ParticleSystem ps = transform.GetChild(i).GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    systems.Add(ps);
                }
            }

            // Init particles positions & scale & color with collider points
            float np = systems.Count;
            float n = _circleSegment.polygonCollider.points.Length;
            for (int i = 0; i < np; i++)
            {
                // Scale
                systems[i].transform.localScale = particleSize * transform.localScale;
                
                // Position
                Vector2 pos = _circleSegment.polygonCollider.points[Mathf.FloorToInt((n/2)*i/np)];
                systems[i].transform.localPosition = pos;

                // Color
                ParticleSystem.MainModule main = systems[i].main;
                main.startColor = _circleSegment.oldColor;
            }

            // Play animation
            FindObjectOfType<AudioManager>().PlaySound("Bounce");
            foreach (ParticleSystem ps in systems)
            {
                ps.Play();
            }
            cameraController.LittleShake();
        }
    }
}
