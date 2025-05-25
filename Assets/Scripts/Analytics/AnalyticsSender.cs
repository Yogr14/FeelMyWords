using System.Collections;
using System.Collections.Generic;
using Services.Korolitics.Core;
using UnityEngine;

public class AnalyticsSender : MonoBehaviour
{
    private const string _eventStartRun = "StartRun";
    private const string _eventGameOver = "GameOver";
    public static void SendStartRunEvent()
    {
        KorolitcsManager.ReportEvent(_eventStartRun, new Dictionary<string, object>
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
    public static void SendWordEvent(string word, bool timeOut, float timeSpan, Dictionary<string, bool> effectsEnabledState)
    {
        var parameters = new Dictionary<string, object>
        {
            { "Word", word },
            { "TimeOut", timeOut },
            { "TimeSpan", timeSpan },
            { "Platform", PlayerPrefs.GetString(StartSurveyMenu.PlatformKey, "PC")}
        };
        foreach (var effect in effectsEnabledState)
        {
            parameters.Add(effect.Key, effect.Value);
        }
        KorolitcsManager.ReportEvent("Word", parameters);
    }
}
