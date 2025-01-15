using UnityEngine;
namespace MatchThreeEngine
{
    public enum TileType
    {
        Standart,
        VerticalExplosion,
        HorizontalExplosion,
        SquareExplosion
    }
    [CreateAssetMenu(fileName = "Item", menuName = "MatchThree/Item")]
    public sealed class Item : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private float _value;
        public TileType tileType;
        public int id;
        public Sprite Sprite => _sprite;
        public float Value => _value;
    }
}
