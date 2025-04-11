using MatchThreeEngine;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TutorialLevelData", menuName = "MatchThree/TutorialLevelData")]
public class TutorialLevelData : LevelData
{
    [SerializeField] private GameObject _tutorialWelcomePrefab;
}
