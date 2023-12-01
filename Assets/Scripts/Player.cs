using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _mySpeed;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private float _rollSpeed;
    private Rigidbody2D _rb;

    [SerializeField] private float _canAttack = 0;
    [SerializeField] private float _attackRate = 0.3f;

    private Animator _animator;

    [SerializeField] private bool _isGrounded;

    [SerializeField] private bool _isFacingRight;

    private float _lastTapTime = 0;
    private float _tapSpeed = 0.2f;

    [SerializeField] private bool _isClimbing;

    [SerializeField] private CapsuleCollider2D _playerCollider;
    [SerializeField] private BoxCollider2D _playerFeet;

    private int _gravityAtStart = 4;

    [SerializeField] private bool _playerHasBow;
    private bool _canFireArrow;
    [SerializeField] private GameObject _arrowPrefab;

    [SerializeField] private GameObject _bowAndArrow;

    [SerializeField] private Bow _bow;

    [SerializeField] private float _arrowSpeed;

    private int _direction = 1;
    private bool _isJumping;


    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _animator.SetLayerWeight(0, 1);
        _bowAndArrow.SetActive(false);

        _rb.gravityScale = _gravityAtStart;
    }

    void Update()
    {
        if (_isGrounded == true)
        {
            _isClimbing = false;
            _animator.SetBool("isClimbing", false);
            _animator.speed = 1;
        }

        if (_isClimbing == false)
        {
            Jump();
        }

        if (_canFireArrow == true)
        {
            _animator.SetBool("isRunning", false);
        }

        //Roll(); // <------Buggy
        Attack();
        FireArrow();

        if (gameObject.transform.localScale.x > 0)
        {
            _isFacingRight = true;
        }
        else if (gameObject.transform.localScale.x < 0)
        {
            _isFacingRight = false;
        }
    }

    private void FixedUpdate()
    {
        PlayerMovement();
        Climbing();
    }


    private void PlayerMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        if (_canFireArrow == false)
        {
            transform.Translate(Vector3.right * _mySpeed * horizontal * Time.deltaTime);
        }

        if (horizontal > 0)
        {
            if (!_isFacingRight)
            {
                FlipSprite();
            }
            if (_isGrounded == true)
            {
                _animator.SetBool("isRunning", true);
            }
        }
        else if (horizontal < 0)
        {
            if (_isFacingRight)
            {
                FlipSprite();
            }
            if (_isGrounded == true)
            {
                _animator.SetBool("isRunning", true);
            }
        }
        else
        {
            _animator.SetBool("isRunning", false);
        }
    }

    private void FlipSprite()
    {
        Vector3 currentScale = gameObject.transform.localScale;
        currentScale.x *= -1;
        gameObject.transform.localScale = currentScale;

        _isFacingRight = !_isFacingRight;
    }

    private void Jump()
    {
        if (!_playerFeet.IsTouchingLayers(LayerMask.GetMask("Platform")))
        {
            _isGrounded = false;
            _animator.SetBool("isJumping", true);
        }
        else if (_playerFeet.IsTouchingLayers(LayerMask.GetMask("Platform")))
        {
            _isGrounded = true;
            _isClimbing = false;
            _animator.SetBool("isJumping", false);
        }

        if (_isGrounded == true && Input.GetKeyDown(KeyCode.Space))
        {
            _rb.velocity += new Vector2(0f, _jumpHeight);
            _rb.gravityScale = _gravityAtStart / 2;
        }
        else if (Input.GetKeyUp(KeyCode.Space) || _rb.velocity.y < -0.1)
        {
            _rb.gravityScale = _gravityAtStart;
        }
    }

    private void Roll()
    {
        if (Input.GetKeyDown(KeyCode.D) && _isGrounded == true)
        {
            if ((Time.time - _lastTapTime) < _tapSpeed)
            {
                _animator.SetTrigger("isRolling");
                _rb.AddForce(transform.right * _rollSpeed, ForceMode2D.Force);
            }
            _lastTapTime = Time.time;
        }
        if (Input.GetKeyDown(KeyCode.A) && _isGrounded == true)
        {
            if ((Time.time - _lastTapTime) < _tapSpeed)
            {
                _animator.SetTrigger("isRolling");
                _rb.AddForce(-transform.right * _rollSpeed, ForceMode2D.Force);
            }
            _lastTapTime = Time.time;
        }
    }

    private void Attack()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && Time.time > _canAttack)
        {
            if (_isGrounded == true)
            {
                _animator.SetTrigger("Attack_1");
            }
            else
            {
                _animator.SetTrigger("Attack_2"); //JUMP ATTACK
            }
            _canAttack = Time.time + _attackRate;
        }
    }

    private void FireArrow()
    {
        if (_playerHasBow == true)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                _animator.SetBool("isFiringArrow", true);
                StartCoroutine("CanFireArrow");
            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                StopCoroutine("CanFireArrow");
                _animator.SetBool("isFiringArrow", false);

                if (_canFireArrow == true)
                {
                    if (_isFacingRight == true)
                    {
                        _direction = 1;
                    }
                    else if (_isFacingRight == false)
                    {
                        _direction = -1;
                    }
                    GameObject newArrow = Instantiate(_arrowPrefab, transform.position, Quaternion.identity);
                    newArrow.GetComponent<Rigidbody2D>().velocity = _direction * transform.right * _arrowSpeed;
                    _canFireArrow = false;
                }
            }
        }
    }

    IEnumerator CanFireArrow()
    {
        yield return new WaitForSeconds(0.4f);
        _canFireArrow = true;
    }

    void Climbing()
    {
        if (_playerCollider.IsTouchingLayers(LayerMask.GetMask("Climbing")))
        {
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) && !_playerFeet.IsTouchingLayers(LayerMask.GetMask("Platform")))
            {
                _isClimbing = true;
                _isGrounded = false;
                float vertical = Input.GetAxis("Vertical");
                transform.Translate(Vector3.up * vertical * _mySpeed / 2 * Time.deltaTime);
                _rb.gravityScale = _gravityAtStart * 0;
                _animator.speed = 1;
                _animator.SetBool("isClimbing", true);
            }
            else
            {
                if (_isGrounded == false)
                {
                    _animator.speed = 0;
                }
            }
        }
        else
        {
            _animator.SetBool("isClimbing", false);
            _animator.speed = 1;

            if (_isClimbing == true)
            {
                _rb.gravityScale = _gravityAtStart;
            }
            _isClimbing = false;
        }
    }



    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Sword")
        {
            _playerHasBow = false;
            _animator.SetLayerWeight(0, 0f);
            _animator.SetLayerWeight(1, 1f);
            _animator.SetLayerWeight(2, 0f);
            _animator.SetLayerWeight(3, 0f);
            Debug.Log("Picked up Sword!");
        }
        if (other.tag == "Spear")
        {
            _playerHasBow = false;
            _animator.SetLayerWeight(0, 0f);
            _animator.SetLayerWeight(1, 0f);
            _animator.SetLayerWeight(2, 1f);
            _animator.SetLayerWeight(3, 0f);
            Debug.Log("Picked up Spear!");
        }
        if (other.tag == "Bow")
        {
            _playerHasBow = true;
            _animator.SetLayerWeight(0, 0f);
            _animator.SetLayerWeight(1, 0f);
            _animator.SetLayerWeight(2, 0f);
            _animator.SetLayerWeight(3, 1f);
            Debug.Log("Picked up Bow!");
        }
    }


}

