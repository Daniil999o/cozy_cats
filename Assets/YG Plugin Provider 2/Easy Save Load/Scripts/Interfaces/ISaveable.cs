namespace EasySaveLoad
{
    public interface ISaveable
    {
        string SaveID { get; }
        
        object GetSaveData();
        
        void LoadSaveData(object data);
    }
}