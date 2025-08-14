using UnityEngine;

public class PushBackEffect : MonoBehaviour
{
    private Rigidbody2D _rb;
    private float _timer;
    private float _timeOut = 4f;

    public Vector2 BossDirection {  get; set; }
    public Vector2 PlayerDirection {  get; set; }
    public float Force { get; set; }
    private void OnEnable()
    {
        _rb = GetComponent<Rigidbody2D>() ?? GetComponentInChildren<Rigidbody2D>();
        BossDirection = Vector2.left;
        PlayerDirection = Vector2.zero;
        Force = 1f;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void FixedUpdate()
    {
        ApplyMovementEffect();
    }
    private void Update()
    {
        HandleInputAndDirection();
    }

    private void ApplyMovementEffect()
    {
        Debug.Log("BOSS DIR: " + BossDirection);
        Vector2 totalDirection = PlayerDirection + BossDirection;
        if (totalDirection != Vector2.zero)
        {
            //_hasPlaying = false;
            _rb.linearVelocityX = -Force;
            //return;
        }
        else
        {
            //_hasPlaying = true;
            _rb.linearVelocityX = Force;
        }
    }
    private void HandleInputAndDirection()
    {
        // Kiểm tra điều kiện đầu vào
        bool isInputActive = (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) &&
                            Input.GetMouseButton(1) &&
                            Input.GetMouseButton(0) &&
                            Time.time - _timer < _timeOut;

        if (Input.GetMouseButtonDown(0))
        {
            _timer = Time.time;
        }

        if (isInputActive)
        {
            PlayerDirection = GetClosestDirection(CalculateDirectionToMouse());
            Debug.Log("has playing " + PlayerDirection);
        }
        else
        {
            PlayerDirection = Vector2.zero;
        }
    }
    private Vector2 CalculateDirectionToMouse()
    {
        Vector3 mouseScreenPosition = Input.mousePosition;
        Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
        Vector3 direction = (mouseWorldPosition - transform.position);
        return direction;
    }
    private Vector2 GetClosestDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return Vector3.zero;

        Vector3 right = Vector2.right;         // (1, 0, 0)
        Vector2 upRight = (Vector3.right + Vector3.up); // (1, 1, 0)
        Vector2 downRight = (Vector3.right + Vector3.down); // (1, -1, 0)

        float angleToRight = Vector2.Angle(direction, right);
        float angleToUpRight = Vector2.Angle(direction, upRight);
        float angleToDownRight = Vector2.Angle(direction, downRight);

        float angleThreshold = 22.5f;

        if (angleToRight <= angleThreshold)
        {
            return right;
        }
        else if (angleToUpRight <= angleThreshold)
        {

            return upRight;
        }
        else if (angleToDownRight <= angleThreshold)
        {

            return downRight;
        }
        else
            return Vector2.zero;
    }
   
}
