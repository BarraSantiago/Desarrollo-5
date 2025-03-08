using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Random = UnityEngine.Random;

namespace Store
{
    public class PopularityManager : MonoBehaviour, IInitializable
    {
        [SerializeField] private Slider popularitySlider;
        [SerializeField] private int[] popularityXpLevels;
        [SerializeField] private int[] maxClientsPerPopularity;
        [SerializeField] private int[] minClientsPerPopularity;
        [SerializeField] private Image popularityMedal;
        [SerializeField] private Sprite[] medals;
        private const string LevelKey = "LevelUp";
        public float PopularityVariation => Random.Range(0.1f, 1);

        public int MaxClients => maxClientsPerPopularity[_popularitryLevel];

        public int MinClients => minClientsPerPopularity[_popularitryLevel];

        public int DailyClients => (int)math.lerp(MinClients, MaxClients, PopularityVariation);

        private int _popularitryXp = 0;
        private int _popularitryLevel = 0;

        public void Initialize()
        {
            _popularitryXp = PlayerPrefs.GetInt("PopularityXp", 0);
            _popularitryLevel = PlayerPrefs.GetInt("PopularityLevel", 0);

            popularitySlider.maxValue = popularityXpLevels[_popularitryLevel];
            popularitySlider.value = _popularitryXp;
            popularityMedal.sprite = medals[_popularitryLevel];
        }

        public void Deinitialize()
        {
            PlayerPrefs.SetInt("PopularityXp", _popularitryXp);
            PlayerPrefs.SetInt("PopularityLevel", _popularitryLevel);
            PlayerPrefs.Save();
        }

        public void ItemPaid()
        {
            _popularitryXp++;
            popularitySlider.value = _popularitryXp;
            if (_popularitryXp < popularityXpLevels[_popularitryLevel]) return;
            LevelUpPopularity();
        }

        private void LevelUpPopularity()
        {
            _popularitryLevel++;
            _popularitryXp = 0;
            popularitySlider.value = 0;
            popularityMedal.sprite = medals[_popularitryLevel];
            popularitySlider.maxValue = popularityXpLevels[_popularitryLevel];
            AudioManager.instance.Play(LevelKey);
        }
    }
}