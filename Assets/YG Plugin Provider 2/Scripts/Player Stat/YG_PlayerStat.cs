using YG;

public static class YG_PlayerStat
{
    
    /*
     *      To use it, add the "public int score;" field to SavesYG
     */
    
    public static int Score {
        get => YandexGame.savesData.score;
        set {
            if (value > YandexGame.savesData.score)
                YG_Provider.NewScore("score", value);
            
            YandexGame.savesData.score = value;
            YandexGame.SaveProgress();
        }
    }
}