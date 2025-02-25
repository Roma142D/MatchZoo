using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using MatchThreeEngine;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Device;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace UI
{
    
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance {get; private set;}
        [SerializeField] private LoadingScreen _loadingScreen;
        [SerializeField] private LevelCompletedTab _levelCompletedTab;
        [SerializeField] private GameObject _gameOverTab;
        [SerializeField] public SettingsTab settingsTab;
        public StartingScreen startingScreen;
        public Image backgroundImage;
        public SoundManager soundManager;
        public TipButton tipButton;
        public InGameData inGameData;
        public List<TilesToCollectUI> tilesToCollectUI;
        public bool Pause {get; set;}
        public Coroutine TimerCoroutine;
        private AsyncOperation _loadingOperation;
        private Sequence _loadingSequence;
        public bool startTimers;
        
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

            if (SceneManager.GetActiveScene().name == GlobalData.IN_GAME_SCENE) SetTipsAmount();
        }
        private IEnumerator Start()
        {
            UnityEngine.Device.Application.targetFrameRate = 60;
                                        
                var loadingSequence = DOTween.Sequence();
                
                var posY = (float)UnityEngine.Device.Screen.height;
                Debug.Log(posY);
                loadingSequence.Join(_loadingScreen.LoadingScreenObject.gameObject.transform.DOMoveY(posY * 0.005f, 1f))
                                .AppendCallback(() => _loadingScreen.LoadingScreenObject.gameObject.SetActive(false));
                
                if (SceneManager.GetActiveScene().name == GlobalData.IN_GAME_SCENE)
                {
                    yield return new WaitUntil(() => startTimers);
                }
                
                yield return loadingSequence.Play().WaitForCompletion();
                
                              
            yield return null;
        }

        public void SetTipsAmount()
        {
            var currentTipsAmount = PlayerPrefs.GetInt(GlobalData.AVAILABLE_TIPS, 0);
            tipButton.TipsAmount.SetText(currentTipsAmount.ToString());
            tipButton.Button.interactable = currentTipsAmount > 0;
        }

        public void OnLevelComplete(LevelData currentLvel)
        {
            //TogglePause();
            if (TimerCoroutine != null) StopCoroutine(TimerCoroutine);

            _levelCompletedTab.LevelCompletTabObject.SetActive(true);
            _levelCompletedTab.SetStars(inGameData.TimerData.StarsImages.Count);

            soundManager.PlaySound(GlobalData.AudioClipType.OnWin);

            var onStarsComleted = inGameData.TimerData.StarsImages.Count;
            var levelNumber = PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL);
            GlobalData.OnLevelComplet(levelNumber, onStarsComleted);
            GlobalData.AddAvailableTips(1);
            PlayerPrefs.Save();
           
        }
        public void OnGameOver(float num)
        {
            if (TimerCoroutine != null) StopCoroutine(TimerCoroutine);
            ToggleObject(_gameOverTab);
            soundManager.PlaySound(GlobalData.AudioClipType.OnLose);
            TogglePause();
        }

        public void ChangeScene(string sceneName)
        {
            _loadingSequence = DOTween.Sequence();
            
            _loadingSequence.Join(_loadingScreen.LoadingScreenObject.gameObject.transform.DOMoveY(0, 1f));

            _loadingScreen.LoadingScreenObject.SetActive(true);

            Instance._loadingOperation = SceneManager.LoadSceneAsync(sceneName);
            Instance._loadingOperation.allowSceneActivation = false;

            StartCoroutine(OnLoadingSceen());
        }
        public void ToggleObject(GameObject objectToToggle)
        {
            objectToToggle.SetActive(objectToToggle.activeSelf ? false : true);
        }
        public void TogglePause()
        {
            Pause = Pause ? false : true;
        }
        public void NextLevel()
        {
            var currentLevel = PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL);
            PlayerPrefs.SetInt(GlobalData.LAST_PLAYED_LEVEL, currentLevel + 1);
            PlayerPrefs.Save();
            ChangeScene(GlobalData.IN_GAME_SCENE);
        }
        public IEnumerator UITimerCorutine(float timeOnTwoStars, float timeOnThreeStars)
        {
            var starsLeft = 3;
            var timeLeft = timeOnThreeStars;
            var normNumber = timeOnThreeStars;
            //var timerPointOnThreeStars = (timeOnTwoStars - timeOnThreeStars) * 0.5 / timeOnTwoStars;
            
            while (starsLeft > 0)
            {
                timeLeft -= Time.smoothDeltaTime;
                var normalizedTime = Mathf.Clamp((timeLeft / normNumber) * 0.5f, 0.0f, 1.0f);

                inGameData.TimerData.TimerImage.fillAmount = normalizedTime;
                
                if (Pause) yield return new WaitWhile(() => Pause);
                
                /*
                if (inGameData.TimerImage.fillAmount <= (float)Math.Round(timerPointOnThreeStars, 3) && inGameData.OffStar(2))
                {
                    //inGameData.OffStar(2);
                }
                */
                if (inGameData.TimerData.TimerImage.fillAmount <= 0.0f && inGameData.TimerData.OffStar(2))
                {
                    starsLeft--;
                    timeLeft = normNumber = starsLeft == 2 ? timeOnTwoStars : 0;
                }
                else if (inGameData.TimerData.TimerImage.fillAmount <= 0.0f && inGameData.TimerData.OffStar(1))
                {
                    starsLeft--;
                    timeLeft = normNumber = starsLeft == 1 ? Mathf.Abs(timeOnThreeStars - timeOnTwoStars) : 0;
                }
                else if (inGameData.TimerData.TimerImage.fillAmount <= 0.0f && inGameData.TimerData.OffStar(0))
                {
                    starsLeft--;
                    timeLeft = normNumber = 0;
                }
                yield return null;
            }
            Debug.Log("Game Over");
            OnGameOver(0);
            
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

        private IEnumerator OnLoadingSceen()
        {
            yield return _loadingSequence.Play().WaitForCompletion();

            _loadingOperation.allowSceneActivation = true;
            while (_loadingScreen.LoadingImage.fillAmount <= 1)
            {
                _loadingScreen.LoadingImage.fillAmount = _loadingOperation.progress;
                yield return new WaitForEndOfFrame();
            }
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
        public struct SettingsTab
        {
            [Header ("AllAudioSetting")]
            public Image MasterSoundButtonImage;
            public Sprite SoundOnSprite;
            public Sprite SoundOffSprite;
            public Slider MasterVolumeSlider;
            [Space]
            [Header ("MusicSetting")]
            public Image MusicButtonImage;
            public Sprite MusicOnSprite;
            public Sprite MusicOffSprite;
            public Slider MusicVolumeSlider;
        }
        [Serializable]
        public struct InGameData
        {
            public TimerData TimerData;
            public TextMeshProUGUI LevelSerialNumber;
            //public Image TimerImage;
            //public List<Image> StarsImages;
            //public Sprite GreyStarSprite;
            //public TextMeshProUGUI ScoreMultiplier;
            public TextMeshProUGUI TotalScoreValue;
            
        }
        [Serializable]
        public struct TimerData
        {
            public Image TimerImage;
            public List<Sprite> TimerSprites;
            public List<Image> StarsImages;
            public Sprite GreyStarSprite;
            public bool OffStar(int starSerialNumber)
            {
                if (starSerialNumber < StarsImages.Count)
                {
                    StarsImages[starSerialNumber].sprite = GreyStarSprite;
                    StarsImages.RemoveAt(starSerialNumber);
                    TimerImage.sprite = TimerSprites[starSerialNumber];
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
        [Serializable]
        public struct BackgroundMusic
        {
            public AudioSource AudioSource;
            public AudioClip[] AudioClips;     
        }
        [Serializable]
        public struct LoadingScreen
        {
            public GameObject LoadingScreenObject;
            public Animator Animator;
            public Image LoadingImage;
        }
        [Serializable]
        public struct StartingScreen
        {
            public GameObject StartingScreenObject;
            public TextMeshProUGUI StartingScreenText;
        }
    }
}
