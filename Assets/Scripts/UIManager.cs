using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Crosshair")]
    public GameObject crosshair;

    [Header("Interaction Prompt")]
    public GameObject promptRoot;
    public TextMeshProUGUI promptText;

    [Header("Objective")]
    public GameObject objectiveRoot;
    public TextMeshProUGUI objectiveText;

    [Header("Win Screen")]
    public GameObject winScreen;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        ShowCrosshair(false);
        ShowPrompt(false, "");
        ShowObjective("");
        winScreen?.SetActive(false);
    }

    public void ShowCrosshair(bool show) => crosshair?.SetActive(show);

    public void ShowPrompt(bool show, string text)
    {
        promptRoot?.SetActive(show);
        if (promptText != null) promptText.text = text;
    }

    public void ShowObjective(string text)
    {
        if (objectiveText != null) objectiveText.text = text;
        objectiveRoot?.SetActive(!string.IsNullOrEmpty(text));
    }

    public void ShowWinScreen()
    {
        winScreen?.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}