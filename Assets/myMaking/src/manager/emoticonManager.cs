using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class emoticonManager : MonoBehaviour
{
    public static emoticonManager Instance;

    [SerializeField] private List<emojiData> emojiList = new List<emojiData>();//설정할 리스트 
    [SerializeField] private Transform poolObject;

    private Dictionary<int , List<GameObject>> emoticonPool = new Dictionary<int, List<GameObject>>();//이모티콘 풀

    public GameObject playerUiObject;

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        poolMake();
    }
    // 풀 생성용도
    void poolMake()
    {
        foreach (emojiData emoji in emojiList)
        {
            emoticonPool.Add(emoji.id, new List<GameObject>());
        }
        for (int i =0; i< GameManager.instance.roomMax ; i++)
        {
            foreach (emojiData emoji in emojiList)
            {
                GameObject emoticon = Instantiate(emoji.emojiObject, poolObject);
                emoticon.GetComponent<emoticons>().emoticonManagerTemp = this;
                emoticon.GetComponent<emoticons>().id = emoji.id;
                emoticonPool[emoji.id].Add(emoticon);
            }
        }
    }
    public void getEmoticon(Transform player , int emoticonId)
    {
        playerUiObject.SetActive(false);
        emoticonPool[emoticonId][0].transform.SetParent(player);
        emoticonPool[emoticonId][0].transform.localPosition = Vector3.zero;
        emoticonPool[emoticonId][0].SetActive(true);
        emoticonPool[emoticonId].RemoveAt(0);
    }
    public void setEmoticon(int emoticonId,GameObject emoticon)
    {
        emoticonPool[emoticonId].Add(emoticon);
        emoticon.SetActive(false);
        emoticon.transform.SetParent(poolObject);
        GameManager.instance.player.isUseEmoticon = false;
    }
    private void Update()
    {
        if (!GameManager.instance.isLive)
        {
            return;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (playerUiObject.activeSelf)
            {
                playerUiObject.SetActive(false);
            }
            else
            {
                playerUiObject.SetActive(true);
            }
        }
    }
}
