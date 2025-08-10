using System;
using System.Collections;
using UnityEngine;

namespace Game.Scripts.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IsJump = Animator.StringToHash("isJump");
        private static readonly int IsRun = Animator.StringToHash("isRun");
        private static readonly int IsGiveUp = Animator.StringToHash("isGiveUp");
        public PlayerDetection detection;

        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _modifierSpeed = 1f;
        [SerializeField] private float _runModifier = 1f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer;


        [Header("State")]
        private bool _checkDoubleJump = false;
        public bool CheckDoubleJump => _checkDoubleJump;

        [Header("Invincibility")]
        [SerializeField] private GameObject _visualObject;
        [SerializeField] private float _invincibleDuration = 1f;

        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;
        private bool _isGrounded;
        private bool _isStop;
        [SerializeField]  private bool _canRun;
        [SerializeField] private bool _isRunning;
        private int _jumpCount;
        private int _maxJumpCount;

        //  private CameraFollowObject _cameraFollowObject;

        private void Start()
        {
            detection = GetComponent<PlayerDetection>();
            GAME_STAT.PLAYER_SPEED = _moveSpeed;  
            _canRun = false;
        }
        public void UnlockRun()
        {
            _canRun = true;
        }
        private void Update()
        {
            CheckGround();
            _animator.SetBool(IsJump, !_isGrounded);
        }

        public void PlayGiveUpAnimation()
        {
            _animator.SetTrigger(IsGiveUp);
            _isStop = true;
        }

        private void CheckGround()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            if (_isGrounded) _jumpCount = 0;
        }

        public void Move(Vector2 input)
        {
            if (_isStop) return;

            if (float.IsNaN(input.x) || float.IsNaN(input.y)) return; // Không hợp lệ

            _rb.linearVelocity = new Vector2(input.x * ModifierSpeed(),  _rb.linearVelocity.y);
            _animator.SetBool(IsRun, Mathf.Abs(input.x) > 0.01f && _isGrounded);

            if (Mathf.Abs(input.x) > 0.01f)
                transform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);
        }

        public float ModifierSpeed()
        {
            return _moveSpeed * _modifierSpeed * _runModifier;
        }
        public void Run(bool value)
        {
            if (!_canRun) return;
            _runModifier = value ? 1.5f : 1 ;
        }
        public void Jump()
        {
            if (_isStop) return;
            if (_jumpCount <= _maxJumpCount && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
                _jumpCount++;
                if (_checkDoubleJump)
                    StartCoroutine(DoubleJump());
            }
   
        }

        public IEnumerator DoubleJump()
        {
            yield return new WaitForSeconds(0.7f);
            if (!_isGrounded)
            {
                _maxJumpCount++;
                if (_jumpCount <= _maxJumpCount)
                    _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce + 2.5f);
                _maxJumpCount--;
                _checkDoubleJump = false;
            }
        }

        public void StopPlayer()
        {
            _isStop = true;
            GameManager.Instance.IsInputEnable = false;
            _rb.linearVelocityX = 0f;
            _animator.SetBool(IsRun, false);
        }


        
        public void SetModifierSpeed(float mult)
        {
            _modifierSpeed = mult;
            Debug.Log("move speed" + _moveSpeed);
        }

        public void UnStop()
        {
            _isStop = false;
            GameManager.Instance.IsInputEnable = true;
        }

        public void CanDoubleJump()
        {
            _checkDoubleJump = true;
        }

        public void LockDoubleJump()
        {
            _checkDoubleJump = false;
        }
    }


}
