public interface IActivable
{
    public bool IsActive { get; }
    public void Activate();

    public void Deactivate();
}
