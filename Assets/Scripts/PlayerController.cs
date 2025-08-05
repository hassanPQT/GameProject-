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
                GameManager.Instance.IsWin = false;
                IsSignaling = false;
            }
            else if (!IsSignaling)
            {
                if (!_endCoroutine && enemy.enabled)
                    StartCoroutine(PausePlayer(2f));
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
                bird.StopMovement();
            }
            else if (!IsSignaling)
            {
                if (!_endCoroutine)
                    StartCoroutine(PausePlayer(2f));
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

        private IEnumerator PausePlayer(float duration)
        {
            yield return new WaitForSeconds(0.2f);
            _isPaused = true;
            _rb.linearVelocity = Vector2.zero;
            _animator.SetBool(IsRun, false);
            yield return new WaitForSeconds(duration);
            _isPaused = false;
            _endCoroutine = true;
            yield return new WaitForSeconds(5f);
            _endCoroutine = false;
        }

        public void Stop()
        {
            _rb.linearVelocity = Vector2.zero;
        }
    }


}