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


        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _modifierSpeed = 1f;
        [SerializeField] private float _runModifier = 1f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer;


        [Header("State")]
        public bool IsSignaling = false;
        private bool _checkDoubleJump = false;


        [Header("Invincibility")]
        [SerializeField] private GameObject _visualObject;
        [SerializeField] private float _invincibleDuration = 1f;

        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;
        private bool _isGrounded;
        private bool _isInvincible;
        private bool _isPaused;
        private bool _endCoroutine;
        [SerializeField]  private bool _canRun;
        [SerializeField] private bool _isRunning;
        private int _jumpCount;
        private int _maxJumpCount;

        //  private CameraFollowObject _cameraFollowObject;

        private void Start()
        {
            GAME_STAT.PLAYER_SPEED = _moveSpeed;  
            _canRun = false;
        }
        public void UnlockRun()
        {
            _canRun = true;
        }
        private void Update()
        {
            //if (GameManager.Instance.IsGameLose) return;
            DetectEnemy();
            CheckGround();
            _animator.SetBool(IsJump, !_isGrounded);
        }

        public void PlayGiveUpAnimation()
        {
            _animator.SetTrigger(IsGiveUp);
            _isPaused = true;
        }

        private void CheckGround()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            if (_isGrounded) _jumpCount = 0;
        }

        public void Move(Vector2 input)
        {
            if (_isPaused) return;

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
            if (_isPaused) return;
            if (_jumpCount <= _maxJumpCount && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
                _jumpCount++;
            }
            if (_checkDoubleJump)
                StartCoroutine(DoubleJump());
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
                //IsSignaling = false;
                //GameManager.Instance.IsWin = false;
                _checkDoubleJump = false;
            }
        }

        public void StartInvincibility()
        {
            if (_isInvincible) return;
            StartCoroutine(InvincibilityCoroutine());
        }

        private IEnumerator InvincibilityCoroutine()
        {
            _isInvincible = true;
            float timer = 0f;
            bool visible = true;
            while (timer < _invincibleDuration)
            {
                visible = !visible;
                if (_visualObject != null) _visualObject.SetActive(visible);
                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }
            if (_visualObject != null) _visualObject.SetActive(true);
            _isInvincible = false;
        }

        private void DetectEnemy()
        {
            float detectRadius = 3.5f;
            LayerMask enemyLayer = LayerMask.GetMask("Flying");
            DrawDebugCircle(transform.position, detectRadius, Color.red);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);

            Debug.Log($"Detected {hits.Length} enemies within radius {detectRadius}");

            foreach (var hit in hits)
            {
                if (hit.CompareTag("Enemy"))
                {

                    HandleEnemyDetection(hit.GetComponent<EnemyController>());

                }
                else if (hit.CompareTag("Bird"))
                {
                    HandleBirdDetection(hit.GetComponent<BirdController>());
                }
            }
        }

        private void HandleEnemyDetection(EnemyController enemy)
        {
            if (enemy == null) return;
            else if (!IsSignaling)
            {
                var distance = Vector2.Distance(transform.position, enemy.transform.position);
                Debug.Log(""+ enemy.IsMovingInDirection);    
                if ((!enemy.IsMovingInDirection && enemy.enabled))
                {
                    Debug.Log("Enemy is not signaling, stopping player movement.");
                    StopPlayer();
                    IsSignaling = enemy.SignalRandomDirection();
                }
            } else
            if (GameManager.Instance.IsWin && IsSignaling)
            {
                enemy.StopMovement();
				enemy.SetInactiveAngryMood();
				enemy.SetActiveMood();             
				GameManager.Instance.IsWin = false;
                IsSignaling = false;
            }
        }

        private void HandleBirdDetection(BirdController bird)
        {
            Debug.Log("Detected Bird");
            if (bird == null) return;
            if (!IsSignaling)
            {
                Debug.Log("Bird is not signaling" + 
                    bird.IsMoving);
                if (!bird.IsMoving && bird.enabled)
                {
                    StopPlayer();
                    IsSignaling = bird.SignalRandomDirection();
                }
            } else
            if (GameManager.Instance.IsWin && IsSignaling)
            {
                _checkDoubleJump = true;
                //bird.SetInactiveMood();
                bird.SetActiveMood();
                bird.StopMovement();
                IsSignaling = false;
                GameManager.Instance.IsWin = false;
            }
            //else if (!GameManager.Instance.IsWin && IsSignaling)
            //{
            //    if (!_endCoroutine && bird.enabled)
            //        StopPlayer();
            //}

        }

        private void DrawDebugCircle(Vector3 center, float radius, Color color, int segments = 32)
        {
            float angle = 0f;
            Vector3 lastPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            for (int i = 1; i <= segments; i++)
            {
                angle += 2 * Mathf.PI / segments;
                Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                Debug.DrawLine(lastPoint, nextPoint, color, 0.01f);
                lastPoint = nextPoint;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Add trigger logic here if needed
        }


        private void StopPlayer()
        {
          
            GameManager.Instance.IsInputEnable = false;
            //if (!_groundCheck)
                //_rb.linearVelocityY = 10f;
            _rb.linearVelocityX = 0f;
            _animator.SetBool(IsRun, false);
           
        }



        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }
        public void SetModifierSpeed(float mult)
        {
            _modifierSpeed = mult;
            Debug.Log("move speed" + _moveSpeed);
        }
        /* private void TurnCheck()
         {
             if (Input.x > 0 && !IsFacingRight)
             {
                 Turn();
             }
             else if(Input.x > 0 && IsFacingRight)
             {
                 Turn();
             }
         }

         private void Turn()
         {
             if (IsFacingRight)
             {
                 Vector3 rotator = new Vector3(transform.rotation.x,180f, transform.rotation.z);
                 transform.rotation = Quaternion.Euler(rotator);
                 IsFacingRight = !IsFacingRight;
                 _cameraFollowObject.CallTurn();
             }
             else
             {
                 Vector3 rotator = new Vector3(transform.rotation.x, 0f, transform.rotation.z);
                 transform.rotation = Quaternion.Euler(rotator);
                 IsFacingRight = !IsFacingRight;
                 _cameraFollowObject.CallTurn();
             }
         }

         private void FixedUpdate()
         {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x,Mathf.Clamp( -_maxFallSpeed, _maxFallSpeed *5));

             if (moveInput > 0 || moveInput < 0)  
             {
                 TurnCheck();
             }
         }*/
        //  }




    }


}