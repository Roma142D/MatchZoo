using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MatchThreeEngine
{
	public sealed class Tile : MonoBehaviour
	{
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
				case TileType.VerticalExplosive:
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
				case TileType.HorizontalExplosive:
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
			}
			return match;
		}
	}
}
