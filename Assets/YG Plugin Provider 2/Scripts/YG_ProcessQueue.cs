using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class YG_ProcessQueue : MonoBehaviour
{
    private struct ScoreData
    {
        public readonly string leaderboardName;
        public readonly long value;
        
        public ScoreData(string leaderboardName, long value) {
            this.leaderboardName = leaderboardName;
            this.value = value;
        }
    }
    
    private const float QueueTimer = 3f;
    private static readonly Queue<ScoreData> ScoreQueue = new Queue<ScoreData>();
    
    private static IEnumerator _processScoreQueue;
    
    private static IEnumerator IE_ProcessScoreQueue()
    {
        while (true) {
            if (ScoreQueue.Count > 0)
            {
                ScoreData scoreData = ScoreQueue.Dequeue();
                YG_Provider.NewScore(scoreData.leaderboardName, scoreData.value);
                
                yield return new WaitForSeconds(QueueTimer);
            } else {
                yield return null;
            }
        }
    }
    
    private void Start() {
        if (_processScoreQueue == null) {
            _processScoreQueue = IE_ProcessScoreQueue();
            StartCoroutine(_processScoreQueue);
        }
    }

    public void AddScoreQueue(string leaderboardName, long value) =>
        ScoreQueue.Enqueue(new ScoreData(leaderboardName, value));
}