using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public string sceneName;
    public string spawnPosName;
    private bool activated;

    private Player player;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!activated)
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if(player)
            {
                PlayerPrefs.SetString("NextEntryPoint", spawnPosName);
                SceneManager.LoadScene(sceneName);
            }
        }
    }

    public void LoadLevel(string sceneName, string spawnPosName)
    {
        this.spawnPosName = spawnPosName;
        player.rb.gravityScale = 0f;
    }
}
