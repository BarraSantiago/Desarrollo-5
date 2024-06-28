namespace InventorySystem
{
    [System.Serializable]
    public class Attribute
    {
        [System.NonSerialized]
        public PlayerInvStats parent;
        public Attributes type;
        public ModifiableInt value;
        public void SetParent(PlayerInvStats _parent)
        {
            parent = _parent;
            value = new ModifiableInt(AttributeModified);
        }
        public void AttributeModified()
        {
            parent.AttributeModified(this);
        }
    }
}