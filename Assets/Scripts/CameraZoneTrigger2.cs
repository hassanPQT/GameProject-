using Cinemachine;
using Game.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraZoneTrigger2 : MonoBehaviour
{
    [SerializeField]
    GameObject nextSceneCamera;
    [SerializeField]
    GameObject player;
    [SerializeField]
    GameObject currentCamera;
    bool isActive = true;

    private void Start()
    {
        nextSceneCamera.SetActive(false);
        player = GameObject.FindGameObjectWithTag("Player");
        currentCamera = currentCamera = GameObject.Find("VC3");

    }

    private void Update()
    {
        currentCamera = GameObject.Find("VC3");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isActive)
            {
                Debug.Log("Player entered camera zone trigger.");

                nextSceneCamera.SetActive(true);
                nextSceneCamera.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
                //currentCamera.SetActive(false);
                CameraManager.SwitchCamera(nextSceneCamera.GetComponent<CinemachineVirtualCamera>());
                isActive = !isActive;
            }
            else
            {
               CameraManager.SwitchCamera(currentCamera.GetComponent<CinemachineVirtualCamera>());
                nextSceneCamera.gameObject.SetActive(false);
                isActive = !isActive;
            }
           

        }
    }
}
