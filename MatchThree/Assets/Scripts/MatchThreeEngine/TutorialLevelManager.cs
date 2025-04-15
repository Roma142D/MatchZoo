using System;
using System.Collections;
using DG.Tweening;
using MatchThreeEngine;
using UI;
using UnityEngine;
using UnityEngine.UIElements;

namespace MatchThreeEngine
{
    public class TutorialLevelManager : MonoBehaviour
    {
        [SerializeField] private Board _board;
        [SerializeField] private TutorialLevelData _tutorialLevelData;
        private int _tutorialStep = 0;

        private void Awake()
        {
            _tutorialStep = PlayerPrefs.GetInt(GlobalData.TUTORIAL_STEP, 0);
            
            if (_tutorialStep > 1)
            {
                _board.CurrentLevelData.levelType = LevelType.BeatTime;
                _tutorialLevelData.StartingScreenWelcoming.SetActive(false);
                _tutorialLevelData.StartingScreenStepTwo.SetActive(true);
            } 
        }
        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UIManager.Instance.startTimers);
            _tutorialLevelData.StartingScreenWelcoming.SetActive(false);

            Debug.Log(_tutorialStep);
            if (_tutorialStep >= 2)
            {
                _tutorialLevelData.StartingScreenStepTwo.SetActive(false);
                SetTutorialStep(3);
                NextTutorialStep(_tutorialStep);
            }
            else
            {
                var bestMove = TileDataMatrixUtility.FindBestMove(_board.Matrix);
                var tile1 = _board.GetTile(bestMove.X1, bestMove.Y1);
                var tile2 = _board.GetTile(bestMove.X2, bestMove.Y2);

                var icon1 = tile1.icon;
                var icon2 = tile2.icon;

                var icon1Transform = icon1.transform;
                var icon2Transform = icon2.transform;

                Debug.Log($"Best Move: {bestMove.X1}, {bestMove.Y1} - {bestMove.X2}, {bestMove.Y2}");

                Debug.Log(_board.Swiping);
                
                while (!_board.Swiping)
                {
                    var SwipeSequence = DOTween.Sequence();
                    var SwipeBackSequence = DOTween.Sequence();


                    SwipeSequence.Join(icon1Transform.DOMove(icon2Transform.position, _board.TweenDuration).SetEase(Ease.OutBack))
                                .Join(icon2Transform.DOMove(icon1Transform.position, _board.TweenDuration).SetEase(Ease.OutBack));

                    SwipeBackSequence.Join(icon1Transform.DOMove(icon1.transform.position, _board.TweenDuration).SetEase(Ease.OutBack))
                                .Join(icon2Transform.DOMove(icon2.transform.position, _board.TweenDuration).SetEase(Ease.OutBack));

                    SwipeSequence.Play().WaitForCompletion();
                    yield return new WaitForSeconds(_board.TweenDuration);
                    SwipeBackSequence.Play().WaitForCompletion();
                    yield return new WaitForSeconds(2f);
                }
                //SetTutorialStep(_tutorialStep += 1);
                NextTutorialStep(_tutorialStep + 1);
            }
            yield return new WaitForSeconds(0.5f);
        }

        public void SetTutorialStep(int step)
        {
            _tutorialStep = step;
            PlayerPrefs.SetInt(GlobalData.TUTORIAL_STEP, _tutorialStep);
            PlayerPrefs.Save();
            if (step == 0) _board.CurrentLevelData.levelType = LevelType.CollectTiles;
        }

        public void NextTutorialStep(int step)
        {
            SetTutorialStep(step);
            Debug.Log($"Next Tutorial Step: {_tutorialStep}");
            StartCoroutine(PlayTutorStep(_tutorialStep));
            //SetTutorialStep(_tutorialStep += 1);
        }
        
        private IEnumerator PlayTutorStep(int step)
        {
            yield return new WaitForSeconds(0.25f);
            UIManager.Instance.Pause = true;
            var tab = _tutorialLevelData.GetTabByStep(step);
            var arrow = _tutorialLevelData.GetArrowByStep(step);

            TabAnimation(tab);
            
            if (arrow != null) StartCoroutine(ArrowAnimation(arrow));
            
            yield return new WaitUntil(() => UIManager.Instance.Pause == false);
            
            tab.SetActive(false);
        }

        private void TabAnimation(GameObject tab)
        {
            tab.SetActive(true);
            tab.transform.localScale = Vector3.zero;
            var tabSequence = DOTween.Sequence();
            tabSequence.Join(tab.transform.DOScale(1f, 0.5f));

            tabSequence.Play().WaitForCompletion();
        }

        private IEnumerator ArrowAnimation(GameObject arrow)
        {
            while (UIManager.Instance.Pause)
            {
                var arrowSequence = DOTween.Sequence();
                var arrowSequenceBack = DOTween.Sequence();
                arrowSequence.Join(arrow.transform.DOScale(1.25f, 0.5f));
                arrowSequenceBack.Join(arrow.transform.DOScale(0.75f, 0.5f));
                
                yield return arrowSequence.Play().WaitForCompletion();
                yield return arrowSequenceBack.Play().WaitForCompletion();
            } 
        }

        private void OnDisable()
        {
            DOTween.KillAll(true);
        }

        [Serializable]
        public struct TutorialLevelData
        {
            public GameObject GetTabByStep(int step)
            {
                switch (step)
                {
                    case 1:
                        return TimerTutorTab;
                    case 2:
                        return LevelGoalsTutorTabOne;
                    case 3:
                        return LevelGoalsTutorTabTwo;
                    case 4:
                        return HintTutorTab;
                    case 5:
                        return SpecialTilesTutorTab;
                    default:
                        return null;
                }
            }
            public GameObject GetArrowByStep(int step)
            {
                switch (step)
                {
                    case 1:
                        return TimerTutorArrow;
                    case 2:
                        return LevelGoalsTutorArrowOne;
                    case 3:
                        return LevelGoalsTutorArrowTwo;
                    case 4:
                        return HintTutorArrow;
                    default:
                        return null;
                }
            }
            public GameObject StartingScreenWelcoming;
            public GameObject StartingScreenStepTwo;
            public GameObject TimerTutorTab;
            public GameObject TimerTutorArrow;
            public GameObject LevelGoalsTutorTabOne;
            public GameObject LevelGoalsTutorArrowOne;
            public GameObject LevelGoalsTutorTabTwo;
            public GameObject LevelGoalsTutorArrowTwo;
            public GameObject HintTutorTab;
            public GameObject HintTutorArrow;
            public GameObject SpecialTilesTutorTab;
        }
    }
}
