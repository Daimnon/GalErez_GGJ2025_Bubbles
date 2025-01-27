using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Bubble : MonoBehaviour, IFreezable
{
    private const string PLAYER_TAG = "Player";
    private const float NORMAL_CHECK_DISTANCE = 0.75f;
    private const float DOWNBLOWN_CHECK_DISTANCE = 0.4f;

    private BubblePooler _bubblePooler = null;
    public BubblePooler @BubblePoller { get => _bubblePooler; set => _bubblePooler = value; }

    private bool _isFrozen = false;
    public bool IsFrozen { get => _isFrozen; set => _isFrozen = value; }

    private Camera _mainCam;
    private PlayerInputs _player;
    private float _outOfBoundsLimit = 0;

    [Header("Bubble core behaviour")]
    [SerializeField] private float _sideSway = 1.0f;
    [SerializeField] private float _horizontalSpeed = 1.0f;
    [SerializeField] private float _verticalSpeed = 0.5f;
    private Vector3 _startPosition;
    private float _elapsedTime;
    private float _swayDirection = 1.0f;

    [Header("Blow animation config")]
    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private SpriteRenderer _sR;
    [SerializeField] private Animator _animator;
    [SerializeField] private CapsuleCollider2D _idleCollider;
    [SerializeField] private CapsuleCollider2D _underPlayerCollider;
    [SerializeField] private Sprite[] _idleSprites;
    [SerializeField] private float _growthAmount = 20.0f;
    [SerializeField] private float _blowDownTimer = 0.0f;
    [SerializeField] private float _blowDownTime = 3.0f;

    [Header("Pop animation")]
    [SerializeField] private static readonly int _stateNameHash = Animator.StringToHash("Base Layer.StateName");

    [Header("Check Player")]
    [SerializeField] private LayerMask _playerLayer;
    [SerializeField] private float _playerCheckDistance = 0.75f;

    [Header("Sounds")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _blowBubbleClip;
    [SerializeField] private AudioClip[] _waterStepClips;
    [SerializeField] private AudioClip _popBubbleClip;

    private delegate void StateMachine();
    private StateMachine _animationState;
    
    private void Start()
    {
        _mainCam = Camera.main;
        _outOfBoundsLimit = _mainCam.orthographicSize * 2;
    }
    private void OnEnable()
    {
        _animator.enabled = true;
        _animator.SetBool("IsBlown", true);
    }
    private void OnDisable()
    {
        _animator.enabled = false;
        _animator.SetBool("IsPopping", false);
    }

    private void Update()
    {
        if (_isFrozen) return;

        CheckPlayer();

        if (_blowDownTimer > 0) _blowDownTimer -= Time.deltaTime;
    }
    private void FixedUpdate()
    {
        if (_isFrozen) return;

        _animationState.Invoke();
        CheckIfBubbleOutOfRange();
    }

    private void CheckPlayer()
    {
        if (_isFrozen) return;
        Vector2 rayOrigin = transform.position;

        Vector2 rayDirection = Vector2.up;
        float rayDistance = transform.localScale.y * _playerCheckDistance;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (_animator.GetBool("IsPopped"))
        {
            if (stateInfo.normalizedTime >= 0.9f)
            {
                _animator.SetBool("IsPopped", false);
                _bubblePooler.ReturnToPool(this);
                return;
            }
        }
        
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDirection, rayDistance, _playerLayer);
        if (hit.collider != null && hit.collider.CompareTag(PLAYER_TAG) && _animationState != StepForPlayer)
        {
            Debug.Log($"Ray hit detected above the bubble: {hit.collider.name}");
            _startPosition = transform.position;
            _idleCollider.enabled = false;
            _underPlayerCollider.enabled = true;
            _animator.SetBool("IsUnderPlayer", true);
            _animationState = StepForPlayer;
            _audioSource.Stop();
            _audioSource.PlayOneShot(_waterStepClips[Random.Range(0, _waterStepClips.Length)]);
            _player = hit.collider.GetComponent<PlayerInputs>();

            if (_blowDownTimer > 0) //handle downwards blown bubbles
            {
                _player.LandSoftly();
                _playerCheckDistance = DOWNBLOWN_CHECK_DISTANCE;
                _underPlayerCollider.size = new(_underPlayerCollider.size.x, _underPlayerCollider.size.y *2);
                _underPlayerCollider.offset = new(_underPlayerCollider.offset.x, (_underPlayerCollider.offset.y - _underPlayerCollider.size.y/2) /1.5f);
            }
        }
        else if (hit.collider == null && _player && _animationState == StepForPlayer && _blowDownTimer <= 0)
        {
            _animator.SetBool("IsPopped", true);
            _audioSource.Stop();
            _audioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
            _audioSource.PlayOneShot(_popBubbleClip);
            

            _player = null;
        }
    }
    private void CheckIfBubbleOutOfRange()
    {
        Vector3 bubblePosition = transform.position;
        float cameraUpperBound = _mainCam.transform.position.y + _mainCam.orthographicSize;

        if (bubblePosition.y > cameraUpperBound + _outOfBoundsLimit) _bubblePooler.ReturnToPool(this);
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
        if (transform.localScale.x > 0.9f)
        {
            transform.localScale = Vector3.one;
            _startPosition = transform.position;
            _sR.sprite = _idleSprites[0];
            _animationState = Idle;
        }
    }
    private void StepForPlayer()
    {
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (_animator.GetBool("IsPopped"))
        {
            if (stateInfo.normalizedTime >= 0.6f)
            {
                Debug.Log("GotIn");

                _playerCheckDistance = NORMAL_CHECK_DISTANCE;
                _underPlayerCollider.size = Vector2.one;
                _underPlayerCollider.offset = Vector2.zero;
                transform.localScale = new(0.3f, 0.3f, 0.3f);
                _animator.SetBool("IsPopped", false);
                _bubblePooler.ReturnToPool(this);
                return;
            }
            Debug.Log(stateInfo.normalizedTime);
        }

        if (transform.localScale.x < 1.0f)
        {
            _animator.SetBool("IsPopped", true);
            _audioSource.Stop();
            _audioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
            _audioSource.PlayOneShot(_popBubbleClip);
            return;
        }

        _elapsedTime += Time.fixedDeltaTime;
        float verticalOffset = _elapsedTime * _verticalSpeed /2;
        transform.position = _startPosition + new Vector3(0.0f, -verticalOffset /2, 0.0f);
    }

    public void ResetAnimationState()
    {
        _rb2D.velocity = Vector3.zero;
        _elapsedTime = 0;
        _blowDownTimer = 0;
        _startPosition = transform.position;
        _idleCollider.enabled = true;
        _underPlayerCollider.enabled = false;
        _sR.enabled = true; // sprite renderer is being disabled by the pop animation
        _isFrozen = false;
        _animator.SetBool("IsPopped", false);
        _animator.SetBool("IsUnderPlayer", false);
        _animator.SetBool("IsBlown", true);
        _animationState = BlowingBubble;
    }
    public void BlowBubble(Vector2 blowBubbleDirection, float blowForce, bool isMirrored)
    {
        _swayDirection = isMirrored ? 1.0f : -1.0f;

        if (blowBubbleDirection == Vector2.down)
        {
            _rb2D.AddForce(10 * blowForce * blowBubbleDirection, ForceMode2D.Impulse);
            _blowDownTimer = _blowDownTime;
        }
        else _rb2D.AddForce(blowBubbleDirection * blowForce, ForceMode2D.Impulse);
        _animator.SetBool("IsBlown", false);
        _audioSource.Stop();
        _audioSource.pitch = 1.0f + Random.Range(-0.2f, 0.2f);
        _audioSource.PlayOneShot(_blowBubbleClip);
    }

    private void OnDrawGizmos()
    {
        Vector2 rayOrigin = transform.position;

        Vector2 rayDirection = Vector2.up;
        float rayDistance = transform.localScale.y * _playerCheckDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(rayOrigin, rayOrigin + rayDirection * rayDistance);
    }
}
