using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMenu : MonoBehaviour
{
    [Header("UI"), SerializeField] private Button _startRandomGameButton;
    [SerializeField] private Button _startCustomGameButton;
    [SerializeField] private Button _openCallbacksSettingsButton;
    [SerializeField] private Button _exitButton;
    [SerializeField] private GameObject _mainMenuView; 
    [SerializeField] private GameObject _gameOverView;
    [SerializeField] private TextMeshProUGUI _gameStatsText;
    [SerializeField] private CallbacksSettingsMenu _callbacksSettingsMenu;
    [SerializeField] private GameObject _setSeedView;
    [SerializeField] private TMPro.TMP_InputField _seedInputField;
    [SerializeField] private Button _setSeedButton;

    [Header("Logic"), SerializeField] private GameManager _gameManager;

    private void Awake()
    {
        _startRandomGameButton.onClick.AddListener(OnStartRandomGameButtonClicked);
        _startCustomGameButton.onClick.AddListener(OnStartCustomGameButtonClicked);
        _openCallbacksSettingsButton.onClick.AddListener(OnOpenCallbacksSettingsButtonClicked);
        _exitButton.onClick.AddListener(OnExitButtonClicked);
        _setSeedButton.onClick.AddListener(OnSetSeedButtonClicked);
    }
    private void OnStartRandomGameButtonClicked()
    {
        _setSeedView.SetActive(true);
        _seedInputField.text = string.Empty;
        _gameOverView.SetActive(false);
    }
    private void OnStartCustomGameButtonClicked()
    {
        _gameManager.RestartGame(_callbacksSettingsMenu.GetCurrentPreset(), OnGameOver, Random.Range(0, 10000));
        _mainMenuView.SetActive(false);
        _gameOverView.SetActive(false);
    }
    private void OnOpenCallbacksSettingsButtonClicked()
    {
        _callbacksSettingsMenu.gameObject.SetActive(true);
    }
    private void OnExitButtonClicked()
    {
        Application.Quit();
    }
    private void OnSetSeedButtonClicked()
    {
        if (int.TryParse(_seedInputField.text, out int seed))
        {
            _gameManager.RestartGame(_callbacksSettingsMenu.GetCurrentPreset().GetRandomPreset(seed), OnGameOver, seed);
            _mainMenuView.SetActive(false);
            _gameOverView.SetActive(false);
            _setSeedView.SetActive(false);
        }
        else
        {
            Debug.LogError("Invalid seed value.");
        }
    }
    private void OnGameOver(int passedRounds, int totalRounds, float averageRoundTime)
    {
        _gameOverView.SetActive(true);
        _gameStatsText.text = string.Format("{0}/{1}\n<size=32>{2}sec</size>", passedRounds, totalRounds, averageRoundTime.ToString("F2"));
    }
}
