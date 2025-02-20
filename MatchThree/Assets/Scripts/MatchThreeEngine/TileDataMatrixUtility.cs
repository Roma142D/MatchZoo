using System.Collections.Generic;
using System.Linq;

//using System.Diagnostics;
using UnityEngine;

namespace MatchThreeEngine
{
	public static class TileDataMatrixUtility
	{
		public static TileData[,] Swap(int x1, int y1, int x2, int y2, TileData[,] tiles)
		{
			var tile1 = tiles[x1, y1];

			tiles[x1, y1] = tiles[x2, y2];

			tiles[x2, y2] = tile1;
			return tiles;
		}

		public static (TileData[], TileData[]) GetConnections(int originX, int originY, TileData[,] tiles)
		{
			var origin = tiles[originX, originY];

			var width = tiles.GetLength(0);
			var height = tiles.GetLength(1);

			var horizontalConnections = new List<TileData>();
			var verticalConnections = new List<TileData>();

			/*
			var rightTilePos = GetNeighborTileCoordinates(GlobalData.Direction.RIGHT, origin, tiles);
			var bottomTilePos = GetNeighborTileCoordinates(GlobalData.Direction.DOWN, origin, tiles);
			var rightBottomTilePos = GetNeighborTileCoordinates(GlobalData.Direction.DOWN_RIGHT, origin, tiles);
			var rightTile = rightTilePos != origin.GetTilePosition() ? tiles[rightTilePos.x, rightTilePos.y] : new TileData(-1, -1, -1);
			var bottomTile = bottomTilePos != origin.GetTilePosition() ? tiles[bottomTilePos.x, bottomTilePos.y] : new TileData(-1, -1, -1);
			var rightBottomTile = rightBottomTilePos != origin.GetTilePosition() ? tiles[rightBottomTilePos.x, rightBottomTilePos.y] : new TileData(-1, -1, -1);
			if (rightTile.TypeId == origin.TypeId && bottomTile.TypeId == origin.TypeId && rightBottomTile.TypeId == origin.TypeId)
			{
				horizontalConnections.Add(rightTile);
				//horizontalConnections.Add(origin);
				verticalConnections.Add(bottomTile);
				verticalConnections.Add(rightBottomTile);
				Debug.Log("Square match");
				return (horizontalConnections.ToArray(), verticalConnections.ToArray());
			}
			*/

			for (var x = originX - 1; x >= 0; x--)
			{
				var other = tiles[x, originY];

				if (other.TypeId != origin.TypeId) break;

				horizontalConnections.Add(other);
			}

			for (var x = originX + 1; x < width; x++)
			{
				var other = tiles[x, originY];

				if (other.TypeId != origin.TypeId) break;

				horizontalConnections.Add(other);
			}

			for (var y = originY - 1; y >= 0; y--)
			{
				var other = tiles[originX, y];

				if (other.TypeId != origin.TypeId) break;

				verticalConnections.Add(other);
				if (horizontalConnections.Count == 1 && verticalConnections.Count == 1)
				{
					//Debug.Log("Horizontal connections: " + horizontalConnections.Count);
					//Debug.Log("Vertical connections: " + verticalConnections.Count);
					var tile = GetNeighborTileCoordinates(GlobalData.Direction.UP, horizontalConnections[0], tiles) != horizontalConnections[0].GetTilePosition() 
							? tiles[horizontalConnections.First().X, horizontalConnections.First().Y - 1] : new TileData(-1, -1, 0);
					if (tile.TypeId == origin.TypeId) verticalConnections.Add(tile);
					return (horizontalConnections.ToArray(), verticalConnections.ToArray());
				}
			}

			for (var y = originY + 1; y < height; y++)
			{
				var other = tiles[originX, y];

				if (other.TypeId != origin.TypeId) break;

				verticalConnections.Add(other);
				if (horizontalConnections.Count == 1 && verticalConnections.Count == 1)
				{
					//Debug.Log("Horizontal connections: " + horizontalConnections.Count);
					//Debug.Log("Vertical connections: " + verticalConnections.Count);
					var tile = GetNeighborTileCoordinates(GlobalData.Direction.DOWN, horizontalConnections.First(), tiles) != horizontalConnections.First().GetTilePosition() 
							? tiles[horizontalConnections.First().X, horizontalConnections.First().Y + 1] : new TileData(0, 0, 0);
					if (tile.TypeId == origin.TypeId) verticalConnections.Add(tile);
					return (horizontalConnections.ToArray(), verticalConnections.ToArray());
				}
			}
					
			//Debug.Log("Horizontal connections: " + horizontalConnections.Count);
			//Debug.Log("Vertical connections: " + verticalConnections.Count);

			return (horizontalConnections.ToArray(), verticalConnections.ToArray());
		}

		public static Match FindBestMatch(TileData[,] tiles)
		{
			var bestMatch = default(Match);

			for (var y = 0; y < tiles.GetLength(1); y++)
			{
				for (var x = 0; x < tiles.GetLength(0); x++)
				{
					var tile = tiles[x, y];

					var (h, v) = GetConnections(x, y, tiles);

					var match = new Match(tile, h, v);

					if (match.Score < 0 || match.TypeId == 0) continue;

					if (bestMatch == null)
					{
						bestMatch = match;
					}
					else if (match.Score > bestMatch.Score) bestMatch = match;
				}
			}

			return bestMatch;
		}

		public static List<Match> FindAllMatches(TileData[,] tiles)
		{
			var matches = new List<Match>();

			for (var y = 0; y < tiles.GetLength(1); y++)
			{
				for (var x = 0; x < tiles.GetLength(0); x++)
				{
					var tile = tiles[x, y];

					var (h, v) = GetConnections(x, y, tiles);

					var match = new Match(tile, h, v);

					if (match.Score > -1) matches.Add(match);
				}
			}

			return matches;
		}

		private static (int, int) GetDirectionOffset(byte direction) => direction switch
		{
			0 => (-1, 0),
			1 => (0, -1),
			2 => (1, 0),
			3 => (0, 1),

			_ => (0, 0),
		};

		public static Vector2Int GetNeighborTileCoordinates(GlobalData.Direction direction, TileData originTile, TileData[,] matrix)
		{
			var width = matrix.GetLength(0);
			var height = matrix.GetLength(1);
			
			switch (direction)
			{
				case GlobalData.Direction.UP:
					if (originTile.Y > 0)
					{
						return new Vector2Int(originTile.X, originTile.Y - 1);
					}
					break;
				case GlobalData.Direction.DOWN:
					if (originTile.Y < height - 1)
					{
						return new Vector2Int(originTile.X, originTile.Y + 1);
					}
					break;
				case GlobalData.Direction.LEFT:	
					if (originTile.X > 0)
					{
						return new Vector2Int(originTile.X - 1, originTile.Y);
					}
					break;
				case GlobalData.Direction.RIGHT:
					if (originTile.X < width - 1)
					{
						return new Vector2Int(originTile.X + 1, originTile.Y);
					}
					break;
				case GlobalData.Direction.UP_LEFT:
					if (originTile.X > 0 && originTile.Y > 0)
					{
						return new Vector2Int(originTile.X - 1, originTile.Y - 1);
					}
					break;
				case GlobalData.Direction.UP_RIGHT:
					if (originTile.X < width - 1 && originTile.Y > 0)
					{
						return new Vector2Int(originTile.X + 1, originTile.Y - 1);
					}
					break;
				case GlobalData.Direction.DOWN_LEFT:
					if (originTile.X > 0 && originTile.Y < height - 1)
					{
						return new Vector2Int(originTile.X - 1, originTile.Y + 1);
					}
					break;
				case GlobalData.Direction.DOWN_RIGHT:
					if (originTile.X < width - 1 && originTile.Y < height - 1)
					{
						return new Vector2Int(originTile.X + 1, originTile.Y + 1);
					}
					break;
			}
			return new Vector2Int(originTile.X, originTile.Y);
		}
		public static Move FindMove(TileData[,] tiles)
		{
			var tilesCopy = (TileData[,])tiles.Clone();

			var width = tilesCopy.GetLength(0);
			var height = tilesCopy.GetLength(1);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					for (byte d = 0; d <= 3; d++)
					{
						var (offsetX, offsetY) = GetDirectionOffset(d);

						var x2 = x + offsetX;
						var y2 = y + offsetY;

						if (x2 < 0 || x2 > width - 1 || y2 < 0 || y2 > height - 1) continue;

						Swap(x, y, x2, y2, tilesCopy);

						if (FindBestMatch(tilesCopy) != null) return new Move(x, y, x2, y2);

						Swap(x2, y2, x, y, tilesCopy);
					}
				}
			}

			return null;
		}

		public static Move FindBestMove(TileData[,] tiles)
		{
			var tilesCopy = (TileData[,])tiles.Clone();

			var width = tilesCopy.GetLength(0);
			var height = tilesCopy.GetLength(1);

			var bestScore = int.MinValue;

			var bestMove = default(Move);

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width; x++)
				{
					for (byte d = 0; d <= 3; d++)
					{
						var (offsetX, offsetY) = GetDirectionOffset(d);

						var x2 = x + offsetX;
						var y2 = y + offsetY;

						if (x2 < 0 || x2 > width - 1 || y2 < 0 || y2 > height - 1) continue;

						tilesCopy = Swap(x, y, x2, y2, tilesCopy);
						var tile1 = tilesCopy[x, y];
						var tile2 = tilesCopy[x2, y2];

						var match = FindBestMatch(tilesCopy);

						if ((match != null && match.Score > bestScore 
							&& GlobalData.IsTile(tile1) && GlobalData.IsTile(tile2)) || (GlobalData.IsSpecialTile(tile1) && GlobalData.IsTile(tile2) && 1 > bestScore))
						{
							bestMove = new Move(x, y, x2, y2);
							
							bestScore = GlobalData.IsSpecialTile(tile1) ? 1 : match.Score;
						}
						

						tilesCopy = Swap(x, y, x2, y2, tilesCopy);
					}
				}
			}

			return bestMove;
		}
	}
}
