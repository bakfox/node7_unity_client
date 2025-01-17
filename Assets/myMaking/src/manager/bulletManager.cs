using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bulletManager : MonoBehaviour
{

    public static bulletManager instance;

    private Dictionary<int, GameObject> bulletPool = new Dictionary<int, GameObject>();//ÃÑ¾Ë Ç®

    [SerializeField] Transform bulletsTransform;
    [SerializeField] GameObject bullet;
    // Start is called before the first frame update
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        poolMake();
    }
    void poolMake()
    {

        for (int i = 0; i < 100; i++)
        {
            GameObject bulletTemp = Instantiate(bullet, bulletsTransform);

            bulletTemp.GetComponent<bullets>().bulletId = i;
            bulletPool.Add(i, bulletTemp);

        }
    }
    public void getBullet(Transform player,int id)
    {
  
    }
    public void moveBullet(int id, bulletData data)
    {
        bullets bulletTemp = bulletPool[id].GetComponent<bullets>();
        if (bulletTemp.isFirst)
        {
            Player user = GameManager.instance.getUserData(data.userId);
            bulletTemp.isFirst = false;
            bulletTemp.strat(user.player.defaultBullet, user.player.defaultAtck,data.z,user.tr.position,new Vector2(data.x, data.y),data.status);
        }
        else
        {
            bulletTemp.move(new Vector2(data.x, data.y),data.status);
        }
    }
    public void setBullet(int emoticonId, GameObject emoticon)
    {

    }
}
