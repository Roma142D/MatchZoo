using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace MatchThreeEngine
{
	public sealed class Board : MonoBehaviour
	{
		//[SerializeField] private Button _helpButton;
		[SerializeField] private AllLvelsData _levelsData;
		[SerializeField] private AudioClip matchSound;
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private Slider _slider;
		//[SerializeField] private Image _handleImage;
		[SerializeField] private float tweenDuration;

		//[SerializeField] private Transform swappingOverlay;

		[SerializeField] private bool ensureNoStartingMatches;
		[SerializeField] private Item[] _specialTilesTypes;
		private List<Row> _rows;
		private Item[] _currentTilesTypes;
		private bool startTimer;
		private float _scoreMultiplier;
		private int _multiplierThreshold = 100;
		public LevelData CurrentLevelData
		{
			get
			{
				return _levelsData.LevelsData[PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL, 0)];
			}
		}

		private readonly List<Tile> _selection = new List<Tile>();
		private List<LevelData.TilesToCollect> _currentTilesToCollect;

		private bool _isSwapping;
		private bool _isMatching;
		private bool _isShuffling;
		
		private float _currentScore;
		private float _totalTime;
		public float TotalScore {get; private set;}
		
		public event Action<Item[]> OnMatch;
		public static Action<float> OnGameOver;
        public static Action<LevelData> OnLevelComplet;

		private Coroutine _checkScoreRoutine;

		private TileData[,] Matrix
		{
			get
			{
				var width = _rows.Max(row => row.tiles.Count);
				var height = _rows.Count;

				var data = new TileData[width, height];
/*
				for (var y = 0; y < height; y++)
					for (var x = 0; x < width; x++)
						data[x, y] = GetTile(x, y).Data;

*/
				for (var y = 0; y < _rows.Count; y++)
				{
					for (var x = 0; x < _rows[y].tiles.Count; x++)
					{
						data[x, y] = GetTile(x, y).Data;
					}
				}
				return data;
			}
		}
		
		private void Start()
		{
			Debug.Log(CurrentLevelData.name);
			_rows = new List<Row>(CurrentLevelData.GenerateBoard(transform));
			_currentTilesTypes = CurrentLevelData.tilesTypes.ToArray();
			var levelNumber = (PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL, 0) + 1);
			UIManager.Instance.inGameData.LevelSerialNumber.SetText($"lvl {levelNumber}");

			_scoreMultiplier = 1;

			Debug.Log($"height{_rows.Count} width{_rows[0].tiles.Count} \n{_currentTilesTypes.Length}");
			for (var y = 0; y < _rows.Count; y++)
			{
				//_rows.Max(row => row.tiles.Count)
				for (var x = 0; x < _rows[y].tiles.Count; x++)
				{
					var tile = GetTile(x, y);

					//tile.x = x;
					//tile.y = y;

					tile.Type = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];

					tile.button.onClick.AddListener(() => Select(tile));
				}
			}

			if (ensureNoStartingMatches) StartCoroutine(EnsureNoStartingMatches());
			
			
			OnLevelComplet += UIManager.Instance.OnLevelComplete;
			OnGameOver += UIManager.Instance.OnGameOver;
			UIManager.Instance.tipButton.Button.onClick.AddListener(GetTip);

			switch (CurrentLevelData.levelType)
			{
				case LevelType.BeatTime:
					_slider.gameObject.SetActive(true);
					StartCoroutine(StartTimer());
					UIManager.Instance.TimerCoroutine = 
						StartCoroutine(UIManager.Instance.UITimerCorutine(CurrentLevelData.TimeOnTwoStars));
					OnMatch += (data) => IncreaseScore(data);
					break;
				case LevelType.CollectTiles:
					UIManager.Instance.TimerCoroutine = 
						StartCoroutine(UIManager.Instance.UITimerCorutine(CurrentLevelData.TimeOnTwoStars));
					_currentTilesToCollect = new List<LevelData.TilesToCollect>(CurrentLevelData.tilesToCollect);
					UIManager.Instance.SetTilesToCollect(CurrentLevelData.tilesToCollect);
					OnMatch += (data) => CheckCollectedTiles(data);
					break;
			}		
		}
		
		private void Update()
		{
			if (startTimer && !UIManager.Instance.Pause) TimerCooldown();
		}

		private void GetTip()
		{
			var bestMove = TileDataMatrixUtility.FindBestMove(Matrix);

			if (bestMove != null && GlobalData.UseTip())
			{
				UIManager.Instance.SetTipsAmount();
				Select(GetTile(bestMove.X1, bestMove.Y1));
				Select(GetTile(bestMove.X2, bestMove.Y2));
				//var tile1 = GetTile(bestMove.X1, bestMove.Y1);
				//var tile2 = GetTile(bestMove.X2, bestMove.Y2);
				
				//await SwapAsync(tile1, tile2, true);
			}
		}
		private IEnumerator StartTimer()
		{
			_slider.maxValue = CurrentLevelData.scoreToBeet;
			_slider.minValue = -CurrentLevelData.TimerStartingTime;
			yield return new WaitForSeconds(3f);

			startTimer = true;
		}

		private IEnumerator EnsureNoStartingMatches()
		{
			var wait = new WaitForEndOfFrame();

			while (TileDataMatrixUtility.FindBestMatch(Matrix) != null)
			{
				Shuffle();

				yield return wait;
			}
		}
		private void IncreaseScore(Item[] matchedTiles)
		{	
			var tilesValue = matchedTiles.Select(tile => tile.Value).ToArray().Sum();
			
			Debug.Log(tilesValue);
			_slider.value = _currentScore += tilesValue * _scoreMultiplier;
			
			TotalScore += tilesValue * _scoreMultiplier;
			UIManager.Instance.inGameData.TotalScoreValue.SetText(((int)TotalScore).ToString());
			
			if (TotalScore > _multiplierThreshold)
			{
				_multiplierThreshold += 100;
				_scoreMultiplier += 0.2f;
			} 	
			if (_checkScoreRoutine == null)
			{
				_checkScoreRoutine = StartCoroutine(CheckScore());
			}
		}

		private IEnumerator CheckScore()
		{
			switch (CurrentLevelData.levelType)
			{
				case LevelType.BeatTime:
					if (_currentScore >= _slider.maxValue)
					{
					UIManager.Instance.Pause = true;
					startTimer = false;
					yield return new WaitUntil(() => !_isMatching);
					GlobalData.AddAvailableTips(2);
					
					OnLevelComplet?.Invoke(CurrentLevelData);
					}
				break;
				case LevelType.CollectTiles:
					UIManager.Instance.Pause = true;
					yield return new WaitUntil(() => !_isMatching);
					GlobalData.AddAvailableTips(2);
					
					OnLevelComplet?.Invoke(CurrentLevelData);
				break;
			}
			
			_checkScoreRoutine = null;
		}

		private void CheckCollectedTiles(Item[] matchedTiles)
		{
			for (int i = 0; i < _currentTilesToCollect.Count; i++)
			{
				var tile = _currentTilesToCollect[i];
				var fittingTiles = matchedTiles.Count(matchedTile => matchedTile.id == tile.TileType.id);

				tile.AmountToCollect -= fittingTiles;
				_currentTilesToCollect[i] = tile;
				UIManager.Instance.CollectTile(i, tile.AmountToCollect);
			}
			if (_currentTilesToCollect.All(tile => tile.AmountToCollect <= 0))
			{
				_checkScoreRoutine = StartCoroutine(CheckScore());
			}
			/*
			for (int i = 0; i < _currentTilesToCollect.Count; i++)
			{
				var tile = _currentTilesToCollect[i];
				if (tile.AmountToCollect > 0)
				{
					break;
				}
				else
				{
					if (i == _currentTilesToCollect.Count - 1)
					{
						_checkScoreRoutine = StartCoroutine(CheckScore());
					}
					continue;
				}
			}
			*/
		}

		private void TimerCooldown()
		{
			_slider.value -= Time.smoothDeltaTime;
			_currentScore -= Time.smoothDeltaTime;
			_totalTime += Time.deltaTime;
			if (_slider.value <= _slider.minValue)
			{
				Debug.Log("LOSE");
				startTimer = false;
				OnGameOver.Invoke(TotalScore);
			}
		}
		private Tile GetTile(int x, int y) => _rows[y].tiles[x];
		
		private Tile[] GetTiles(IList<TileData> tileData)
		{
			var length = tileData.Count;

			var tiles = new Tile[length];

			for (var i = 0; i < length; i++) tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

			return tiles;
		}

		private async void Select(Tile tile)
		{
			var goToOrigin = DOTween.Sequence();
            var highlightSequence = DOTween.Sequence();
            
            
            highlightSequence.Join(tile.icon.transform.DOScale(new Vector3 (1.3f, 1.3f, 1.3f), tweenDuration));

			if (_isSwapping || _isMatching || _isShuffling) return;

			if (!_selection.Contains(tile))
			{
				if (_selection.Count > 0)
				{
					if (Math.Abs(tile.x - _selection[0].x) == 1 && Math.Abs(tile.y - _selection[0].y) == 0
					    || Math.Abs(tile.y - _selection[0].y) == 1 && Math.Abs(tile.x - _selection[0].x) == 0)
						_selection.Add(tile);
					goToOrigin.Join(_selection[0].icon.transform.DOScale(Vector3.one, tweenDuration));
				}
				else
				{
					_selection.Add(tile);
					highlightSequence.Play().Complete();
				}
			}
			else 
			{
				goToOrigin.Join(_selection[0].icon.transform.DOScale(Vector3.one, tweenDuration));
				goToOrigin.Play().Complete();
				_selection.Clear();
			}

			if (_selection.Count < 2) return;

			await SwapAsync(_selection[0], _selection[1], false);
			goToOrigin.Play().Complete();

			if (!await TryMatchAsync(tile.Execute(Matrix))) await SwapAsync(_selection[0], _selection[1], false);

			var matrix = Matrix;

			while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null)
			{
				Shuffle();

				matrix = Matrix;
			}

			_selection.Clear();
		}

		private async Task SwapAsync(Tile tile1, Tile tile2, bool justShow)
		{
			_isSwapping = true;

			var icon1 = tile1.icon;
			var icon2 = tile2.icon;

			var icon1Transform = icon1.transform;
			var icon2Transform = icon2.transform;

			//icon1Transform.SetParent(swappingOverlay);
			//icon2Transform.SetParent(swappingOverlay);

			//icon1Transform.SetAsLastSibling();
			//icon2Transform.SetAsLastSibling();

			var sequence = DOTween.Sequence();

			sequence.Join(icon1Transform.DOMove(icon2Transform.position, tweenDuration).SetEase(Ease.OutBack))
			        .Join(icon2Transform.DOMove(icon1Transform.position, tweenDuration).SetEase(Ease.OutBack));

			await sequence.Play()
			              .AsyncWaitForCompletion();

			if (!justShow)
			{
				icon1Transform.SetParent(tile2.transform);
				icon2Transform.SetParent(tile1.transform);

				tile1.icon = icon2;
				tile2.icon = icon1;

				var tile1Item = tile1.Type;

				tile1.Type = tile2.Type;

				tile2.Type = tile1Item;
			}
			else
			{
				var swapBackSequence = DOTween.Sequence();
				swapBackSequence.Join(icon1Transform.DOMove(tile1.transform.position, tweenDuration).SetEase(Ease.OutBack))
								.Join(icon2Transform.DOMove(tile2.transform.position, tweenDuration).SetEase(Ease.OutBack));

				await swapBackSequence.Play()
									  .AsyncWaitForCompletion();								
			}

			_isSwapping = false;
		}

		private async Task<bool> TryMatchAsync(Match explosion)
		{
			var didMatch = false;

			_isMatching = true;

			var match = explosion != null ? explosion : TileDataMatrixUtility.FindBestMatch(Matrix);

			var matchedTiles = new List<Item>();

			while (match != null)
			{
				didMatch = true;

				var tiles = GetTiles(match.Tiles);

				var deflateSequence = DOTween.Sequence();

				foreach (var tile in tiles)
				{
					deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));
					matchedTiles.Add(tile.Type);
				}

				audioSource.PlayOneShot(matchSound);

				await deflateSequence.Play()
				                     .AsyncWaitForCompletion();

				var inflateSequence = DOTween.Sequence();

				for (int i = 0; i < tiles.Length; i++)
				{
					var tile = tiles[i];
					Item item;
					if (match != explosion)
					{
						item = i == 3 ? _specialTilesTypes[Random.Range(0, _specialTilesTypes.Length)]
								: _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];			
					}
					else
					{
						item = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
					}
                    tile.Type = item;

					inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));
				}
				
				await inflateSequence.Play()
				                     .AsyncWaitForCompletion();


				match = TileDataMatrixUtility.FindBestMatch(Matrix);
			}
			OnMatch?.Invoke(matchedTiles.ToArray());

			_isMatching = false;

			return didMatch;
		}

		private void Shuffle()
		{
			_isShuffling = true;

			foreach (var row in _rows)
				foreach (var tile in row.tiles)
					tile.Type = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];

			_isShuffling = false;
		}

		private void OnDestroy()
		{
			OnMatch -= (data) => IncreaseScore(data);
			OnLevelComplet -= UIManager.Instance.OnLevelComplete;
			OnGameOver -= UIManager.Instance.OnGameOver;
		}
		private void OnDisable()
		{
			OnMatch -= (data) => IncreaseScore(data);
			OnLevelComplet -= UIManager.Instance.OnLevelComplete;
			OnGameOver -= UIManager.Instance.OnGameOver;
		}

	}
}
