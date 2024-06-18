using InventorySystem;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private player.Player _player;
        [SerializeField] private GameObject groundItemPrefab;
        [SerializeField] private ItemDatabaseObject itemDatabase;
        [SerializeField] private Button goToStore;

        private Inventory _inventory;

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            LoadInventory();
            goToStore?.onClick.AddListener(ChangeScene);
        }

        private void ChangeScene()
        {
            SceneManager.LoadScene("Store - Pablo");
        }

        public void SpawnItem()
        {
            int randZ = Random.Range(-7, 0);
            int randX = Random.Range(-5, 1);
            Vector3 position = new Vector3(randX, 0, randZ);
            GameObject gameObject = Instantiate(groundItemPrefab);
            gameObject.transform.position = position;
            gameObject.transform.rotation = Quaternion.Euler(90, 0, 0);
            int rand = Random.Range(0, itemDatabase.ItemObjects.Length);
            gameObject.GetComponent<GroundItem>().item = itemDatabase.ItemObjects[rand];
            gameObject.GetComponentInChildren<SpriteRenderer>().sprite = itemDatabase.ItemObjects[rand].uiDisplay;
        }


        private void LoadInventory()
        {
            //TODO load inventory here
        }
    }
}