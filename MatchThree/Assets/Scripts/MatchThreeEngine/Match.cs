using UnityEngine;
using System.IO;
using System.Linq;

namespace MatchThreeEngine
{
	public enum MatchType
	{
		Vertical,
		Horizontal,
		BothDirections,
		Square
	}
	public sealed class Match
	{
		public readonly int TypeId;

		public readonly int Score;
		public readonly MatchType MatchType;

		public readonly TileData[] Tiles;

		
		public Match(TileData origin, TileData[] horizontal, TileData[] vertical)
		{
			TypeId = origin.TypeId;

			if (TypeId == 0) return;
			
			if (horizontal.Length >= 2 && vertical.Length >= 2)
			{
				Tiles = new TileData[horizontal.Length + vertical.Length + 1];

				Tiles[0] = origin;

				horizontal.CopyTo(Tiles, 1);
				vertical.CopyTo(Tiles, horizontal.Length + 1);

				MatchType = MatchType.BothDirections;
			}
			else if (horizontal.Length >= 2 && vertical.Length == 0)
			{
				MatchType = MatchType.Horizontal;
				Tiles = new TileData[horizontal.Length + 1];

				Tiles[0] = origin;

				horizontal.CopyTo(Tiles, 1);
			}
			else if (vertical.Length >= 2 && horizontal.Length == 0)
			{
				MatchType = MatchType.Vertical;
				Tiles = new TileData[vertical.Length + 1];

				Tiles[0] = origin;

				vertical.CopyTo(Tiles, 1);
			}
			else if (CanBeSquareMatch(horizontal, vertical, origin))
			{
				MatchType = MatchType.Square;
				Tiles = new TileData[horizontal.Length + vertical.Length + 1];

				Tiles[0] = origin;

				horizontal.CopyTo(Tiles, 1);
				vertical.CopyTo(Tiles, horizontal.Length + 1);
				//UnityEngine.Debug.Log($"match id: {TypeId} First: {Tiles[0].X}, {Tiles[0].Y} Second: {Tiles[1].X}, {Tiles[1].Y} Third: {Tiles[2].X}, {Tiles[2].Y} Fourth: {Tiles[3].X}, {Tiles[3].Y}");
			}
			else Tiles = null;

			Score = Tiles?.Length ?? -1;
		}

		private bool CanBeSquareMatch(TileData[] horizontal, TileData[] vertical, TileData origin)
		{
			if (horizontal.Length + vertical.Length == 3 && !vertical.Contains(horizontal.First()) && !vertical.First().Equals(vertical.Last())
				&& !horizontal.Contains(origin) && !vertical.Any(tile => tile.Equals(origin)))
			{
				return true;
			}
			return false;
		}
	}
}
