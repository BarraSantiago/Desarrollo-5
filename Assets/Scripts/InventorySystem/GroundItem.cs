using UnityEngine;

public class GroundItem : MonoBehaviour
{
    public ItemObject item;

    
    private void OnValidate()
    {
#if UNITY_EDITOR
        GetComponentInChildren<SpriteRenderer>().sprite = item.uiDisplay;
#endif
    }

}
