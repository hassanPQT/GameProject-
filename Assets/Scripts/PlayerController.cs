using System.Collections;
using UnityEngine;

namespace Game.Scripts.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IsJump = Animator.StringToHash("isJump");
        private static readonly int IsRun = Animator.StringToHash("isRun");


        [Header("Movement")]
        [SerializeField] private float _moveSpeed = 5f;
        [SerializeField] private float _jumpForce = 10f;
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer;

        /* [Header("Camera Stuff")]
         [SerializeField] private GameObject _cameraFollowGO;
 */

        [Header("State")]
        public bool IsSignaling = false;
        private bool _checkDoubleJump = false;


        [Header("Invincibility")]
        [SerializeField] private GameObject _visualObject;
        [SerializeField] private float _invincibleDuration = 1f;

        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private Animator _animator;
        private bool _isGrounded;
        private int _jumpCount;
        private int _maxJumpCount;
        private bool _isInvincible;
        private bool _isPaused;
        private bool _endCoroutine;


        //  private CameraFollowObject _cameraFollowObject;

        private void Start()
        {
            // _cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowObject>();
           
        }


        private void Update()
        {
            DetectEnemy();
            CheckGround();
            _animator.SetBool(IsJump, !_isGrounded);
        }

        private void CheckGround()
        {
            _isGrounded = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            if (_isGrounded) _jumpCount = 0;
        }

        public void Move(Vector2 input)
        {
            if (_isPaused) return;
            _rb.linearVelocity = new Vector2(input.x * _moveSpeed, _rb.linearVelocity.y);
            _animator.SetBool(IsRun, Mathf.Abs(input.x) > 0.01f && _isGrounded);

            if (Mathf.Abs(input.x) > 0.01f)
                transform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);
        }

        public void Jump()
        {
            if (_isPaused) return;
            if (_jumpCount <= _maxJumpCount && _isGrounded)
            {
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, _jumpForce);
                _jumpCount++;
            }
            if (GameManager.Instance.IsWin && IsSignaling && _checkDoubleJump)
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
                IsSignaling = false;
                GameManager.Instance.IsWin = false;
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
            if (GameManager.Instance.IsWin && IsSignaling)
            {
                enemy.StopMovement();
				enemy.SetInactiveAngryMood();
				enemy.SetActiveMood();             
				GameManager.Instance.IsWin = false;
                IsSignaling = false;
            }
            else if(!GameManager.Instance.IsWin && IsSignaling)
            {
                if (!_endCoroutine && enemy.enabled)
                    StopPlayer();
            }
            else if (!IsSignaling)
            {
                if (!_endCoroutine && enemy.enabled)
                    StopPlayer();
                enemy.OnSignalDirection += GameManager.Instance.OnEnemySignal;
                IsSignaling = enemy.SignalRandomDirection();
            }
        }

        private void HandleBirdDetection(BirdController bird)
        {
            if (bird == null) return;
            if (GameManager.Instance.IsWin && IsSignaling)
            {
                _checkDoubleJump = true;
                bird.FlyIntoPlayer();
                bird.SetActiveMood();
				bird.StopMovement();
            }
            else if (!GameManager.Instance.IsWin && IsSignaling)
            {
                if (!_endCoroutine && bird.enabled)
                    StopPlayer();
            }
            else if (!IsSignaling)
            {
                if (!_endCoroutine && bird.enabled)
                    StopPlayer();
                bird.OnSignalDirection += GameManager.Instance.OnEnemySignal;
                IsSignaling = bird.SignalRandomDirection();
            }
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
            //yield return new WaitForSeconds(0.2f);
            //_isPaused = true;
            GameManager.Instance.IsInputEnable = false;
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool(IsRun, false);
            //yield return new WaitForSeconds(duration);
            //_isPaused = false;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
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