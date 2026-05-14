using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Quest Objects")]
    public GunMountPoint mountPoint;
    public SnapPoint ladderSnapPoint;   
    public ButtonInteract button;

    [Header("Timing")]
    public float restartDelay = 5f;
    public float objectiveShowDelay = 2f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        button?.GetComponent<HighlightPulse>()?.SetHighlight(false);

        StartCoroutine(ShowFirstObjective());
    }

    IEnumerator ShowFirstObjective()
    {
        yield return new WaitForSeconds(objectiveShowDelay);
        UIManager.Instance?.ShowObjective("Найдите гравитационную пушку");
    }

    public void OnGunPickedUp()
    {
        UIManager.Instance?.ShowObjective("Установите пушку на платформу");
        mountPoint?.EnableHighlight();
    }

    public void OnGunInstalled()
    {
        UIManager.Instance?.ShowObjective("Используйте пушку, чтобы установить лестницу");
        ladderSnapPoint?.Activate();
    }

    public void OnLadderPlaced()
    {
        UIManager.Instance?.ShowObjective("Поднимитесь и нажмите кнопку");
        button?.GetComponent<HighlightPulse>()?.SetHighlight(true);
    }

    public void OnGateOpened()
    {
        UIManager.Instance?.ShowObjective("Выход открыт!");
    }

    public void TriggerWin()
    {
        UIManager.Instance?.ShowObjective("");
        UIManager.Instance?.ShowWinScreen();
        StartCoroutine(RestartAfterDelay());
    }

    IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSeconds(restartDelay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}