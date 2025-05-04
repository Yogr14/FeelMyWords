using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WordsLib", menuName = "ScriptableObjects/WordsLib", order = 1)]
public class WordsLib : ScriptableObject
{
    [SerializeField] private List<string> _words = new List<string>();
    public List<string> Words => _words;
}
