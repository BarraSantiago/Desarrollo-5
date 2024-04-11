using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Store
{
    public class Client : MonoBehaviour
    {
        [SerializeField] private Player player; // TODO remove this
        [SerializeField] private StoreManager storeManager; // TODO remove this
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Transform exit;
        private DisplayItem _desiredItem = null;

        /// <summary>
        /// rango de 0 a 100 de lo que esta dispuesto a pagar por sobre el precio de lista (ListPrice)
        /// </summary>
        private int _willingnessToPay = 20; // TODO update WOT value

        /// <summary>
        /// afecta al willingnessToPay y da una porbabilidad de que deje propina
        /// </summary>
        private int _happiness;

        /// <summary>
        /// booleano para decir si el cliente ya esta en la tienda
        /// </summary>
        public bool inShop;

        private int _tipValue;

        /// <summary>
        /// Por si hacemos mejoras para modificar la probabilidad que dejen propina
        /// </summary>
        private int _tipChanceModifier = 0;

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        public void EnterStore()
        {
            ChooseItem();

            StartCoroutine(WalkToItem());
        }

        private void ChooseItem()
        {
            if (!StoreManager.AvailableItem()) return;

            //foreach (DisplayItem displayedItem in StoreManager.DisplayedItems)
            //{
            //    if (displayedItem.BeingViewed) continue;
            //    _desiredItem = displayedItem;
            //}

            // TODO make desired item selection random
            DisplayItem[] abaliavleItems = System.Array.FindAll(StoreManager.DisplayedItems, item => !item.BeingViewed);


            int randomItem = Random.Range(0, abaliavleItems.Length);

            _desiredItem = StoreManager.DisplayedItems[randomItem];

            StoreManager.DisplayedItems[randomItem].BeingViewed = true;
        }

        private IEnumerator WalkToItem()
        {
            //Move
            agent.SetDestination(_desiredItem.transform.position);
            //StartCoroutine(Move(transform.position, _desiredItem.transform.position, 3));

            yield return new WaitForSeconds(3);

            //When client reaches item
            CheckBuyItem();

            //Leave store after buying item
            agent.SetDestination(exit.position);
        }


        IEnumerator Move(Vector3 beginPos, Vector3 endPos, float time)
        {
            for (float t = 0; t < 1; t += Time.deltaTime / time)
            {
                transform.position = Vector3.Lerp(beginPos, endPos, t);
                yield return null;
            }
        }

        private void CheckBuyItem()
        {
            ListPrice itemList = storeManager.listPrices.prices[_desiredItem.ItemId];
            float difference = _desiredItem.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            if (_desiredItem.price >= itemList.CurrentPrice)
            {
                // Cliente no compra el item y se va
                if (percentageDifference > _willingnessToPay)
                {
                    _happiness--; //TODO cambiar la cantidad de felicidad que pierde
                    LeaveStore();
                    return;
                }

                //Esta aca para que no deje propina si el precio es mas caro que el precio de lista
                BuyItem();
                return;
            }

            BuyItem();

            if (_happiness > 0)
                LeaveTip(Math.Abs(percentageDifference) + _happiness);
        }

        /// <summary>
        /// Gives the player money and removes the item from being displayed
        /// </summary>
        private void BuyItem()
        {
            Player.Money += _desiredItem.price;
            player.UpdateMoneyText(); // TODO remove this

            storeManager.listPrices.prices[_desiredItem.ItemId].amountSoldLastDay++;
            StoreManager.DisplayedItems[_desiredItem.ItemId].gameObject.SetActive(false);
        }

        /// <summary>
        /// Al comprar el item deja propina
        /// </summary>
        /// <param name="chance"></param>
        private void LeaveTip(float chance)
        {
            int randomNum = Random.Range(0, 100);

            if (chance < randomNum + _tipChanceModifier)
            {
                Player.Money += _tipValue;
                player.UpdateMoneyText(); // TODO remove this
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LeaveStore()
        {
            //move to outside of store
            transform.position = Vector3.zero;

            gameObject.SetActive(false);
        }
    }
}