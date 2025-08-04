using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class CameraFollowObject : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerTransform;

    [Header("Flip Rotation Stats")]
    [SerializeField] private float _flipRotationTime = 0.5f;

    private PlayerController _player;

    private bool _isFacingRight ;

    private Coroutine _turnCoroutine;

    private void Awake()
    {
        _player = _playerTransform.gameObject.GetComponent<PlayerController>();

      //  _isFacingRight = _player.IsFacingRight;
    }

   
    private void Update()
    {
        transform.position = _playerTransform.position;
    }

    public void CallTurn()
    {
       // _turnCoroutine = StartCoroutine(FlipYLerp());
       LeanTween.rotateY(gameObject,DetermineEndRotation(),_flipRotationTime).setEaseInOutSine();
    }

    private IEnumerator FlipYLerp()
    {
        float statsRotation = transform.localEulerAngles.y;
        float endRotationAmount = DetermineEndRotation();
        float yRotation = 0f;

        float elapsedTime = 0f;
        while(elapsedTime < _flipRotationTime)
        {
            elapsedTime += Time.deltaTime;
            yRotation = Mathf.Lerp(statsRotation, endRotationAmount, (elapsedTime / _flipRotationTime));
            transform.rotation = Quaternion.Euler(0f, yRotation, 0f);

            yield return null;
        }
    }

    private float DetermineEndRotation()
    {
        _isFacingRight = !_isFacingRight;

        if(_isFacingRight)
        {
            return 180f;
        }
        else
        {
            return 0;
        }
    }
}
