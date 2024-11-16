using UnityEngine;

namespace Clients
{
    public class SellerController : MonoBehaviour
    {
        [SerializeField] private Animator sellerAnimator;
        [SerializeField] private Animator chestAnimator;

        private readonly int _charge = Animator.StringToHash("Charge");
        private readonly int _open = Animator.StringToHash("Open");

        private void Awake()
        {
            Client.OnItemBought += OpenChest;
        }

        private void OnDestroy()
        {
            Client.OnItemBought -= OpenChest;
        }

        private void OpenChest(int i, int i1)
        {
            sellerAnimator.SetTrigger(_charge);
            chestAnimator.SetTrigger(_open);
        }
    }
}