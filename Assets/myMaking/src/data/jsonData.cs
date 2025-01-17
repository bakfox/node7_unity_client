using ProtoBuf;
using System;
using System.Collections.Generic;
using UnityEngine;

public class jsonData : MonoBehaviour
{

}

// ¹æ °ü·Ã

[System.Serializable]
public class gameSessions
{
    public List<gameSession> games;
}

[System.Serializable]
public class nextPosition
{
    public string id;
    public float x;
    public float y;
    public long timestamp;
}


[System.Serializable]
public class gameSession
{
   public string id;
   public int users;
}

[System.Serializable]
public class inSession
{
    public string gameId;
    public int maxPlayer;
    public List<userData> users;
}
[System.Serializable]
public class ping
{
    public Int64 timestamp;
}
[System.Serializable]
public class userData
{
    public string id;

    public int level;
    public int exp;
    public int expMax ;

    public float x;
    public float y;

    public int defaultBullet;
    public int defaultSpead;
    public int defaultAtck;
    public int defaultHp;

    public int upgradeAtck;
    public int upgradeHp;
}
[System.Serializable]
public class bulletData
{
    public string userId;
    public int bulletId;

    public float x;
    public float y;
    public float z;

    public int status;
}
[System.Serializable]
public class bulletsData
{
    public List<bulletData> data;
}