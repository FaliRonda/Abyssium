using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Puzzles
{
    [CreateAssetMenu(fileName = "New CombinationPuzzleData", menuName = "Puzzle/Combination Puzzle")]
    public class CombinationPuzzleSO : ScriptableObject
    {
        [SerializeField] public int puzzleID;
        [SerializeField] public string[] solutionCombination;
        
        [System.Serializable]
        public class SymbolData
        {
            public List<string> symbols = new List<string>();
        }
        
        [System.Serializable]
        public class SymbolsDictionary : SerializableDictionary<int, SymbolData> { }
    
        [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "Button index", ValueLabel = "Symbols")]
        public SymbolsDictionary symbolsDictionary = new SymbolsDictionary();
    }
}
