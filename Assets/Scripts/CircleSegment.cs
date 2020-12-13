using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleSegment : MonoBehaviour
{
    private GameManager gameManager;

    public PolygonCollider2D polygonCollider;
    private SpriteRenderer _spriteRenderer;
    private CircleSegmentAnimation _animator;
    
    [SerializeField] private int numPoints;
    [SerializeField] private float offset;
    
    private float _innerRadius;
    private float _outerRadius;
    private float _startAngle;
    private float _endAngle;

    // To keep track of the location of the circle Segment
    private int _slice;
    private int _layer;
    
    // To know which color corresponds to a disabled circlesegment
    private Color _disabledColor;
    public Color oldColor = Color.black;

    public void Initialize(float innerRadius, float outerRadius, float startAngle, float endAngle, int slice, int layer)
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();

        // Init variables (Do not put this in start because of order of execution with CircleSegmentManager's start function)
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<CircleSegmentAnimation>();
        _disabledColor = gameManager.segmentColors[0];
        
        // Init GFX and collider
        float angleOffset = Mathf.Asin(offset / innerRadius);
        _innerRadius = innerRadius + offset;
        _outerRadius = outerRadius - offset;
        _startAngle = startAngle+angleOffset;
        _endAngle = endAngle-angleOffset;
        _slice = slice;
        _layer = layer;
        
        polygonCollider = GetComponent<PolygonCollider2D>();
        
        CreateCustomCollider();
    }

    void CreateCustomCollider()
    {
        Vector2[] points = new Vector2[2*(numPoints+1)];
        float unitAngle = (_endAngle - _startAngle) / numPoints;
        float angle = _startAngle;
        for(int i = 0; i <= numPoints; i++)
        {
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _innerRadius;
            angle += unitAngle;
        }
        for(int i = numPoints+1; i <= 2*numPoints+1; i++)
        {
            angle -= unitAngle;
            points[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _outerRadius;
        }

        polygonCollider.points = points;
    }


    // Collider only with first segment encountered for now
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Projectile"))
        {   

            if (!other.GetComponent<TransformedProjectileController>().WillItHitBossSoon()){

                // Access to circleSegmentManager attributes
                CircleSegmentManager circleSegmentManager = GameObject.Find("Planet Bottom").GetComponent<CircleSegmentManager>(); 

                // ----- Case when a collision has to be handled ----- //
                Vector2Int coordinatesToRetrieve = other.GetComponent<TransformedProjectileController>().GetNextCollisionCoords();
                if (coordinatesToRetrieve.x >= 0){

                    // Handle collision only if it is the right segment
                    GameObject segmentToRetrieve = other.GetComponent<TransformedProjectileController>().GetNextCollision();
                    if (segmentToRetrieve == this.gameObject){
                        
                        // Change colour of block by colour of ball
                        circleSegmentManager.segmentsOrdered[coordinatesToRetrieve.x, coordinatesToRetrieve.y].ChangeColor(other.GetComponent<SpriteRenderer>().color);

                        // Update color Array
                        circleSegmentManager.colorBlocks[coordinatesToRetrieve.x, coordinatesToRetrieve.y] = other.GetComponent<SpriteRenderer>().color;

                        // Ball disppear at the good spot
                        Destroy(other.gameObject);

                        // Manage matching of blocks
                        circleSegmentManager.ManageMatching(coordinatesToRetrieve.x, coordinatesToRetrieve.y);
                    }
                }
                
                // ----- Case when collision is not yet handled ----- //
                else {
                    
                    bool collision_handled = false; 

                    // Update variables for collision
                    // If the first layer encountered is coloured (not of the color describing an empty block), then the player loses
                    if (circleSegmentManager.segmentsOrdered[_slice, gameManager.nLayer - 1].GetColor() != gameManager.segmentColors[0]) {
                        
                        collision_handled = true;
                        Destroy(other.gameObject);

                        // Player looses
                        gameManager.PlayerLoses();
                        

                    // Normal case
                    } else if (!collision_handled) {

                        // Loop on all segments (layer) of the current slice to find the uncolored segment that is the closest one
                        for (int i = gameManager.nLayer - 2; i >= 0; i--) {
                            if (!collision_handled && circleSegmentManager.segmentsOrdered[_slice, i].GetColor() != gameManager.segmentColors[0]) {
                                
                                // Handle the collision immediately if it has too
                                if (i == gameManager.nLayer - 2){
                                    // Change colour of block by colour of ball
                                    circleSegmentManager.segmentsOrdered[_slice, i+1].ChangeColor(other.GetComponent<SpriteRenderer>().color);
                                                        
                                    // Update color Array
                                    circleSegmentManager.colorBlocks[_slice, i+1] = other.GetComponent<SpriteRenderer>().color;
                                    
                                    Destroy(other.gameObject);

                                    // Manage matching of blocks
                                    circleSegmentManager.ManageMatching(_slice, i+1);
                                } else {
                                    // Indicate the corresponding layer to game object to handle collision at the right spot
                                    other.GetComponent<TransformedProjectileController>().SetNextCollision(new Vector2Int(_slice, i+1), circleSegmentManager.segmentsOrdered[_slice, i+1].gameObject);
                                }
                                
                                collision_handled = true;
                            }
                        }
                        
                        // If the collision is not yet managed, it means the projectile encountered only empty blocks and will hit the boss
                        if (!collision_handled) {
                                
                            // Indicate to not search for colission in puzzle anymore for current egg as it will hit the boss
                            other.GetComponent<TransformedProjectileController>().WillHitBossSoon();
                            collision_handled = true;

                            // Boss lose a life point and update to phase of Boss fight, destruction of projectile are managed in PlanetCoreGFX
                            //gameManager.OnHitBoss();                          
                        }
                    }
                }           
            }
        }
    }

    public Color GetColor()
    {
        return _spriteRenderer.color;
    }

    public void ChangeColor(Color color, bool playAnimation = true)
    {
        _spriteRenderer.color = color;
        
        if (color != _disabledColor)
        {
            oldColor = color;
            if (playAnimation)
                _animator.PlayAnimation("scaleUp&Down");
        }
        else
        {
            if (playAnimation)
                _animator.PlayAnimation("explode");
        }
    }

    public void EnableCollider(bool enable)
    {
        // TODO: verify that it doesn't break the game
        polygonCollider.enabled = enable;
    }
}
