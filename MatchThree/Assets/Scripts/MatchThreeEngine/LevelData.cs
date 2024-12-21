using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace MatchThreeEngine
{
    public enum LevelType
    {
        BeatTime,
        CollectTiles
    }
    [CreateAssetMenu(fileName = "LevelData", menuName = "MatchThree/LevelData")]
    public class LevelData : ScriptableObject
    {
        public LevelType levelType;
        [SerializeField] private Row _rowPrefab;
        [SerializeField] private Vector2Int _boardSize;
        [SerializeField] private TimeToComplete timeToComplet;
        [SerializeField] private BoolGrid _tilesToDisable;
        [Space]
        public List<Item> tilesTypes;
        [Tooltip("Set if Level Type: BeatTime")]
        public float scoreToBeet; 
        [Tooltip("Set if Level Type: CollectTiles")]
        public List<TilesToCollect> tilesToCollect;
        public float TimerStartingTime => timeToComplet.TimerStartingTime;
        public float TimeOnTwoStars => timeToComplet.TwoStars;
        //public int OnStarsComleted;
        

        public List<Row> GenerateBoard(Transform parent)
        {
            var rowsList = new List<Row>();
            for (int y = 0; y < _boardSize.y; y++)
            {
                var newRow = Instantiate(_rowPrefab, parent);
                newRow.SpawnTiles(_boardSize.x);
                rowsList.Add(newRow);
                for (int x = 0; x < newRow.tiles.Count; x++)
                {
                    var tile = newRow.tiles[x];
                    tile.x = x;
                    tile.y = y;
                    if (_tilesToDisable.GetValue(y, x))
                    {
                        tile.icon.gameObject.SetActive(false);
                        tile.button.interactable = false;
                    }
                }
            }
            return rowsList;
        }
                
        [Serializable]
        public struct TimeToComplete
        {
            [Tooltip("Set if Level Type: BeatTime")]
            public float TimerStartingTime;
            public float TwoStars;
            public float ThreeStars;
        }
        [Serializable]
        public struct TilesToCollect
        {
            public Item TileType;
            public int AmountToCollect;
        }
    }
}
