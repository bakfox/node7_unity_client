using System.Collections;
using System.Collections.Generic;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
   

    [Header("# Game Control")]
    public bool isLive;
    public bool isGameStart;
    public float gameTime;
    public int targetFrameRate;
    public string version = "1.0.0";
    public float latency = 2;//지연시간 체크용으로 변경
    public uint sequence = 0;
    public int roomMax = 2;

   [Header("# Player Info")]
    public uint playerId;
    public string deviceId;
    public uint playerHp;
    public GameObject playerClone;

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public GameObject hud;
    private Dictionary<string, GameObject> players = new Dictionary<string, GameObject>();
    private Dictionary<string, GameObject> monsters = new Dictionary<string, GameObject>();


    void Awake() {
        instance = this;
        Application.targetFrameRate = targetFrameRate;
        playerId = (uint)Random.Range(0, 4);
    }

    public void GameStart() {
        hud.SetActive(true);
        isLive = true;

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver() {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine() {
        isLive = false;
        yield return new WaitForSeconds(0.5f);

        AudioManager.instance.PlayBgm(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameRetry() {
        SceneManager.LoadScene(0);
    }

    public void GameQuit() {
        Application.Quit();
    }

    void Update()
    {
        if (!isGameStart || !isLive) {
            return;
        }
        gameTime += Time.deltaTime;
    }
    //움직임 체크 
    public void playerMove(LocationUpdate data)
    {
        if (data.id == deviceId)
        {
            player.moving(new Vector2(data.x, data.y));
        }
        else
        {
            if (players.TryGetValue(data.id, out GameObject value))
            {
                value.GetComponent<Player>().moving(new Vector2(data.x, data.y));
            }
          

        }
    }
    public void bulletMove(bulletsData bullets)
    {
        foreach (bulletData dataTemp in bullets.data)
        {
            bulletManager.instance.moveBullet(dataTemp.bulletId, dataTemp);
        }
    }
    public Player getUserData(string userID)
    {
        if (userID == deviceId)
        {
            return player;
        }
        else
        {
            return players[userID].GetComponent<Player>();
        }
    }

    //이모티콘 체크 
    public void playerEmoticon(EmoticonUpdate data)
    {
        if (data.userId == deviceId)
        {
            return;
        }
        else
        {
            if (players.TryGetValue(data.userId, out GameObject value))
            {
                emoticonManager.Instance.getEmoticon(value.transform,data.emoticonId);     
            }
        }
    }
    public void playerSpawn(inSession data)
    {
        roomManager.instance.nowSessionData = data;

        foreach (var user in data.users)
        {
            if (user.id == deviceId)
            {
                player.player = user;
            }
            else
            {
                if (!players.ContainsKey(user.id))
                {
                    GameObject clone = Instantiate(playerClone);
                    clone.GetComponent<Player>().player = user;
                    clone.transform.position = new Vector2(user.x, user.y);
                    player.gameObject.SetActive(true);
                    roomManager.instance.roomUi.SetActive(false);
                    players.Add(user.id,clone);
                }
            }
        }
    }
}
