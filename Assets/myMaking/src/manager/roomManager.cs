using ProtoBuf.WellKnownTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class roomManager : MonoBehaviour
{
    public static roomManager instance;

    public GameObject roomUi;
    public Text nameUi;
    public GameObject noRoomUi;

    [SerializeField] int maxRoomObject = 5;
    [SerializeField] int nowPage = 0;// 페이지 네이션 용도.
    [SerializeField] int spacingRoomObject = -12;

    [SerializeField] GameObject roomuiObject;
    [SerializeField] GameObject roomBase;
    [SerializeField] List<GameObject> roomList = new List<GameObject>();

    [SerializeField] gameSessions sessionsData;
    [SerializeField] InputField gameIdInput;

    public inSession nowSessionData;// 방 들어가면 넣어주기!

    private void Awake()
    {
        instance = this;
    }

    public void startRoom(gameSessions sessions , GameObject ui)
    {
        sessionsData = sessions;
        poolMaker();

        ui.SetActive(false);
        roomUi.SetActive(true);

        nameUi.text = GameManager.instance.deviceId; 

        if (sessionsData.games.Count != 0)
        {
            noRoomUi.SetActive(false);
            nowPageGet();
        }
        Debug.Log(sessions);
    }
    public void nowPageGet()
    {
        int max = (nowPage + 1) * maxRoomObject < sessionsData.games.Count ? (nowPage + 1) * maxRoomObject : sessionsData.games.Count;
        int count = 0;
        for (int i = nowPage * maxRoomObject; i < max; i++ )
        {
            roomList[count].SetActive(true);
            roomList[count].GetComponent<rooms>().roomData = sessionsData.games[i];
            roomList[count].GetComponent<rooms>().settingRoom();
            count++;
        }
    }
    void poolMaker()
    {
        for (int i = 0; i < maxRoomObject; i++)
        {
            GameObject roomObject = Instantiate(roomuiObject, roomBase.transform);
            RectTransform roomTr = roomObject.GetComponent<RectTransform>();
            roomTr.localPosition = new Vector2(roomTr.localPosition.x, roomTr.localPosition.y + spacingRoomObject * (i + 1));
            roomList.Add(roomObject);
        }
    }
    // 방 생성 요청
    public void creatRoom()
    {
        CreateGamePayload createGamePayload = new CreateGamePayload();
        createGamePayload.timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        NetworkManager.instance.SendPacket(createGamePayload, (uint)Packets.HandlerIds.CREATE_GAME);
    }
    // 방 생성 승인
    public void creatOK(inSession data)
    {
        nowSessionData = data;
        GameManager.instance.player.player = data.users[0];
        GameManager.instance.player.gameObject.SetActive(true);
        roomUi.SetActive(false);
        Debug.Log(data.gameId);
    }
    public void goRoom()
    {
        NetworkManager.instance.SendJoinGamePacket(gameIdInput.text);
    }
}
