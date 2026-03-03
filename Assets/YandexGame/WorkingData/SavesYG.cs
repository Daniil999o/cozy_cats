using System.Collections.Generic;

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

            public int gems = 0;
            public int megas = 0;

            public float dayValue = 1f;
            
            public string[] savedMaterials = new string[300];
            public string[] savedSkins = new string[300];

            public string[] savedObjectsId = new string[100];
            
            public string[] currentObjectsMaterials = new string[100];
            public string[] currentObjectsSkins = new string[100];
        
        // === YOUR SAVES ===
        

        // Вы можете выполнить какие то действия при загрузке сохранений
        public SavesYG()
        {
            // Допустим, задать значения по умолчанию для отдельных элементов массива

            openLevels[1] = true;
        }
    }
}
