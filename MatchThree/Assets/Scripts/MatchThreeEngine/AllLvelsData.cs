using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MatchThreeEngine
{
    [CreateAssetMenu(fileName = "AllLevelsData", menuName = "MatchThree/AllLevelsData")]
    public class AllLvelsData : ScriptableObject
    {
        public List<LevelData> LevelsData;
    }
}
