
namespace Interactable
{
    public interface IInteractable
    {
        public bool State { get; }
        public bool Interact();
        public void Close();
    }
}