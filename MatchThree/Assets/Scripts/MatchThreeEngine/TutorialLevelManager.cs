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

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => UIManager.Instance.startTimers);

            Debug.Log("Start Tutorial Level");

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
                Debug.Log(_board.Swiping);
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
                                       
        }

        private void OnDisable()
        {
            DOTween.KillAll(true);
        }
    }
}
