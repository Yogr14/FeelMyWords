using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordsLib", menuName = "ScriptableObjects/WordsLib", order = 1)]
public class WordsLib : ScriptableObject
{
    [SerializeField] private int _wordsPerRound = 21;
    [SerializeField] private int _analyticsFreeWords = 1;
    [SerializeField] private List<string> _words = new List<string>();
    public List<string> Words => _words;
    public int WordsPerRound =>  Mathf.Clamp(_wordsPerRound, 0, _words.Count);
    public int AnalyticsFreeWords => Mathf.Clamp(_analyticsFreeWords, 0, _words.Count);
}
