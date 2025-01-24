using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour, IFreezable
{
    private BubblePooler _bubblePooler = null;
    public BubblePooler @BubblePoller { get => _bubblePooler; set => _bubblePooler = value; }

    [Header("Bubble core behaviour")]
    [SerializeField] private float _sideSway = 1f;
    [SerializeField] private float _horizontalSpeed = 1f;
    [SerializeField] private float _verticalSpeed = 0.5f;
    private Vector3 _startPosition;
    private float _elapsedTime;
    private float _swayDirection = 1.0f;

    [Header("Blow animation config")]
    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private float _growthAmount = 20.0f;

    [Header("Check Player")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _playerCheckOffset = 2.0f;
    [SerializeField] private float _playerCheckRadius = 3.0f;

    private delegate void StateMachine();
    private StateMachine _animationState;
    
    private void Start()
    {
        _animationState = BlowingBubble;
    }

    private void Update()
    {
        CheckPlayer();
    }
    private void FixedUpdate()
    {
        _animationState.Invoke();
    }
    private void CheckPlayer()
    {
        Vector2 circleOrigin = transform.position + Vector3.up * (transform.localScale.y / _playerCheckOffset);
        float radius = transform.localScale.y / _playerCheckRadius;

        Collider2D hit = Physics2D.OverlapCircle(circleOrigin, radius, _playerLayer);

        if (hit != null && _animationState != StepForPlayer)
        {
            Debug.Log($"Hit detected above the bubble: {hit.name}");
            _startPosition = transform.position;
            _animationState = StepForPlayer;
        }
        else if (hit == null && _animationState == StepForPlayer)
        {
            // do pop animation
            _bubblePooler.ReturnToPool(this);
        }
    }

    private void Idle()
    {
        _elapsedTime += Time.fixedDeltaTime;

        float horizontalOffset = Mathf.Sin(_elapsedTime * _horizontalSpeed) * _sideSway * _swayDirection;
        float verticalOffset = _elapsedTime * _verticalSpeed;

        transform.position = _startPosition + new Vector3(horizontalOffset, verticalOffset, 0.0f);
    }
    private void BlowingBubble()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one, _growthAmount * Time.fixedUnscaledDeltaTime);
        if (transform.localScale == Vector3.one)
        {
            _startPosition = transform.position;
            _animationState = Idle;
        }
    }
    private void StepForPlayer()
    {
        _elapsedTime += Time.fixedDeltaTime;
        float verticalOffset = _elapsedTime * _verticalSpeed;

        transform.position = _startPosition + new Vector3(0.0f, -verticalOffset /2, 0.0f);
    }

    public void BlowBubble(Vector2 blowBubbleDirection, float blowForce, bool isMirrored)
    {
        _swayDirection = isMirrored ? 1.0f : -1.0f;
        _rb2D.AddForce(blowBubbleDirection * blowForce, ForceMode2D.Impulse);
    }

    private void OnDrawGizmos()
    {
        Vector2 circleOrigin = transform.position + Vector3.up * (transform.localScale.y / _playerCheckOffset);
        float radius = transform.localScale.y / _playerCheckRadius;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(circleOrigin, radius);
    }
}
