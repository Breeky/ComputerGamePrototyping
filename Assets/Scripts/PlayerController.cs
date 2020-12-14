using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private GameManager gameManager;

    private Rigidbody2D _rb;
    
    [Header("Renderer")]
    public ParticleSystem featherBurst;

    [Header("Gravity")] 
    private bool enableGravity;
    private float gravity;
    private Vector3 _gravityDirection;

    [Header("Controls")]
    private float jumpForce;
    public float speed;
    private float throwForce;  // For throwing a projectile
    
    [Header("Others")]
    [SerializeField] private LayerMask groundLayer;
    public Transform planet;
    private GameObject projectile;
    public GameObject projectileRemover;

    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        enableGravity = gameManager.enableGravity;
        gravity = gameManager.gravity;
        jumpForce = gameManager.jumpForce;
        speed = gameManager.speed;
        throwForce = gameManager.throwForce;
        projectile = gameManager.projectile;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        // Get gravity direction
        _gravityDirection = (planet.transform.position - transform.position).normalized;
        
        // Freeze rotation in the right direction
        if (transform.position.x > planet.position.x)
        {
            transform.rotation = Quaternion.Euler(0, 0, -Vector2.Angle(Vector2.up, -_gravityDirection));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, Vector2.Angle(Vector2.up, -_gravityDirection));
        }
        
        // GRAVITY
        if (enableGravity)
            _rb.AddForce(_rb.mass * gravity * _gravityDirection);
        
        // HORIZONTAL MOVEMENT
        Vector2 movementDirection = Vector2.Perpendicular(_gravityDirection).normalized;
        // _rb.AddForce(speed * movementDirection);
        _rb.velocity = new Vector2(
            speed * movementDirection.x,
            speed * movementDirection.y
        );

        // VERTICAL MOVEMENT
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            _rb.AddForce(-jumpForce * _gravityDirection);
        }

        if (Input.GetAxisRaw("Vertical") < 0 && !enableGravity)
        {
            _rb.AddForce(jumpForce * _gravityDirection);
        }
    }

    public void ThrowProjectile(ProjectileController source)
    {
        Transform egg = Instantiate(projectile.transform, transform.position, transform.rotation);
        egg.GetComponent<SpriteRenderer>().color = source.GetComponent<SpriteRenderer>().color;
        Rigidbody2D rb = egg.GetComponent<Rigidbody2D>();
        rb.velocity = _gravityDirection * throwForce;
        
        // Feather burst
        ParticleSystem.MainModule main = featherBurst.main;
        main.startColor = source.GetComponent<SpriteRenderer>().color;
        featherBurst.Play();
    }
}
