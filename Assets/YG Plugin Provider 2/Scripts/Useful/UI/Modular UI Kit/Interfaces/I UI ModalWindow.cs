using System;

public interface IUIModalWindow
{
    public bool Active { get; }
    
    public Action OnClose { get; set; }
    
    public void Close();
    public void Show();
}