public interface IUIModalWindow
{
    public bool Active { get; }
    
    public void Close();
    public void Show();
}