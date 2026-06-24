using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startMenuUI, pauseUI, controlsUI;

    [Header("Controllers")]
    [SerializeField] private WorldCrossHairController crosshairController;
    [SerializeField] private CinemachineInputAxisController cameraAxisController;
    [SerializeField] private AimCameraController aimCameraController;

    private bool hasStarted;

    void Start() => ToggleState(start: true, pause: false, controls: false, active: false);

    void Update()
    {
        if (hasStarted && Input.GetKeyDown(KeyCode.BackQuote))
        {
            if (pauseUI.activeSelf) OnResumePress(); else onPausePress();
        }
    }

    public void OnStartPress() => ToggleState(start: false, pause: false, controls: true, active: true);
    public void onPausePress() => ToggleState(start: false, pause: true, controls: false, active: false);
    public void OnResumePress() => ToggleState(start: false, pause: false, controls: true, active: true);
    public void OnReturnPress() => ToggleState(start: true, pause: false, controls: false, active: false);

    public void OnExitPress()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnRestartPress()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void ToggleState(bool start, bool pause, bool controls, bool active)
    {
        hasStarted = !start;
        startMenuUI.SetActive(start);
        pauseUI.SetActive(pause);
        controlsUI.SetActive(controls);

        Cursor.visible = !active;
        Cursor.lockState = active ? CursorLockMode.Locked : CursorLockMode.None;
        Time.timeScale = active ? 1f : 0f;

        if (crosshairController) crosshairController.SetCrosshairVisibility(active);
        if (cameraAxisController) cameraAxisController.enabled = active;
        if (aimCameraController) aimCameraController.enabled = active;
    }
}
