using System.Collections;
using System.Collections.Generic;
using MatchThreeEngine;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

public class ChooseLevelButton : MonoBehaviour
{
    [SerializeField] private LevelData _levelData;
    [SerializeField] private AllLvelsData _allLevelsData;
    [SerializeField] private TextMeshProUGUI _levelNumberText;
    [SerializeField] private Button _button;
    [SerializeField] private Image _isLevelCompleteImage;
    [SerializeField] private Sprite _ifLevelCompeteSprite;
    [SerializeField] private Image[] _starsImages;
    [SerializeField] private Sprite _goldStarSprite;
    private int _levelSerialNumber;
    private void Start()
    {
        _levelSerialNumber = _allLevelsData.LevelsData.IndexOf(_levelData);
        _levelNumberText.SetText((_allLevelsData.LevelsData.IndexOf(_levelData) + 1).ToString());
        
        var onStarsComleted = GlobalData.IsLevelComplet(_levelSerialNumber);
        if (onStarsComleted > 0)
        {
            _isLevelCompleteImage.sprite = _ifLevelCompeteSprite;
            for (int i = 0; i < onStarsComleted; i++)
            {
                _starsImages[i].sprite = _goldStarSprite;
            }
        } 
        
        if (_levelSerialNumber > 0)
        {
            var previousLevelOnStarsComplet = GlobalData.IsLevelComplet(_levelSerialNumber - 1);
            _button.interactable = previousLevelOnStarsComplet > 0;
        }
        
        _button.onClick.AddListener(ChooseLevel);
    }

    private void ChooseLevel()
    {
        PlayerPrefs.SetInt(GlobalData.LAST_PLAYED_LEVEL, _levelSerialNumber);
        Debug.Log(_levelSerialNumber);
        UIManager.Instance.ChangeScene(GlobalData.IN_GAME_SCENE);
    }
}
