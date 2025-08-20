using UnityEngine;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine.InputSystem.Controls;

public class CameraManager : MonoBehaviour
{
    static List<CinemachineVirtualCamera> cameras ;

    /*  public static CinemachineVirtualCamera _activeCamera = null;

      public static bool IsCameraActive(CinemachineVirtualCamera camera)
      
          return camera =_activeCamera;
      }
  */
    private void Start()
    {
        cameras = new List<CinemachineVirtualCamera>(FindObjectsByType<CinemachineVirtualCamera>(FindObjectsSortMode.None));
      
    }
    public static void SwitchCamera(CinemachineVirtualCamera newCamera)
    {
 /*       newCamera.Priority = 11;
        _activeCamera = newCamera;*/
     //   Register(newCamera);
     newCamera.gameObject.SetActive(true);

        foreach (CinemachineVirtualCamera camera in cameras)
        {
            if (camera != newCamera)
            {
                camera.gameObject.SetActive(false);
                Debug.Log("Camera deactivated: " + camera.name);
            }
        }
    }
    public static void Register(CinemachineVirtualCamera camera)
    {
        cameras.Add(camera);
    }

    public static void Unregister(CinemachineVirtualCamera camera)
    {
        cameras.Remove(camera);
    }
}
