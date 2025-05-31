using System.Collections;
using System.Collections.Generic;
using Services.Korolitics.Core;
using UnityEngine;

public class AnalyticsSender : MonoBehaviour
{
    private const string _eventEndRun = "EndRun";
    private const string _wordPassedEvent = "WordPassed";
    public static void SendEndRunEvent()
    {
        KorolitcsManager.ReportEvent(_eventEndRun, new Dictionary<string, object>
        {
            { StartSurveyMenu.UserGenderKey, PlayerPrefs.GetString(StartSurveyMenu.UserGenderKey, "Male") },
            { StartSurveyMenu.UserAgeKey, PlayerPrefs.GetInt(StartSurveyMenu.UserAgeKey, 18)},
            { StartSurveyMenu.UserGoogleTableLinkKey, PlayerPrefs.GetString(StartSurveyMenu.UserGoogleTableLinkKey, "-1") },
            { StartSurveyMenu.UserNameKey, PlayerPrefs.GetString(StartSurveyMenu.UserNameKey, "Иванов Иван Иванович") },
            { StartSurveyMenu.UserTelegramNicknameKey, PlayerPrefs.GetString(StartSurveyMenu.UserTelegramNicknameKey, "@ivan") },
            { StartSurveyMenu.UserGroupKey, PlayerPrefs.GetString(StartSurveyMenu.UserGroupKey, "ПСИ-324") },
            { StartSurveyMenu.TeacherLastNameKey, PlayerPrefs.GetString(StartSurveyMenu.TeacherLastNameKey, "Ардисламов") },
            { StartSurveyMenu.DisciplineNameKey, PlayerPrefs.GetString(StartSurveyMenu.DisciplineNameKey, "Критическое мышление") }
        });
    }
    public static void SendWordEvent(string word, bool timeOut, float timeSpan, string firstLetter, float fLTimeSpan, Dictionary<string, bool> effectsEnabledState)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Word", word },
            { "TimeOut", timeOut },
            { "TimeSpan", timeSpan },
            { "FirstLetter", firstLetter },
            { "FirstLetterTimeSpan", fLTimeSpan },
            { "Platform", PlayerPrefs.GetString(StartSurveyMenu.PlatformKey, "PC")}
        };
        foreach (var effect in effectsEnabledState)
        {
            parameters.Add(effect.Key, effect.Value);
        }
        KorolitcsManager.ReportEvent(_wordPassedEvent, parameters);
    }
}
