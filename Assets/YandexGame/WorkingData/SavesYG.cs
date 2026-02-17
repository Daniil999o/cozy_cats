
namespace YG
{
    [System.Serializable]
    public class SavesYG
    {
        // "Технические сохранения" для работы плагина (Не удалять)
        public int idSave;
        public bool isFirstSession = true;
        public string language = "ru";
        public bool promptDone;

        // === YG PROVIDER SAVES ===
        
            public int money = 0;
            public string newPlayerName = "Hello!";
            public bool[] openLevels = new bool[3];
            
            public int score;
            
            public string[] enteredPromoCodes = new string[100];
            
            public string lastLoginTimestamp;
            public string lastRestorationTimestamp;
            
            public bool[] unlockedItems = new bool[100];
            public int[] currentItems = new int[10];
        
        // === YG PROVIDER SAVES ===
        
        // === YOUR SAVES ===
        
            // SAVES
        
        // === YOUR SAVES ===
        

        // Вы можете выполнить какие то действия при загрузке сохранений
        public SavesYG()
        {
            // Допустим, задать значения по умолчанию для отдельных элементов массива

            openLevels[1] = true;
        }
    }
}
