using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private PlayerControls _controls;
    private InputAction _moveAction, _fireAction, _jumpAction;

    private Vector2 _moveInputValue = Vector2.zero;
    private Vector2 _lastInputValue = Vector2.zero;
    private float _groundCheckRadius;
    private bool _isInputEnabled = true;
    private bool _isMirrored = false;
    private bool _isGrounded = false;

    [Header("Player Config")]
    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private float _speed = 100.0f;
    [SerializeField] private float _acceleration = 20.0f;
    [SerializeField] private float _deceleration = 1.0f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _jumpForce = 10.0f;

    [Header("Bubble Interactions")]
    [SerializeField] private BubblePooler _bubblePooler;
    [SerializeField] private float _blowBubbleForce = 20.0f;

    private void Awake()
    {
        _controls = new();
        InitializeInputs();

        _groundCheckRadius = transform.localScale.y / 1.9f;
    }
    private void OnEnable()
    {
        _moveAction.Enable();
        _fireAction.Enable();
        _jumpAction.Enable();

        _fireAction.performed += Fire;
        _jumpAction.performed += Jump;
    }
    private void OnDisable()
    {
        _moveAction.Disable();
        _fireAction.Disable();
        _jumpAction.Disable();

        _fireAction.performed -= Fire;
        _jumpAction.performed -= Jump;
    }

    private void Update()
    {
        GetMoveVector();
        CheckGrounded();
    }
    private void FixedUpdate()
    {
        Move(_moveInputValue, _rb2D);
    }

    private void InitializeInputs()
    {
        _moveAction = _controls.Player.Move;
        _fireAction = _controls.Player.Fire;
        _jumpAction = _controls.Player.Jump;
    }

    private void CheckGrounded()
    {
        _isGrounded = Physics2D.OverlapCircle(transform.position, _groundCheckRadius, _groundLayer);
    }

    private void GetMoveVector()
    {
        if (!_isInputEnabled) return;

        Vector2 moveVector = _moveAction.ReadValue<Vector2>();
        _moveInputValue = moveVector; // might not be needed
        _lastInputValue = _moveInputValue;
    }
    private void Move(Vector2 moveVector, Rigidbody2D rb2D)
    {
        if (!_isInputEnabled) return;
        if (moveVector != Vector2.zero) _isMirrored = moveVector.x < 0 ? true : false;

        moveVector.y = 0.0f;
        Vector2 moveDirection = moveVector.normalized;
        Vector2 newVelocity = Time.fixedUnscaledDeltaTime * moveDirection;
        Vector2 targetVelocity = newVelocity * _speed;

        float accelerationFactor = moveVector.magnitude > 0 ? _acceleration : _deceleration;
        rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetVelocity, accelerationFactor * Time.fixedUnscaledDeltaTime);

        // add camera movement with up and down
    }
    private void Jump(InputAction.CallbackContext obj)
    {
        if (_isGrounded) _rb2D.velocity = new Vector2(_rb2D.velocity.x, _jumpForce);
    }
    private void Fire(InputAction.CallbackContext obj)
    {
        if (_lastInputValue == Vector2.zero) _lastInputValue = _isMirrored ? Vector2.left : Vector2.right;
        Vector3 newBubblePos = transform.position + (Vector3)_lastInputValue.normalized;
        newBubblePos.z = 0.0f;

        Bubble bubble = _bubblePooler.GetFromPool(newBubblePos);
        bubble.BlowBubble(_lastInputValue, _blowBubbleForce);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _groundCheckRadius);
    }
}