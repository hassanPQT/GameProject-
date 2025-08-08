using UnityEngine;

public class InputManager : MonoBehaviour
{
    /// <summary>
    ///  nho sua singlton
    /// </summary>
    public static InputManager Instance;

    private bool IsCursor = true;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        IsCursor = true;
    }
    public void LockCursor()
    {
        if (!IsCursor) return;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void UnlockCursor()
    {
        if (!IsCursor) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    [ContextMenu("a")]
    public void GameLose()
    {
        
        IsCursor = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
