using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour, IFreezable
{
    private const string PLAYER_TAG = "Player";

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

    private delegate void StateMachine();
    private StateMachine _animationState;
    
    private void Start()
    {
        _animationState = BlowingBubble;
    }

    private void FixedUpdate()
    {
        _animationState.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(PLAYER_TAG))
        {
            _animationState = StepForPlayer;
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
    public void BlowBubble(Vector2 blowBubbleDirection, float blowForce)
    {
        _swayDirection = Random.value > 0.5f ? 1.0f : -1.0f;
        _rb2D.AddForce(blowBubbleDirection * blowForce, ForceMode2D.Impulse);
    }
}
