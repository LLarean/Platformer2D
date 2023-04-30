using UnityEngine;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
    [SerializeField] private Rigidbody2D _rigidbody2D;
    
    public float MinGroundNormalY = .65f; 
    public float GravityModifier = 1f;
    public Vector2 Velocity;
    public LayerMask LayerMask;
    
    protected Vector2 _targetVelocity;
    protected bool _grounded;
    protected Vector2 _groundNormal;
    protected ContactFilter2D _contactFilter2D;
    protected RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>();

    protected const float _minMoveDistance = .001f;
    protected const float _shellRaidus = .01f;

    private void Start()
    {
        _contactFilter2D.useTriggers = false;
        _contactFilter2D.SetLayerMask(LayerMask);
        _contactFilter2D.useLayerMask = true;
    }

    private void Update()
    {
        _targetVelocity = new Vector2(Input.GetAxis("Horizontal"), 0);

        _grounded = true;
        
        if (Input.GetKey(KeyCode.Space) && _grounded)
        {
            Velocity.y = 5;
        }
    }

    private void FixedUpdate()
    {
        Velocity += GravityModifier * Physics2D.gravity * Time.deltaTime;
        Velocity.x = _targetVelocity.x;

        _grounded = false;

        Vector2 deltaPosition = Velocity * Time.deltaTime;
        Vector2 moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x);
        Vector2 move = moveAlongGround * deltaPosition.x;
        
        Movement(move, false);
        
        move = Vector2.up * deltaPosition.y;
        
        Movement(move, true);
    }

    private void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > _minMoveDistance)
        {
            int count = _rigidbody2D.Cast(move, _contactFilter2D, _hitBuffer, distance + _shellRaidus);
            
            _hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            for (int i = 0; i < _hitBufferList.Count; i++)
            {
                Vector2 currentNormal = _hitBufferList[i].normal;

                if (currentNormal.y > MinGroundNormalY)
                {
                    _grounded = true;

                    if (yMovement)
                    {
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(Velocity, currentNormal);

                if (projection < 0)
                {
                    Velocity = Velocity - projection * currentNormal;
                }

                float modifiedDistance = _hitBufferList[i].distance - _shellRaidus;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }

            _rigidbody2D.position = _rigidbody2D.position + move.normalized * distance;
        }
    }
}
