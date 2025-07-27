using System;
using TMPro;
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
        [SerializeField] private GameObject levelUpPanel;
        [SerializeField] private Image levelUpMedalImage;
        [SerializeField] private TMP_Text levelUpText;
        [SerializeField] private int ForceLevel = 0;
        private const string LevelKey = "LevelUp";
        public float PopularityVariation => Random.Range(0.1f, 1);

        public int MaxClients => maxClientsPerPopularity[_popularityLevel];

        public int MinClients => minClientsPerPopularity[_popularityLevel];

        public int DailyClients => (int)math.lerp(MinClients, MaxClients, PopularityVariation);

        private int _popularityXp = 0;
        private static int _popularityLevel = 0;

        public static int Level => _popularityLevel;
        public static Action OnLevelUp;

        public void Initialize()
        {
            _popularityXp = PlayerPrefs.GetInt("PopularityXp", 0);
            _popularityLevel = PlayerPrefs.GetInt("PopularityLevel", 0);
            
            if (ForceLevel > 0)
            {
                _popularityLevel = ForceLevel;
                _popularityXp = 0;
            }
            popularitySlider.maxValue = popularityXpLevels[_popularityLevel];
            popularitySlider.value = _popularityXp;
            popularityMedal.sprite = medals[_popularityLevel];
        }

        public void Deinitialize()
        {
            PlayerPrefs.SetInt("PopularityXp", _popularityXp);
            PlayerPrefs.SetInt("PopularityLevel", _popularityLevel);
            PlayerPrefs.Save();
        }

        public void ItemPaid()
        {
            _popularityXp++;
            popularitySlider.value = _popularityXp;
            if (_popularityXp < popularityXpLevels[_popularityLevel]) return;
            LevelUpPopularity();
        }

        [ContextMenu("Level up")]
        public void LevelUpPopularity()
        {
            if(_popularityLevel >= medals.Length - 1) return;
            _popularityLevel++;
            _popularityXp = 0;
            popularitySlider.value = 0;
            popularityMedal.sprite = medals[_popularityLevel];
            popularitySlider.maxValue = popularityXpLevels[_popularityLevel];
            AudioManager.instance.Play(LevelKey);
            levelUpPanel.SetActive(true);
            levelUpMedalImage.sprite = medals[_popularityLevel];
            string level = _popularityLevel switch
            {
                0 => "Bronze",
                1 => "Silver",
                2 => "Gold",
                3 => "Platinum",
                4 => "Emerald",
                _ => "Silver"
            };
            levelUpText.text = $"You have reached " + level + "!";
            OnLevelUp?.Invoke();
        }
    }
}