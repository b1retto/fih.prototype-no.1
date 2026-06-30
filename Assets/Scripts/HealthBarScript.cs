using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    public Image fill;

    [SerializeField] private GameObject gameOverScene;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private PlayerController playerController;

    [SerializeField] private UIManager uiManager;

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

        if (slider.value == 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        if (gameOverScene.activeSelf == true) return;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        playerController.enabled = false;

        AudioListener.pause = true;

        Time.timeScale = 0f;

        gameOverScene.SetActive(true);
        mainCamera.SetActive(false);
        uiManager.controlsUI.SetActive(false);

        gameObject.SetActive(false);
    }

}
