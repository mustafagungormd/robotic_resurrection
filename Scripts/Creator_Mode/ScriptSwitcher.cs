using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class ScriptSwitcher : MonoBehaviour
{
    public MonoBehaviour script1;
    public MonoBehaviour script2;
    public GameObject canvas1, canvas2;
    public GameObject player;
    public GameObject targetObject; // El ile atayacağınız GameObject

    private bool isOn = false;
    private VideoPlayer videoPlayer;

    void Start()
    {
        if (targetObject != null)
        {
            videoPlayer = targetObject.GetComponent<VideoPlayer>();
            if (videoPlayer == null)
            {
                Debug.LogError("Hedef obje üzerinde VideoPlayer komponenti bulunamadı.");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (isOn)
            {
                OFF();
            }
            else
            {
                ON();
            }
            isOn = !isOn;
        }
        
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (isOn)
            {
                OFFPh();
            }
            else
            {
                ONPh();
            }
            isOn = !isOn;
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        { 
            ExtraOutro();
        }
        
        if (Input.GetKeyDown(KeyCode.V))
        { 
            QuitExtraOutro();
        }
    }

    public void ONPh()
    {
        canvas1.SetActive(true);
        canvas2.SetActive(true);
        player.SetActive(true);
    }

    public void OFFPh()
    {
        canvas1.SetActive(false);
        canvas2.SetActive(false);
        player.SetActive(false);
    }

    public void OFF()
    {

        script1.enabled = true;
        script2.enabled = false;
        if (videoPlayer != null)
        {
            videoPlayer.playbackSpeed = 1f;
        }
    }

    public void ON()
    {

        script1.enabled = false;
        script2.enabled = true;
        if (videoPlayer != null)
        {
            videoPlayer.playbackSpeed = 10f;
        }
    }

    public void ExtraOutro()
    {
        SceneManager.LoadScene("SoundtrackOutro");
    }

    public void QuitExtraOutro()
    {
        SceneManager.LoadScene("Map1");
    }
    
    
}
