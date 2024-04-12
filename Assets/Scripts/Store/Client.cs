using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Store
{
    public class Client : MonoBehaviour
    {
        public DisplayItem desiredItem = null;
        public static Action<int> ItemBought;
        public static Action<int> MoneyAdded; //TODO change name
        
        [SerializeField] private Player player; // TODO remove this
        [SerializeField] private StoreManager storeManager; // TODO remove this
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Transform exit;

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
        
        /// <summary>
        /// Por si hacemos mejoras para modificar la probabilidad que dejen propina
        /// </summary>
        private int _tipChanceModifier = 0;
        private int _tipValue;

        private void Start()
        {
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        public void EnterStore()
        {
            StartCoroutine(WalkToItem());
        }

        

        private IEnumerator WalkToItem()
        {
            //Move
            agent.SetDestination(desiredItem.transform.position);

            //TODO update this
            yield return new WaitUntil(CheckDistance);

            //When client reaches item
            CheckBuyItem();

            //Leave store after buying item
            LeaveStore();
        }

        private bool CheckDistance()
        {
            float minimumDistance = 0.9f;
            float distance = Vector3.Distance(transform.position, desiredItem.transform.position);

            if (distance < minimumDistance)
            {
                return true;
            }

            return false;
        }

        private void CheckBuyItem()
        {
            ListPrice itemList = storeManager.listPrices.prices[desiredItem.ItemId];
            float difference = desiredItem.price - itemList.CurrentPrice;
            float percentageDifference = (difference / itemList.CurrentPrice) * 100f;

            if (desiredItem.price >= itemList.CurrentPrice)
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
            MoneyAdded?.Invoke(desiredItem.price);

            storeManager.listPrices.prices[desiredItem.ItemId].amountSoldLastDay++;
            ItemBought?.Invoke(desiredItem.ItemId);
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
                MoneyAdded.Invoke(_tipValue);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void LeaveStore()
        {
            //move to outside of store
            agent.SetDestination(exit.transform.position);
        }
    }
}