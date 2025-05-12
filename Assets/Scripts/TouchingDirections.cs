using Animation;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D), typeof(Animator))]
public class TouchingDirections : MonoBehaviour
{
    [Header("Filter Settings")]
    [Tooltip("Filter for determining valid collisions")]
    public ContactFilter2D contactFilter;

    [Header("Detection Distances")]
    [Tooltip("Distance to check for ground contact")]
    [SerializeField] private float groundDistance = 0.05f;
    [Tooltip("Distance to check for wall contact")]
    [SerializeField] private float wallDistance = 0.05f;
    [Tooltip("Distance to check for ceiling contact")]
    [SerializeField] private float ceilingDistance = 0.05f;

    [Header("Enemy Detection")]
    [Tooltip("Layer mask for enemy collisions")]
    [SerializeField] private LayerMask enemy;
    [Tooltip("Distance to check for enemy collision (using an OverlapBox)")]
    [SerializeField] private Vector2 enemyCheckSize = new Vector2(0.5f, 0.5f);

    [Header("Debug Visualization")]
    [SerializeField] private bool showDebugRays = true;

    [Header("Wall Detection")]
    public bool disableWallDetection;

    private Rigidbody2D _rb;
    private Animator _animator;
    private Collider2D _col;

    // Arrays for ground, wall, and ceiling detection
    private readonly RaycastHit2D[] _groundHits = new RaycastHit2D[10];
    private readonly RaycastHit2D[] _wallHits = new RaycastHit2D[10];
    private readonly RaycastHit2D[] _ceilingHits = new RaycastHit2D[10];

    // Automatic property handling with change detection
    private bool _isGrounded;
    public bool IsGrounded
    {
        get
        {
            return _isGrounded;
        }
        private set
        {
            if (_isGrounded == value) return;
            _isGrounded = value;
            if (_animator) _animator.SetBool(AnimationStrings.IsGrounded, value);
        }
    }

    private bool _isOnWall;
    public bool IsOnWall
    {
        get
        {
            return _isOnWall;
        }
        private set
        {
            if (_isOnWall == value) return;
            _isOnWall = value;
            if (_animator) _animator.SetBool(AnimationStrings.IsOnWall, value);
        }
    }

    // New property for enemy detection
    public bool IsTouchingEnemy { get; private set; }

    // Cache wall check direction based on local scale
    private Vector2 WallCheckDirection
    {
        get
        {
            return transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _col = GetComponent<Collider2D>();

        if (_col == null)
        {
            Debug.LogError($"No Collider2D attached to {gameObject.name}", gameObject);
        }
    }

    private void FixedUpdate()
    {
        PerformGroundCheck();
        PerformWallCheck();
        PerformEnemyCheck();

        if (showDebugRays)
        {
            DrawDebugRays();
        }
    }

    private void PerformGroundCheck()
    {
        var hitCount = _rb.Cast(Vector2.down, contactFilter, _groundHits, groundDistance);
        IsGrounded = hitCount > 0;
    }

    private void PerformWallCheck()
    {
        // Calculate an offset upward – adjust 0.5f as needed.
        Vector2 offset = Vector2.up * (_col.bounds.extents.y * 0.5f);
        // You can use a Raycast from an adjusted origin instead of rb.Cast.
        RaycastHit2D hit = Physics2D.Raycast((Vector2)_col.bounds.center + offset, WallCheckDirection, wallDistance, contactFilter.layerMask);
        IsOnWall = (hit.collider is not null);
    }

    
    // New method to check for enemy collisions using an OverlapBox
    private void PerformEnemyCheck()
    {
        // Adjust the center of the box if needed (here we use the collider's bounds)
        Vector2 boxCenter = _col.bounds.center;
        Collider2D enemyCollider = Physics2D.OverlapBox(boxCenter, enemyCheckSize, 0f, enemy);
        IsTouchingEnemy = enemyCollider is not null;
    }

    private void DrawDebugRays()
    {
        // Ground check
        Debug.DrawRay(_col.bounds.center, Vector2.down * (_col.bounds.extents.y + groundDistance), Color.green);
        // Wall check
        Debug.DrawRay(_col.bounds.center, WallCheckDirection * (_col.bounds.extents.x + wallDistance), Color.blue);
        // Ceiling check
        Debug.DrawRay(_col.bounds.center, Vector2.up * (_col.bounds.extents.y + ceilingDistance), Color.red);

        // Draw enemy check box (for visualization)
        Vector3 enemyBoxSize = new Vector3(enemyCheckSize.x, enemyCheckSize.y, 0f);
        Debug.DrawLine(_col.bounds.center - enemyBoxSize * 0.5f, _col.bounds.center + enemyBoxSize * 0.5f, Color.magenta);
    }
}
