
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;


public class Player : MonoBehaviour
{
    // 방향
    public Vector2 inputVec;
    // 다음 위치
    public Vector2 nextVec;

    public userData player;

    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;
    TextMeshPro myText;
    public Transform tr;

    //이모티콘 연속 제한 걸기
    public bool isUseEmoticon = false;
    public GameObject emoticonBtn;
    public Transform emoticonTr;

    public GameObject waponObject;

    Vector2 mouseVec;
    float nextZ = 0;

    [SerializeField] bool isNotPlayer = false;

    private Vector2 mousePosition
    {
        get
        {
            return mouseVec;
        }
        set
        {
            mouseVec = new Vector2(
                Mathf.Clamp(value.x, -1f, 1f), 
                Mathf.Clamp(value.y, -1f, 1f)  
            );
        }
    }

    void Awake()
    {
        tr = transform;
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        myText = GetComponentInChildren<TextMeshPro>();
    }

    void OnEnable() {

        if (player.id.Length > 5) {
            myText.text = player.id[..5];
        } else {
            myText.text = player.id;
        }
        myText.GetComponent<MeshRenderer>().sortingOrder = 6;
     
        anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
        inputVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.isLive) {
            return;
        }

        Vector2 nowInputVec = new Vector2(
           Input.GetAxisRaw("Horizontal"),
           Input.GetAxisRaw("Vertical")
        );

        transform.position = Vector2.Lerp(transform.position, nextVec,  player.defaultSpead * Time.deltaTime);

        // 회전 처리
        if (!isNotPlayer)
        {
            waponObject.transform.rotation = Quaternion.Euler(0, 0, nextZ);
            if (nowInputVec != inputVec)
            {
                NetworkManager.instance.SendLocationUpdatePacket(nowInputVec.x, nowInputVec.y);
                inputVec = nowInputVec; // 이전 값을 갱신
            }
        }
        
    }

    // Update가 끝난이후 적용
    void LateUpdate() {
        if (!GameManager.instance.isLive) {
            return;
        }

        anim.SetFloat("Speed", inputVec.magnitude);

        if (!isNotPlayer)
        {
            if (inputVec.x != 0)
            {
                spriter.flipX = inputVec.x < 0;
            }

            // 이모티콘 이벤트 처리
            if (emoticonManager.Instance.playerUiObject.activeSelf)
            {
                emoticonBtn.GetComponent<RectTransform>().localPosition = mousePosition;
                mousePosition = new Vector2((Input.mousePosition.x - 960) / 120, (Input.mousePosition.y - 712) / 120);
                if (Input.GetMouseButtonDown(0) && isUseEmoticon == false)
                {
                    if (mousePosition.x == 1 && mousePosition.y == Mathf.Clamp(mousePosition.y, -0.2f, 0.2f))
                    {
                        NetworkManager.instance.SendEmoticonUpdatePacket(1);
                        emoticonManager.Instance.getEmoticon(emoticonTr, 1);
                        isUseEmoticon = true;
                        return;
                    }
                    if (mousePosition.x == -1 && mousePosition.y == Mathf.Clamp(mousePosition.y, -0.2f, 0.2f))
                    {
                        NetworkManager.instance.SendEmoticonUpdatePacket(3);
                        emoticonManager.Instance.getEmoticon(emoticonTr, 3);
                        isUseEmoticon = true;
                        return;
                    }
                    if (mousePosition.x == Mathf.Clamp(mousePosition.x, -0.2f, 0.2f) && mousePosition.y == 1)
                    {
                        NetworkManager.instance.SendEmoticonUpdatePacket(0);
                        emoticonManager.Instance.getEmoticon(emoticonTr, 0);
                        isUseEmoticon = true;
                        return;
                    }
                    if (mousePosition.x == Mathf.Clamp(mousePosition.x, -0.2f, 0.2f) && mousePosition.y == -1)
                    {
                        NetworkManager.instance.SendEmoticonUpdatePacket(2);
                        emoticonManager.Instance.getEmoticon(emoticonTr, 2);
                        isUseEmoticon = true;
                        return;
                    }
                }
            }
            else//이모티콘 창 없을때
            {
                mousePosition = new Vector2((Input.mousePosition.x - 960) / 120, (Input.mousePosition.y - 540) / 120);
                Vector2 direction = mousePosition - new Vector2(0, 0);
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                waponRotation(angle);
                if (Input.GetMouseButtonDown(0))
                {
                    nextZ = angle;
                    NetworkManager.instance.SendShotUpdatePacket(angle);
                }
            }
        }
        
    }

    
    void OnCollisionStay2D(Collision2D collision) {
        if (!GameManager.instance.isLive) {
            return;
        }
    }
    public void waponRotation(float z)
    {
        nextZ = z;
    }
    public void moving(Vector2 vec)
    {
        nextVec = vec;
    }
}
