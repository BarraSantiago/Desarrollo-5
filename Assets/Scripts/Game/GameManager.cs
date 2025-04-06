using InventorySystem;
using player;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameObject groundItemPrefab;
        [SerializeField] private ItemDatabaseObject itemDatabase;
        [SerializeField] private Button goToStore;
        [SerializeField] private Canvas mainCanvas;

        private Inventory _inventory;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            UIManager.MainCanvas = mainCanvas;
            LoadInventory();
            goToStore?.onClick.AddListener(ChangeScene);
        }

        private void ChangeScene()
        {
            SceneManager.LoadScene("Store - Santi");
        }


        private void LoadInventory()
        {
            //TODO load inventory here
        }
    }
}