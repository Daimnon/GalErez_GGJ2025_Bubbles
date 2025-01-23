using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private Controls _controls;
    private InputAction _moveAction, _fireAction;

    private Vector2 _moveInputValue = Vector2.zero;
    private bool _isInputEnabled = true;

    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private float _speed = 100.0f;
    [SerializeField] private float _acceleration = 20.0f;
    [SerializeField] private float _deceleration = 1.0f;

    private void Awake()
    {
        _controls = new();
        InitializeInputs();
    }
    private void OnEnable()
    {
        _moveAction.Enable();
        _fireAction.Enable();

        _fireAction.performed += Fire;
    }
    private void OnDisable()
    {
        _moveAction.Disable();
        _fireAction.Disable();

        _fireAction.performed -= Fire;
    }

    private void FixedUpdate()
    {
        Move(_rb2D);
    }

    private void InitializeInputs()
    {
        _moveAction = _controls.Player.Move;
        _fireAction = _controls.Player.Fire;
    }

    private void Move(Rigidbody2D rb2D)
    {
        if (!_isInputEnabled) return;

        Vector2 moveVector = _moveAction.ReadValue<Vector2>();
        _moveInputValue = moveVector; // might not be needed

        moveVector.y = 0.0f;
        Vector2 moveDirection = moveVector.normalized;
        Vector2 newVelocity = Time.fixedUnscaledDeltaTime * moveDirection;
        Vector2 targetVelocity = newVelocity * _speed;

        float accelerationFactor = moveVector.magnitude > 0 ? _acceleration : _deceleration;
        rb2D.velocity = Vector2.Lerp(rb2D.velocity, targetVelocity, accelerationFactor * Time.fixedUnscaledDeltaTime);

        // add camera movement with up and down
    }

    private void Fire(InputAction.CallbackContext obj)
    {

    }
}
