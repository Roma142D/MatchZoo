using System.Numerics;
using UnityEngine;

namespace MatchThreeEngine
{
	public readonly struct TileData
	{
		public readonly int X;
		public readonly int Y;

		public readonly int TypeId;

		public TileData(int x, int y, int typeId)
		{
			X = x;
			Y = y;

			TypeId = typeId;
		}
		public Vector2Int GetTilePosition()
		{
			return new Vector2Int(X, Y);
		}
	}
}
