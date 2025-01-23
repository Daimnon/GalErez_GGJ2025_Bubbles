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
        Vector2 moveDirection = moveVector.normalized;

        _moveInputValue = moveVector; // might not be needed
        Vector2 newVelocity = Time.fixedUnscaledDeltaTime * moveDirection;
        rb2D.velocity = newVelocity * _speed;
    }
    private void Fire(InputAction.CallbackContext obj)
    {

    }
}
