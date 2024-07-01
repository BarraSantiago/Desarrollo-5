using System;
using player;
using UnityEngine;

namespace InventorySystem
{
    [Serializable]
    public class ItemBuff : IModifiers
    {
        [SerializeField] private int max; //buff max value roll
        [SerializeField] private int min; //buff min value roll
        [SerializeField] private float duration = 0; //buff duration
        public Attributes stat;
        public int value;
        public int Min
        {
            get => min;
            set => min = value;
        }

        public int Max {
            get => max;
            set => max = value;
        }
        public float Duration {
            get => duration;
            set => duration = value;
        }

        public ItemBuff(int _min, int _max)
        {
            min = _min;
            max = _max;
            GenerateField();
        }

        public void AddValue(ref int v)
        {
            v += value;
        }

        public void GenerateField()
        {
            value = UnityEngine.Random.Range(min, max);
        }

        public void UseBuff()
        {
            PlayerStats.OnBuffReceived?.Invoke(this);
        }
    }
}