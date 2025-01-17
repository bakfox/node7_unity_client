using UnityEngine;

public class bullets : MonoBehaviour
{
    public int bulletId ;
    public float bulletSpeed;
    public int bulletdmg;
    public Vector2 nextVec;
    public int Statuse = 2;// 상태 0 출발 1 가는중 2 도착
    public bool isFirst = true;

    public void strat(int speed , int dmg, float angle,Vector2 nowVec, Vector2 nextVec, int status)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0,0, angle));
        bulletSpeed = speed;
        bulletdmg = dmg;
        
        transform.position = nowVec;
        Statuse = status;
        move(nextVec, status);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector2.Lerp(transform.position, nextVec, bulletSpeed / 6 * Time.deltaTime);

        if (Statuse == 2)
        {
            moveEnd();
            return;
        }

       
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ( collision.CompareTag("Monster"))
        {
            
        }
        if ( collision.CompareTag("PlayerClone"))
        {

        }
    }
    public void move(Vector2 newVec,int status)
    { 
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
        Statuse = status;
        nextVec = newVec;
    }
    void moveEnd()
    {
        isFirst = true;
        gameObject.SetActive(false);
    }
}
