using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;
using UnityEngine.Video; // REQUIRED: Added for VideoPlayer control

public class HealthBarScript : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    [SerializeField] private GameObject gameOverScene, mainCamera;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private VideoPlayer gameOverVideo;

    void Start()
    {
        gameOverScene.SetActive(false);
        mainCamera.SetActive(true);
    }

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        fill.color = gradient.Evaluate(1f);
    }

    public void SetHealth(int health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);

        if (slider.value <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (gameOverScene.activeSelf) return;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        playerController.enabled = false;

        Time.timeScale = 0f;

        if (gameOverVideo != null)
        {
            gameOverVideo.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
            gameOverVideo.Play();
        }

        gameOverScene.SetActive(true);
        uiManager.controlsUI.SetActive(false);
        mainCamera.SetActive(false);

        gameObject.SetActive(false);
    }
}
