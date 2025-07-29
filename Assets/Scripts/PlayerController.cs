using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Gameplay
{
    public class PlayerController : MonoBehaviour
    {
        private static readonly int IsJump = Animator.StringToHash("isJump");
        private static readonly int IsRun = Animator.StringToHash("isRun");

        public float moveSpeed = 5f;
        public float jumpForce = 10f;
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        public LayerMask groundLayer;

        [SerializeField] private Rigidbody2D rb;
        [SerializeField] private Animator animator;

        private bool _isGrounded;
        private int _jumpCount = 0;

        private const int _maxJumpCount = 0;

        // invicible
        [SerializeField] private GameObject visualObject;
        [SerializeField] private float invincibleDuration = 1f;
        private bool _isInvincible = false;

        public void Stop()
        {
            rb.linearVelocity = Vector2.zero;
        }

        private void Update()
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (_isGrounded)
                _jumpCount = 0;

            animator.SetBool(IsJump, !_isGrounded);
        }


        public void Move(Vector2 input)
        {
            rb.linearVelocity = new Vector2(input.x * moveSpeed, rb.linearVelocity.y);

            bool check = Mathf.Abs(input.x) > 0.01f && _isGrounded;

            animator.SetBool(IsRun, Mathf.Abs(input.x) > 0.01f && _isGrounded);

            if (Mathf.Abs(input.x) > 0.01f)
            {
                transform.localScale = new Vector3(Mathf.Sign(input.x), 1, 1);
            }
        }

        public void Jump()
        {
            if (_jumpCount <= _maxJumpCount && _isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                _jumpCount++;
                Debug.Log(_jumpCount + " " + _maxJumpCount);
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

            while (timer < invincibleDuration)
            {
                visible = !visible;
                if (visualObject != null)
                    visualObject.SetActive(visible);

                yield return new WaitForSeconds(0.1f);
                timer += 0.1f;
            }

            if (visualObject != null)
                visualObject.SetActive(true);

            _isInvincible = false;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //if (other.CompareTag("EndPoint"))
            //{
            //    Destroy(other.gameObject);
            //    GameController.Instance.ShowEndGame();
            //}

            //if (other.CompareTag("Coin"))
            //{
            //    Destroy(other.gameObject);
            //    GameController.Instance.EarnCoin();
            //}

            if (other.CompareTag("Trap") && !_isInvincible)
            {
                GameManager.Instance.TakeDamage(1);
                StartInvincibility();
            }
        }
    }
}