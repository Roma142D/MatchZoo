using System;
using System.Collections;
using DG.Tweening;
using MatchThreeEngine;
using UI;
using UnityEngine;

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
            //SetTutorialStep(0);
            if (_tutorialStep > 1)
            {
                _board.CurrentLevelData.levelType = LevelType.BeatTime;
                _tutorialLevelData.StartingScreenWelcoming.SetActive(false);
                _tutorialLevelData.StartingScreenStepTwo.SetActive(true);
            } 
        }
        private IEnumerator Start()
        {
            
            //_tutorialStep = PlayerPrefs.GetInt(GlobalData.TUTORIAL_STEP, 0);

            yield return new WaitUntil(() => UIManager.Instance.startTimers);
            _tutorialLevelData.StartingScreenWelcoming.SetActive(false);

            Debug.Log(_tutorialStep);
            if (_tutorialStep >= 2)
            {
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
                SetTutorialStep(_tutorialStep += 1);
                TimerTutorCoroutine();
                NextTutorialStep(_tutorialStep);
            }
            yield return new WaitForSeconds(0.5f);
            //SetTutorialStep(_tutorialStep += 1);
            //TimerTutorCoroutine();
            //NextTutorialStep(_tutorialStep);
        }

        public void SetTutorialStep(int step)
        {
            _tutorialStep = step;
            PlayerPrefs.SetInt(GlobalData.TUTORIAL_STEP, _tutorialStep);
            PlayerPrefs.Save();
        }

        public void NextTutorialStep(int step)
        {
            SetTutorialStep(step);
            Debug.Log($"Next Tutorial Step: {_tutorialStep}");

            switch (_tutorialStep)
            {
                case 1:
                    StartCoroutine(TimerTutorCoroutine());
                    break;
                case 2:
                    StartCoroutine(LevelGoalsTutorCoroutineOne());
                    break;
                case 3:
                    StartCoroutine(LevelGoalsTutorCoroutineTwo());
                    break;
                case 4:
                    StartCoroutine(HintTutorCoroutine());
                    
                    break;
                case 5:
                    StartCoroutine(SpecialTilesTutorCoroutine());
                    break;
            }
        }
        
        private IEnumerator TimerTutorCoroutine()
        {
            UIManager.Instance.Pause = true;

            _tutorialLevelData.TimerTutorTab.SetActive(true);

            StartCoroutine(ArrowAnimation(_tutorialLevelData.TimerTutorArrow));
            
            yield return new WaitUntil(() => UIManager.Instance.Pause == false);
            //SetTutorialStep(_tutorialStep += 1);
            _tutorialLevelData.TimerTutorTab.SetActive(false);
        }

        private IEnumerator LevelGoalsTutorCoroutineOne()
        {
            yield return new WaitForSeconds(2f);

            UIManager.Instance.Pause = true;

            _tutorialLevelData.LevelGoalsTutorTabOne.SetActive(true);
            
            StartCoroutine(ArrowAnimation(_tutorialLevelData.LevelGoalsTutorArrowOne));
            
            yield return new WaitUntil(() => UIManager.Instance.Pause == false);
            //SetTutorialStep(_tutorialStep += 1);
            _tutorialLevelData.LevelGoalsTutorTabOne.SetActive(false);
        }
        private IEnumerator LevelGoalsTutorCoroutineTwo()
        {
            yield return new WaitForSeconds(2f);

            UIManager.Instance.Pause = true;

            _tutorialLevelData.LevelGoalsTutorTabTwo.SetActive(true);
            
            StartCoroutine(ArrowAnimation(_tutorialLevelData.LevelGoalsTutorArrowTwo));
            
            yield return new WaitUntil(() => UIManager.Instance.Pause == false);
            //SetTutorialStep(_tutorialStep += 1);
            _tutorialLevelData.LevelGoalsTutorTabTwo.SetActive(false);
        }
        private IEnumerator HintTutorCoroutine()
        {
            yield return new WaitForSeconds(2f);

            UIManager.Instance.Pause = true;

            _tutorialLevelData.HintTutorTab.SetActive(true);
            
            StartCoroutine(ArrowAnimation(_tutorialLevelData.HintTutorArrow));
            
            yield return new WaitUntil(() => UIManager.Instance.Pause == false);
            //SetTutorialStep(_tutorialStep += 1);
            _tutorialLevelData.HintTutorTab.SetActive(false);
        }
        private IEnumerator SpecialTilesTutorCoroutine()
        {
            yield return new WaitForSeconds(2f);

            UIManager.Instance.Pause = true;

            _tutorialLevelData.SpecialTilesTutorTab.SetActive(true);
            
            yield return new WaitUntil(() => UIManager.Instance.Pause == false);
            //SetTutorialStep(_tutorialStep += 1);
            _tutorialLevelData.SpecialTilesTutorTab.SetActive(false);
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
