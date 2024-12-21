using System;
using System.Collections;
using System.Collections.Generic;
using MatchThreeEngine;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace UI
{
    
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance {get; private set;}
        [SerializeField] private LevelCompletedTab _levelCompletedTab;
        [SerializeField] private GameObject _gameOverTab;
        public List<TilesToCollectUI> tilesToCollectUI;
        public TipButton tipButton;
        public InGameData inGameData;
        public bool Pause {get; set;}
        public Coroutine TimerCoroutine;
        

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            SetTipsAmount();
        }

        public void SetTipsAmount()
        {
            var currentTipsAmount = PlayerPrefs.GetInt(GlobalData.AVAILABLE_TIPS, 0);
            tipButton.TipsAmount.SetText(currentTipsAmount.ToString());
            tipButton.Button.interactable = currentTipsAmount > 0;
        }

        public void OnLevelComplete(LevelData currentLvel)
        {
            if (TimerCoroutine != null) StopCoroutine(TimerCoroutine);

            _levelCompletedTab.LevelCompletTabObject.SetActive(true);
            _levelCompletedTab.SetStars(inGameData.StarsImages.Count);

            var onStarsComleted = inGameData.StarsImages.Count;
            var levelNumber = PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL);
            GlobalData.OnLevelComplet(levelNumber, onStarsComleted);
            PlayerPrefs.Save();
        }
        public void OnGameOver(float num)
        {
            if (TimerCoroutine != null) StopCoroutine(TimerCoroutine);
            ToggleObject(_gameOverTab);
        }

        public void ChangeScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        public void ToggleObject(GameObject objectToToggle)
        {
            objectToToggle.SetActive(objectToToggle.activeSelf ? false : true);
        }
        public void NextLevel()
        {
            var currentLevel = PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL);
            PlayerPrefs.SetInt(GlobalData.LAST_PLAYED_LEVEL, currentLevel + 1);
            PlayerPrefs.Save();
            ChangeScene(GlobalData.IN_GAME_SCENE);
        }
        public IEnumerator UITimerCorutine(float timeOnTwoStars)
        {
            inGameData.TimerImage.fillAmount = 0.5f;

            var currentTime = 0f;
            var deltaTime = 0f;
            var endTime = 1f;
            while (deltaTime != timeOnTwoStars)
            {
                inGameData.TimerImage.fillAmount = Mathf.SmoothStep(0.5f, 0f, currentTime);
                deltaTime = Mathf.Min(timeOnTwoStars, deltaTime + Time.deltaTime);
                currentTime = Mathf.Min(endTime, (endTime * deltaTime) / timeOnTwoStars);

                yield return new WaitWhile(() => Pause);

                if (inGameData.TimerImage.fillAmount <= 0.25f && inGameData.OffStar(2))
                {
                    inGameData.OffStar(2);
                }

                yield return new WaitForEndOfFrame();
            }
            inGameData.OffStar(1);
            TimerCoroutine = null;

        }
        public void SetTilesToCollect(List<LevelData.TilesToCollect> tilesToCollect)
        {
            inGameData.TotalScoreValue.gameObject.SetActive(false);
            var parent = tilesToCollectUI[0].TileIcon.gameObject.GetComponentInParent<LayoutElement>();
            Debug.Log($"Parent: {parent.name}");
            parent.ignoreLayout = false;
            parent.gameObject.SetActive(true);
            for (int i = 0; i < tilesToCollect.Count; i++)
            {
                var tile = tilesToCollect[i];
                tilesToCollectUI[i].TileIcon.gameObject.SetActive(true);
                tilesToCollectUI[i].TileIcon.sprite = tile.TileType.Sprite;
                tilesToCollectUI[i].TilesAmount.SetText(tile.AmountToCollect.ToString());
            }
        }

        public void CollectTile(int tileIndex, int tilesCount)
        {
            tilesToCollectUI[tileIndex].TilesAmount.SetText(tilesCount >= 0 ? 
                                                            tilesCount.ToString() : 0.ToString());
        }

        [Serializable]
        public struct LevelCompletedTab
        {
            public GameObject LevelCompletTabObject;
            public List<Image> StarsImages;
            public ParticleSystem StarsParticles;
            public Sprite GoldStarSprite;
            public void SetStars(int starsCount)
            {
                for (int i = 0; i < starsCount; i++)
                {
                    StarsImages[i].sprite = GoldStarSprite;
                    var starsParticles = Instantiate(StarsParticles, StarsImages[i].rectTransform);
                    starsParticles.startDelay = i;
                }
            }
        }
        [Serializable]
        public struct InGameData
        {
            public TextMeshProUGUI LevelSerialNumber;
            public Image TimerImage;
            public List<Image> StarsImages;
            public Sprite GreyStarSprite;
            //public TextMeshProUGUI ScoreMultiplier;
            public TextMeshProUGUI TotalScoreValue;
            public bool OffStar(int starSerialNumber)
            {
                if (starSerialNumber < StarsImages.Count)
                {
                    StarsImages[starSerialNumber].sprite = GreyStarSprite;
                    StarsImages.RemoveAt(starSerialNumber);
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        [Serializable]
        public struct TipButton
        {
            public Button Button;
            public TextMeshProUGUI TipsAmount;
        }
        [Serializable]
		public struct TilesToCollectUI
		{
			public Image TileIcon;
            public TextMeshProUGUI TilesAmount;
		}
    }
}
