﻿using System.IO;

namespace MatchThreeEngine
{
	public enum MatchType
	{
		Vertical,
		Horizontal,
		BothDirections
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

			if (horizontal.Length >= 2 && vertical.Length >= 2)
			{
				MatchType =  MatchType.BothDirections;
				Tiles = new TileData[horizontal.Length + vertical.Length + 1];

				Tiles[0] = origin;

				horizontal.CopyTo(Tiles, 1);

				vertical.CopyTo(Tiles, horizontal.Length + 1);
			}
			else if (horizontal.Length >= 2)
			{
				MatchType = MatchType.Horizontal;
				Tiles = new TileData[horizontal.Length + 1];

				Tiles[0] = origin;

				horizontal.CopyTo(Tiles, 1);
			}
			else if (vertical.Length >= 2)
			{
				MatchType = MatchType.Vertical;
				Tiles = new TileData[vertical.Length + 1];

				Tiles[0] = origin;

				vertical.CopyTo(Tiles, 1);
			}
			else Tiles = null;

			Score = Tiles?.Length ?? -1;
		}
	}
}
