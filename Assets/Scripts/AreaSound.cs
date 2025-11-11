using UnityEngine;

[DisallowMultipleComponent]
public class AreaSound : MonoBehaviour
{
    [SerializeField] private int areaSoundIndex;
    //[SerializeField] private AudioManager audio;        // 在 Inspector 指派
    [SerializeField] private string audioManagerTag = "AudioManager"; // 給退路用

    private void Awake()
    {
        //if (!audio)
        //{
        //    var go = GameObject.FindGameObjectWithTag(audioManagerTag);
        //    if (go) audio = go.GetComponent<AudioManager>();
        //    if (!audio)
        //        Debug.LogWarning("[AreaSound] 找不到 AudioManager，請在該場景的 AudioManager 物件加上 'AudioManager' Tag，或直接在 Inspector 指派。", this);
        //}
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!audio) return;
        //if (collision.GetComponent<Player>() != null)
        //    audio.PlaySFX(areaSoundIndex, transform); // 傳入 transform 可支援距離門檻
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //if (!audio) return;
        //if (collision.GetComponent<Player>() != null)
        //    audio.StopSFXWithTime(areaSoundIndex);
    }
}
