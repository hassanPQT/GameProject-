using System.Collections.Generic;
using UnityEngine;

public class SongWheelManager : MonoBehaviour
{
    public static SongWheelManager Instance;
    public int[] DirectionNumber => _directionNumberList.ToArray();
    private List<int> _directionNumberList;
    private int _currentDirectionIndex;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        _currentDirectionIndex = 6;

        SetupDirectionNumbers();
    }
    public SongDirection GetRandomSongDirection()
    {
        return (SongDirection)DirectionNumber[Random.Range(0, DirectionNumber.Length)];
    }
    private void SetupDirectionNumbers()
    {
        _directionNumberList = new List<int>();
        _directionNumberList.Add(0);
        _directionNumberList.Add(7);
    }
    public void AddMoreRandomSongWheelNumbers()
    {
        if (_currentDirectionIndex == 0)
            return;
        _directionNumberList.Add(_currentDirectionIndex);
        _currentDirectionIndex--;
    }
}
