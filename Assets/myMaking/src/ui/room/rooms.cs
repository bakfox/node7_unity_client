using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class rooms : MonoBehaviour
{
    public gameSession roomData;

    public Text roomName;

    public void goRoom()
    {
        NetworkManager.instance.SendJoinGamePacket(roomData.id);
    }
    public void settingRoom()
    {
        gameObject.SetActive(true);
        roomName.text = (roomData.id + ":id " + roomData.users + "/" + GameManager.instance.roomMax);
    }
}
