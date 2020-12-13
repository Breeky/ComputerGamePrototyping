using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetCoreGFX : MonoBehaviour
{
    public GameObject player;
    public Sprite[] sprites;
    private int spriteID = 0;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _gravityDirection;

    private Color c1 = Color.black;
    private Color c2 = new Color(165, 42, 42); //brown
    LineRenderer lineRenderer;

    private GameManager gameManager;

    private void Start()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        _spriteRenderer.sprite = sprites[spriteID];

        //Generate leach
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = 0.1f;
        // A simple 2 color gradient with a fixed alpha of 1.0f.
        float alpha = 1.0f;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(c1, 1.0f), new GradientColorKey(c2, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(alpha, 1.0f), new GradientAlphaKey(alpha, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
        lineRenderer.sortingOrder = 101;
    }

    private void Update()
    {
        _gravityDirection = player.transform.position - transform.position;
        
        // Angle & orientation
        float angle = Vector2.Angle(Vector2.up, _gravityDirection);
        if (player.transform.position.x < transform.position.x)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, angle-90);
            _spriteRenderer.flipX = true;
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 90-angle);
            _spriteRenderer.flipX = false;
        }

        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, gameManager.follower.transform.position);
    }

    // Destroy a projectile hitting the core
    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Projectile"))
        {  
            Destroy(other.gameObject);
            // Todo: animate/sound when receiving projectile ?
            gameManager.OnHitBoss();
            spriteID ++;
            _spriteRenderer.sprite = sprites[spriteID];
            
        }
    }
}
