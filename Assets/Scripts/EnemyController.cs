using UnityEngine;
using System;
using Random = UnityEngine.Random;
using System.Collections;

public enum SongDirection
{
    Up = 0,
    UpRight = 7,
    Right = 6,
    DownRight = 5,
    Down = 4,
    DownLeft = 3,
    Left = 2,
    UpLeft = 1
}

public class EnemyController : MonoBehaviour
{
    public float moveDistance = 2f;
    public float moveDuration = 0.2f;

    // Sự kiện đưa ra hướng mới  
    public event Action<SongDirection[]> OnSignalDirection;

    private SongDirection[] currentDir;

    public bool SignalRandomDirection()
    {
        currentDir = new SongDirection[2];

        for (int i = 0; i < currentDir.Length; i++)
        {
            // Chọn ngẫu nhiên một hướng từ SongDirection  
            currentDir[i] = (SongDirection)Random.Range(0, 7);
        }

        OnSignalDirection?.Invoke(currentDir);
        // Khởi động hành động (di chuyển hoặc effect) theo hướng đó  
        StartCoroutine(MoveInDirection(currentDir));

        return true;
    }

    private IEnumerator MoveInDirection(SongDirection[] dir)
    {
        for (int i = 0; i < dir.Length; i++)
        {
            Vector3 dirVec = DirectionToVector(dir[i]);
            Vector3 start = transform.position;
            Vector3 end = start + dirVec * moveDistance;

            float t = 0f;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, t / moveDuration);
                yield return null;
            }

            t = 0;
            while (t < moveDuration)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(end, start, t / moveDuration);
                yield return null;
            }
            // Đợi một chút trước khi di chuyển tiếp  
            yield return new WaitForSeconds(0.5f);
        }
    }

    private Vector3 DirectionToVector(SongDirection dir)
    {
        // mỗi dir cách nhau 45°, và bạn muốn dir=0 là 90°  
        float angleDeg = (int)dir * 45f + 90f;
        float angleRad = angleDeg * Mathf.Deg2Rad;
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad), 0f).normalized;
    }
}
