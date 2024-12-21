using System;
using System.Collections.Generic;
using UnityEngine;

namespace MatchThreeEngine
{
	public sealed class Row : MonoBehaviour
	{
		[SerializeField] private Tile _tilePrefab;
		[NonSerialized] public List<Tile> tiles;

		public void SpawnTiles(int tilesCount)
		{
			tiles = new List<Tile>();
			for (int i = 0; i < tilesCount; i++)
			{
				tiles.Add(Instantiate(_tilePrefab, transform));
			}
		}
	}
}
