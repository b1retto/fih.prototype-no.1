using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private CinemachineInputAxisController cameraAxisController;
    [SerializeField] private AimCameraController aimCameraController;


    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (!pauseUI.activeSelf)
            {
                onPausePress();
            }
            else
            {
                OnResumePress();
            }
        }
    }

    public void onPausePress()
    {
        pauseUI.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;

        if (cameraAxisController != null) cameraAxisController.enabled = false;
        aimCameraController.enabled = false;
    }

    public void OnResumePress()
    {
        pauseUI.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        if (cameraAxisController != null) cameraAxisController.enabled = true;
        aimCameraController.enabled = true;
    }

    public void OnExitPress()
    {
        Time.timeScale = 1f;

        Application.Quit();

        UnityEditor.EditorApplication.isPlaying = false;
    }
}
