using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class emoticons : MonoBehaviour
{
    public emoticonManager emoticonManagerTemp;
    public Player playerTemp;
    public int id;

    private void OnEnable()
    {
        StartCoroutine("emoticonAnim");
    }

    IEnumerator emoticonAnim()
    {
        yield return new WaitForSeconds(1f);
        emoticonManagerTemp.setEmoticon(id, gameObject);
    }
}
