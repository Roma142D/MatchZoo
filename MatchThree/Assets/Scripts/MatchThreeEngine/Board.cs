﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace MatchThreeEngine
{
	public sealed class Board : MonoBehaviour
	{
		[SerializeField]private GraphicRaycaster _raycaster;
		[SerializeField] private float _minSwipeLength;
		[SerializeField] private AllLvelsData _levelsData;
		[SerializeField] private Slider _slider;
		//[SerializeField] private Image _handleImage;
		[SerializeField] private float tweenDuration;

		//[SerializeField] private Transform swappingOverlay;

		[SerializeField] private bool _ensureNoStartingMatches;
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
		private bool _isSwiping;
		
		private float _currentScore;
		private float _totalTime;
		public float TotalScore {get; private set;}
		
		public event Action<Tile[]> OnMatch;
		public static Action<float> OnGameOver;
        public static Action<LevelData> OnLevelComplet;

		private Coroutine _checkScoreRoutine;

		private MyInputControler _inputControler;

		private EventSystem _eventSystem;
		private PointerEventData _pointerEventData;
        private Vector2 _startSwipePosition;
		private Vector2 _endSwipePosition;

		private bool _startTimer = false;
		
		private List<Match> _explosionMatches = new List<Match>();

        private TileData[,] Matrix
		{
			get
			{
				var width = _rows.Max(row => row.tiles.Count);
				var height = _rows.Count;

				var data = new TileData[width, height];

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
		
		private void Awake()
		{
			_eventSystem = EventSystem.current;
						
			_inputControler = new MyInputControler();
			_inputControler.Enable();
			_inputControler.Touchscreen.Touch.started += StartSwipe;
			_inputControler.Touchscreen.Touch.canceled += SwipeTile;
		}
		private IEnumerator Start()
		{
			//Time.timeScale = 0.5f;
			
			_rows = new List<Row>(CurrentLevelData.GenerateBoard(transform));
			_currentTilesTypes = CurrentLevelData.tilesTypes.ToArray();
			var levelNumber = (PlayerPrefs.GetInt(GlobalData.LAST_PLAYED_LEVEL, 0) + 1);
			UIManager.Instance.inGameData.LevelSerialNumber.SetText($"lvl {levelNumber}");
			
			UIManager.Instance.backgroundImage.sprite = CurrentLevelData.background != null ? CurrentLevelData.background 
																							: UIManager.Instance.backgroundImage.sprite;

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

					//tile.button.onClick.AddListener(() => Select(tile));
				}
			}

			if (_ensureNoStartingMatches) StartCoroutine(EnsureNoStartingMatches());
			
			
			OnLevelComplet += UIManager.Instance.OnLevelComplete;
			OnGameOver += UIManager.Instance.OnGameOver;
			UIManager.Instance.tipButton.Button.onClick.AddListener(GetTip);

			yield return new WaitUntil(() => UIManager.Instance.startTimers);

			switch (CurrentLevelData.levelType)
			{
				case LevelType.BeatTime:
					_slider.gameObject.SetActive(true);
					StartCoroutine(StartTimer());
					UIManager.Instance.TimerCoroutine = 
						StartCoroutine(UIManager.Instance.UITimerCorutine(CurrentLevelData.TimeOnTwoStars, CurrentLevelData.TimeOnThreeStars));
					OnMatch += (data) => IncreaseScore(data);
					break;
				case LevelType.CollectTiles:
					UIManager.Instance.TimerCoroutine = 
						StartCoroutine(UIManager.Instance.UITimerCorutine(CurrentLevelData.TimeOnTwoStars, CurrentLevelData.TimeOnThreeStars));
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
		private IEnumerator StartCountDown()
		{
			var number = 3;
			
			var endSequence = DOTween.Sequence();

			while (number > 0)
			{
				var countDownSequence = DOTween.Sequence();

				countDownSequence.Join(UIManager.Instance.startingScreen.StartingScreenText.DOFade(1, tweenDuration))
								.Join(UIManager.Instance.startingScreen.StartingScreenText.gameObject.transform.DOScale(1.5f, tweenDuration))
								.AppendCallback(() => UIManager.Instance.startingScreen.StartingScreenText.SetText(number.ToString()))
								.Join(UIManager.Instance.startingScreen.StartingScreenText.DOFade(0.5f, tweenDuration))
								.Join(UIManager.Instance.startingScreen.StartingScreenText.gameObject.transform.DOScale(0.5f, tweenDuration));
				
				yield return countDownSequence.Play().WaitForCompletion();

				number--;
			}
			
			UIManager.Instance.startingScreen.StartingScreenText.gameObject.SetActive(false);
			
			UIManager.Instance.startTimers = true;
		}
        private void StartSwipe(InputAction.CallbackContext context)
        {
			if (UIManager.Instance.Pause || _isSwapping || _isMatching || _isShuffling || _isSwiping) return;
			Debug.Log("StartSwipe");
			_isSwiping = true;

			if(_startTimer == false) StartCoroutine(StartCountDown());
 			
            _startSwipePosition = _inputControler.Touchscreen.Swipe.ReadValue<Vector2>();
			
			
			_pointerEventData = new PointerEventData(_eventSystem)
			{
				position = _startSwipePosition
			};
			
			List<RaycastResult> startResults = new List<RaycastResult>();
			_raycaster.Raycast(_pointerEventData, startResults);
			
			if (startResults.Any(result => result.gameObject.TryGetComponent(out Tile tile)))
			{
				var tile = startResults.First(result => result.gameObject.TryGetComponent(out Tile tile)).gameObject.GetComponent<Tile>();
				Select(tile);
			}
			Debug.Log("EndStartSwipe");
        }

        public void SwipeTile(InputAction.CallbackContext context)
        {
			Debug.Log("SwipeTile");
			_endSwipePosition = _inputControler.Touchscreen.Swipe.ReadValue<Vector2>();
			
			var swipeVector = _endSwipePosition - _startSwipePosition;
			var selectedTile = _selection.FirstOrDefault();
			if (swipeVector.magnitude > _minSwipeLength && selectedTile != null)
			{
				if (Math.Abs(swipeVector.x) > Math.Abs(swipeVector.y))
				{
					if (_endSwipePosition.x > _startSwipePosition.x)
					{
						var rightPosition = TileDataMatrixUtility.GetNeighborTileCoordinates(GlobalData.Direction.RIGHT, selectedTile.Data, Matrix);
						Select(GetTile(rightPosition));
					}
					else if (_endSwipePosition.x < _startSwipePosition.x)
					{
						var leftPosition = TileDataMatrixUtility.GetNeighborTileCoordinates(GlobalData.Direction.LEFT, selectedTile.Data, Matrix);
						Select(GetTile(leftPosition));
					}
				}
				else
				{
					if (_endSwipePosition.y > _startSwipePosition.y)
					{
						var upPosition = TileDataMatrixUtility.GetNeighborTileCoordinates(GlobalData.Direction.UP, selectedTile.Data, Matrix);
						Select(GetTile(upPosition));
					}
					else if (_endSwipePosition.y < _startSwipePosition.y)
					{
						var downPosition = TileDataMatrixUtility.GetNeighborTileCoordinates(GlobalData.Direction.DOWN, selectedTile.Data, Matrix);
						Select(GetTile(downPosition));
					}
				}
			}
			Debug.Log("EndSwipeTile");
			_isSwiping = false;
		}

		private void GetTip()
		{
			if (_isMatching || _isSwapping || _isShuffling) return;
			//UIManager.Instance.tipButton.Button.interactable = false;
			var bestMove = TileDataMatrixUtility.FindBestMove(Matrix);

			if (bestMove != null && GlobalData.UseTip())
			{
				UIManager.Instance.SetTipsAmount();
				Select(GetTile(bestMove.X1, bestMove.Y1));
				Select(GetTile(bestMove.X2, bestMove.Y2));
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
		private void IncreaseScore(Tile[] matchedTiles)
		{	
			var tilesValue = matchedTiles.Select(tile => tile.Type.Value).ToArray().Sum();
			
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

		private void CheckCollectedTiles(Tile[] matchedTiles)
		{
			for (int i = 0; i < _currentTilesToCollect.Count; i++)
			{
				var tile = _currentTilesToCollect[i];
				var fittingTiles = matchedTiles.Where(matchedTile => matchedTile.Type.id == tile.TileType.id)
												.Select(matchedTile => matchedTile.icon).ToArray();

				List<Image> images = new List<Image>();
				images.AddRange(fittingTiles);
				tile.AmountToCollect -= fittingTiles.Count();
				_currentTilesToCollect[i] = tile;
				UIManager.Instance.CollectTile(i, tile.AmountToCollect);

				var endPosition = UIManager.Instance.tilesToCollectUI[i].TileIcon.rectTransform.position;
				CollectAnimation(images, endPosition);
			}
			if (_currentTilesToCollect.All(tile => tile.AmountToCollect <= 0))
			{
				_checkScoreRoutine = StartCoroutine(CheckScore());
			}
		}
		private async void CollectAnimation(List<Image> tiles, Vector3 endPosition)
		{
			var moveSequence = DOTween.Sequence();
			List<RectTransform> icons = new List<RectTransform>();
			
			foreach (var fitTile in tiles)
			{
				//var icon = fitTile.rectTransform;
				var icon = Instantiate(fitTile.rectTransform, fitTile.rectTransform);
				icons.Add(icon);
				var startPos = fitTile.rectTransform.position;
				moveSequence.Join(icon.DOMove(endPosition, tweenDuration));	
				//moveBackSequence.Join(icon.DOMove(startPos, 0.5f));	
			}
			await moveSequence.Play().AsyncWaitForCompletion();
			foreach (var icon in icons)
			{
				Destroy(icon.gameObject);
			}
			icons.Clear();
			
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
		private Tile GetTile(Vector2Int coordinates) => GetTile(coordinates.x, coordinates.y);
		
		private Tile[] GetTiles(IList<TileData> tileData)
		{
			var length = tileData.Count;

			var tiles = new Tile[length];

			for (var i = 0; i < length; i++) tiles[i] = GetTile(tileData[i].X, tileData[i].Y);

			return tiles;
		}
		
		private async void Select(Tile tile)
		{
			if (!GlobalData.IsTile(tile.Data)) return;
            
			var goToOrigin = DOTween.Sequence();
            var highlightSequence = DOTween.Sequence();
            
			highlightSequence.Join(tile.icon.transform.DOScale(new Vector3(1.3f, 1.3f, 1f), tweenDuration));

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
				await TryMatchAsync(tile.Execute(Matrix));
				_selection.Clear();
			}

			if (_selection.Count < 2) return;

			await SwapAsync(_selection[0], _selection[1], false);
			goToOrigin.Play().Complete();

			if (!await TryMatchAsync(tile.Execute(Matrix))) await SwapAsync(_selection[0], _selection[1], false);

			var matrix = Matrix;

			
			while (TileDataMatrixUtility.FindBestMove(matrix) == null || TileDataMatrixUtility.FindBestMatch(matrix) != null) 
			{			
				if (!_isShuffling) Shuffle();

				matrix = Matrix;
			}

			goToOrigin.Kill();
			highlightSequence.Kill();
			_selection.Clear();
		}

		private async Task SwapAsync(Tile tile1, Tile tile2, bool justShow)
		{
			_isSwapping = true;

			var icon1 = tile1.icon;
			var icon2 = tile2.icon;

			var icon1Transform = icon1.transform;
			var icon2Transform = icon2.transform;

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
			var excludeList = new List<Tile>();
			var didMatch = false;

			_isMatching = true;

			var match = explosion != null ? explosion : TileDataMatrixUtility.FindBestMatch(Matrix);

			//List<Tile> matchedTiles;

			while (match != null)
			{
				var matchedTiles = new List<Tile>();

				didMatch = true;

				var tiles = excludeList.Count > 0 ? GetTiles(match.Tiles).Except(excludeList).ToArray() : GetTiles(match.Tiles);
				excludeList.Clear();

				var deflateSequence = DOTween.Sequence();

				foreach (var tile in tiles)
				{
					deflateSequence.Join(tile.icon.transform.DOScale(Vector3.zero, tweenDuration).SetEase(Ease.InBack));
					matchedTiles.Add(tile);
				}

				//if (explosion == null) 
				UIManager.Instance.soundManager.PlaySound(GlobalData.AudioClipType.OnMatch);
				
				//audioSource.PlayOneShot(matchSound);
				
				OnMatch?.Invoke(matchedTiles.ToArray());

				await deflateSequence.Play()
				                     .AsyncWaitForCompletion();
						
				var inflateSequence = DOTween.Sequence();

						
				if (matchedTiles.Skip(1).Any(tile => GlobalData.IsSpecialTile(tile.Data)))
				{
					//excludeList.Clear();
					Debug.Log("Special Tile"); 
					var explodeTiles = matchedTiles.Skip(1).Where(tile => GlobalData.IsSpecialTile(tile.Data)).ToList();
					
					
					foreach (var tile in explodeTiles)
					{
						_explosionMatches.Add(tile.Execute(Matrix));
					}
					excludeList.AddRange(matchedTiles);
					//explodeMatch = explodeTiles.First().Execute(Matrix);
				}
		
				
				for (int i = 0; i < tiles.Length; i++)
				{
					var tile = tiles[i];
					Item item;
					if (match.Tiles.Any(tile => GlobalData.IsSpecialTile(tile)))
					{
						item = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];	
					}
					else
					{
						item = i == 3 ? SetSpecialTileType(match.MatchType)
								: _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
						//item = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
					}
                    tile.Type = item;

					inflateSequence.Join(tile.icon.transform.DOScale(Vector3.one, tweenDuration).SetEase(Ease.OutBack));
				}
				
				await inflateSequence.Play()
				                     .AsyncWaitForCompletion();


				match = TileDataMatrixUtility.FindBestMatch(Matrix);
				
				if (_explosionMatches.Count > 0)
				{
					match = _explosionMatches.Last();
					_explosionMatches.RemoveAt(_explosionMatches.Count - 1);					
				}
				
			}

			_isMatching = false;
			UIManager.Instance.tipButton.Button.interactable = true;
			return didMatch;
		}
		private Item SetSpecialTileType(MatchType matchType)
		{
			Item tileItem;
			
			switch (matchType)
			{
				case MatchType.Horizontal :
					tileItem = _specialTilesTypes.First(tileItem => tileItem.tileType == TileType.HorizontalExplosion);
					break;
				case MatchType.Vertical :
					tileItem = _specialTilesTypes.First(tileItem => tileItem.tileType == TileType.VerticalExplosion);
					break;
				case MatchType.BothDirections :
					tileItem = _specialTilesTypes.First(tileItem => tileItem.tileType == TileType.SquareExplosion);
					break;
				case MatchType.Square :
					tileItem = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
					break;
                default : tileItem = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
					break;
			}
			return tileItem;
		}

		private void Shuffle()
		{
			_isShuffling = true;

			foreach (var row in _rows)
				foreach (var tile in row.tiles)
					{
						//await TryMatchAsync(tile.Execute(Matrix));
						tile.Type = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
					}

			_isShuffling = false;
		}
		private void ExplodeBombs()
		{
			foreach (var row in _rows)
				foreach (var tile in row.tiles)
					{
						tile.Execute(Matrix);
						//tile.Type = _currentTilesTypes[Random.Range(0, _currentTilesTypes.Length)];
					}
		}

		private void OnDestroy()
		{
			OnMatch -= (data) => IncreaseScore(data);
			OnLevelComplet -= UIManager.Instance.OnLevelComplete;
			OnGameOver -= UIManager.Instance.OnGameOver;

			_inputControler.Disable();
			_inputControler.Touchscreen.Touch.started -= StartSwipe;
			_inputControler.Touchscreen.Touch.canceled -= SwipeTile;
		}
		private void OnDisable()
		{
			//_touchInputAction.action.Disable();

			OnMatch -= (data) => IncreaseScore(data);
			OnLevelComplet -= UIManager.Instance.OnLevelComplete;
			OnGameOver -= UIManager.Instance.OnGameOver;
			
			_inputControler.Disable();
			_inputControler.Touchscreen.Touch.started -= StartSwipe;
			_inputControler.Touchscreen.Touch.canceled -= SwipeTile;
		}

	}
}
