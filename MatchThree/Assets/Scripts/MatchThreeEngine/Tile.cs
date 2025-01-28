using System;
using System.Collections.Generic;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace MatchThreeEngine
{
	public sealed class Tile : MonoBehaviour
	{
		[SerializeField] private Particles _particles;
		public int x;
		public int y;

		public Image icon;

		public Button button;

		[SerializeField] private Item _type;

		public Item Type
		{
			get => _type;

			set
			{
				if (button.IsInteractable())
				{
					if (_type == value) return;

					_type = value;

					icon.sprite = _type.Sprite;
				}
			}
		}

		public TileData Data => new TileData(x, y, _type.id);

		private void Start()
		{
			button.onClick.AddListener(UIManager.Instance.soundManager.OnTileClickSoundPlay);
			if(_type.tileType == TileType.Standart) icon.sprite = _type.Sprite;
		
		}
		public Match Execute(TileData[,] tiles)
		{
			var width = tiles.GetLength(0);
			var height = tiles.GetLength(1);
			var verticalTilesToMatch = new List<TileData>();
			var horizontalTilesToMatch = new List<TileData>();
			var match = default(Match);
			switch (_type.tileType)
			{
				case TileType.Standart:
					break;
				case TileType.VerticalExplosion:
					//Instantiate(_particles.VerticalExplosion, transform).Play();
					_particles.VerticalExplosion.Play();
					UIManager.Instance.soundManager.PlaySound(GlobalData.AudioClipType.Explosion);
					for (int i = y - 1; i >= 0; i--)
					{
						var other = tiles[x, i];
						verticalTilesToMatch.Add(other);
					}
					for (int i = y + 1; i < height; i++)
					{
						var other = tiles[x, i];
						verticalTilesToMatch.Add(other);
					}
					match = new Match(Data, horizontalTilesToMatch.ToArray() ,verticalTilesToMatch.ToArray());
					break;
				case TileType.HorizontalExplosion:
					//Instantiate(_particles.HorizontalExplosion, transform).Play();
					_particles.HorizontalExplosion.Play();
					UIManager.Instance.soundManager.PlaySound(GlobalData.AudioClipType.Explosion);
					for (int i = x - 1; i >= 0; i--)
					{
						var other = tiles[i, y];
						horizontalTilesToMatch.Add(other);
					}
					for (int i = x + 1; i < width; i++)
					{
						var other = tiles[i, y];
						horizontalTilesToMatch.Add(other);
					}
					match = new Match(Data, horizontalTilesToMatch.ToArray() ,verticalTilesToMatch.ToArray());
					break;
				case TileType.SquareExplosion:
					_particles.SquareExplosion.Play();
					UIManager.Instance.soundManager.PlaySound(GlobalData.AudioClipType.Explosion);
					for (int i = x - 1; i <= x + 1; i++)
					{
						for (int j = y - 1; j <= y + 1; j++)
						{
							if (i >= 0 && i < width && j >= 0 && j < height)
							{
								var other = tiles[i, j];
								horizontalTilesToMatch.Add(other);
							}
						}
					}
					match = new Match(Data, horizontalTilesToMatch.ToArray() ,verticalTilesToMatch.ToArray());
					break;
			}
			return match;
		}
		[Serializable]
		public struct Particles
		{
			public ParticleSystem VerticalExplosion;
			public ParticleSystem HorizontalExplosion;	
			public ParticleSystem SquareExplosion;
		}
	}
}
