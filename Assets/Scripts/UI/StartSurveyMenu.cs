using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StartSurveyMenu : MonoBehaviour
{
    public const string UserGenderKey = "UserGender";
    public const string UserAgeKey = "UserAge";
    public const string UserGoogleTableLinkKey = "UserGoogleTableLink";
    public const string UserNameKey = "UserName";
    public const string UserTelegramNicknameKey = "UserTelegramNickname";
    public const string UserGroupKey = "UserGroup";
    public const string TeacherLastNameKey = "TeacherLastName";
    public const string DisciplineNameKey = "DisciplineName";
    public const string PlatformKey = "UserPlatform";
    [Header("Initial question"), SerializeField] private GameObject _initialQuestionView;
    [SerializeField] private TMPro.TMP_InputField _seedInputField;
    [SerializeField] private Button _startFullSurveyButton;
    [SerializeField] private Button _startSimpleSurveyButton;
    [Header("Survey"), SerializeField] private GameObject _surveyView;
    [SerializeField, Tooltip("False - female, True - male")] private Toggle _userGender;
    [SerializeField] private TMPro.TMP_InputField _userAge;
    [SerializeField] private TMPro.TMP_InputField _userGoogleTableLink;
    [SerializeField] private TMPro.TMP_InputField _userName;
    [SerializeField] private TMPro.TMP_InputField _userTelegramNickname;
    [SerializeField] private TMPro.TMP_InputField _userGroup;
    [SerializeField] private TMPro.TMP_InputField _teacherLastName;
    [SerializeField] private TMPro.TMP_InputField _disciplineName;
    [SerializeField, Tooltip("True - PC, False - Mobile")] private Toggle _platformToggle;
    [SerializeField] private Button _startGameButton;
    [SerializeField] private Button _backButton;
    private bool _isSimpleSurvey;
    private Action StartGameEvent;

    private void Awake()
    {
        _startFullSurveyButton.onClick.AddListener(StartFullSurvey);
        _startSimpleSurveyButton.onClick.AddListener(StartSimpleSurvey);
        _startGameButton.onClick.AddListener(OnStartGameButtonClicked);
        _backButton.onClick.AddListener(OnBackButtonClicked);

        //load saved data
        _userGender.isOn = PlayerPrefs.GetString(UserGenderKey, "Male") == "Male";
        _userAge.text = PlayerPrefs.GetInt(UserAgeKey, 18).ToString();
        _userGoogleTableLink.text = PlayerPrefs.GetString(UserGoogleTableLinkKey, "1234");
        _userName.text = PlayerPrefs.GetString(UserNameKey, "Иванов Иван Иванович");
        _userTelegramNickname.text = PlayerPrefs.GetString(UserTelegramNicknameKey, "@ivan");
        _userGroup.text = PlayerPrefs.GetString(UserGroupKey, "ПСИ-324");
        _teacherLastName.text = PlayerPrefs.GetString(TeacherLastNameKey, "Ардисламов");
        _disciplineName.text = PlayerPrefs.GetString(DisciplineNameKey, "Критическое мышление");
        _platformToggle.isOn = PlayerPrefs.GetString(PlatformKey, "PC") == "PC";

        //subscribe to events to update saved data
        _userGender.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(UserGenderKey, v ? "Male" : "Female"); });
        _userAge.onValueChanged.AddListener((v) => { if (int.TryParse(v, out int value)) PlayerPrefs.SetInt(UserAgeKey, value); CheckIfDataIsFull(); });
        _userGoogleTableLink.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(UserGoogleTableLinkKey, v); CheckIfDataIsFull(); });
        _userName.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(UserNameKey, v); CheckIfDataIsFull(); });
        _userTelegramNickname.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(UserTelegramNicknameKey, v); CheckIfDataIsFull(); });
        _userGroup.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(UserGroupKey, v); CheckIfDataIsFull(); });
        _teacherLastName.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(TeacherLastNameKey, v); CheckIfDataIsFull(); });
        _disciplineName.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(DisciplineNameKey, v); CheckIfDataIsFull(); });
        _platformToggle.onValueChanged.AddListener((v) => { PlayerPrefs.SetString(PlatformKey, v ? "PC" : "Mobile"); });
    }
    public void SetCallback(Action startGameCallback)
    {
        StartGameEvent = null;
        StartGameEvent += startGameCallback;
    }
    private void StartFullSurvey()
    {
        if (!int.TryParse(_seedInputField.text, out int seed)) return;
        _initialQuestionView.SetActive(false);
        _surveyView.SetActive(true);
        _userGender.gameObject.SetActive(true);
        _userAge.gameObject.SetActive(true);
        _userGoogleTableLink.gameObject.SetActive(true);
        _userName.gameObject.SetActive(true);
        _userTelegramNickname.gameObject.SetActive(true);
        _userGroup.gameObject.SetActive(true);
        _teacherLastName.gameObject.SetActive(true);
        _disciplineName.gameObject.SetActive(true);
        _platformToggle.gameObject.SetActive(true);
        _startGameButton.interactable = false;
        _isSimpleSurvey = false;
        CheckIfDataIsFull();
    }
    private void StartSimpleSurvey()
    {
        if (!int.TryParse(_seedInputField.text, out int seed)) return;
        _initialQuestionView.SetActive(false);
        _surveyView.SetActive(true);
        _userGender.gameObject.SetActive(true);
        _userAge.gameObject.SetActive(true);
        _userGoogleTableLink.gameObject.SetActive(false);
        _userName.gameObject.SetActive(false);
        _userTelegramNickname.gameObject.SetActive(false);
        _userGroup.gameObject.SetActive(false);
        _teacherLastName.gameObject.SetActive(false);
        _disciplineName.gameObject.SetActive(false);
        _platformToggle.gameObject.SetActive(false);
        _startGameButton.interactable = false;

        _isSimpleSurvey = true;
        CheckIfDataIsFull();
    }
    private void OnStartGameButtonClicked()
    {
        _initialQuestionView.SetActive(false);
        _surveyView.SetActive(false);
        StartGameEvent?.Invoke();
    }
    private void OnBackButtonClicked()
    {
        _initialQuestionView.SetActive(false);
        _surveyView.SetActive(false);
    }

    private void CheckIfDataIsFull()
    {
        _startGameButton.interactable = false;

        if (!int.TryParse(_userAge.text, out int age)) return;
        if (age < 0 || age > 100) return;
        if (_isSimpleSurvey) { _startGameButton.interactable = true; return; }
        if (string.IsNullOrEmpty(_userGoogleTableLink.text)) return;
        if (string.IsNullOrEmpty(_userName.text)) return;
        if (string.IsNullOrEmpty(_userTelegramNickname.text)) return;
        if (string.IsNullOrEmpty(_userGroup.text)) return;
        if (string.IsNullOrEmpty(_teacherLastName.text)) return;
        if (string.IsNullOrEmpty(_disciplineName.text)) return;
        _startGameButton.interactable = true;
    }
}
