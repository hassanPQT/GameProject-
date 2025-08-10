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
        public bool IsPlaying = false;
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

        private IEnemy currentEnemy;
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


        private void DetectEnemy()
        {
            if (IsPlaying) return;
            Collider2D[] hits = GetEnemies();

            foreach (var hit in hits)
            {
                var enemy = hit.GetComponent<IEnemy>();
                if (enemy != null)
                {
                    if (enemy.IsWin) continue;
                    currentEnemy = enemy;
                    StartPlay();
                }
            }
        }

        private Collider2D[] GetEnemies()
        {
            float detectRadius = 3.5f;
            LayerMask enemyLayer = LayerMask.GetMask("Flying");
            DrawDebugCircle(transform.position, detectRadius, Color.red);
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius, enemyLayer);
            return hits;
        }

        private void StartPlay()
        {
            IsPlaying = true;
            StopPlayer();
            currentEnemy.OnPlayerRequest();
        }
        public void OnPlayerWin()
        {
            IsPlaying = false;
            currentEnemy.IsWin = true;
        }
        public void OnPayerLose()
        {
            IsPlaying = false;
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
        public void SetModifierSpeed(float mult)
        {
            _modifierSpeed = mult;
            Debug.Log("move speed" + _moveSpeed);
        }

       


    }


}