using Unity.Cinemachine;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    public GameObject startMenuUI;
    public GameObject pauseUI;
    public GameObject controlsUI;
    public GameObject healthBarUI;

    [SerializeField] private WorldCrossHairController crosshairController;
    [SerializeField] private CinemachineInputAxisController cameraAxisController;
    [SerializeField] private AimCameraController aimCameraController;

    public bool hasStarted = false;


    void Start()
    {
        startMenuUI.SetActive(true);
        pauseUI.SetActive(false);
        controlsUI.SetActive(false);
        healthBarUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
        if (cameraAxisController != null) cameraAxisController.enabled = false;
        if (aimCameraController != null) aimCameraController.enabled = false;
    }

    void Update()
    {
        if (!hasStarted) return;

        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (!hasStarted) return;

            if (!pauseUI.activeSelf)
            {
                if (Time.timeScale == 0f) return;
                onPausePress();
            }
            else
            {
                OnResumePress();
            }
        }
    }


    public void OnStartPress()
    {
        hasStarted = true;
        startMenuUI.SetActive(false);
        controlsUI.SetActive(true);
        healthBarUI.SetActive(true);

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairVisibility(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        if (cameraAxisController != null) cameraAxisController.enabled = true;
        if (aimCameraController != null) aimCameraController.enabled = true;
    }

    public void onPausePress()
    {
        pauseUI.SetActive(true);
        controlsUI.SetActive(false);
        healthBarUI.SetActive(false);

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairVisibility(false);
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;

        if (cameraAxisController != null) cameraAxisController.enabled = false;
        aimCameraController.enabled = false;
    }

    public void OnResumePress()
    {
        pauseUI.SetActive(false);
        controlsUI.SetActive(true);
        healthBarUI.SetActive(true);

        if (crosshairController != null)
        {
            crosshairController.SetCrosshairVisibility(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;

        if (cameraAxisController != null) cameraAxisController.enabled = true;
        aimCameraController.enabled = true;
    }

    public void OnReturnPress()
    {
        hasStarted = false;

        startMenuUI.SetActive(true);
        pauseUI.SetActive(false);
        controlsUI.SetActive(false);
        healthBarUI.SetActive(false);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Time.timeScale = 0f;
        if (cameraAxisController != null) cameraAxisController.enabled = false;
        if (aimCameraController != null) aimCameraController.enabled = false;
    }

    public void OnExitPress()
    {
        Time.timeScale = 1f;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
