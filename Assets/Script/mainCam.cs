using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
public class mainCam : MonoBehaviour
{
    public GameObject mainCamera;
    public GameObject moveCamera;
    public PlayableDirector playableDirector;
    public PlayableDirector back_playableDirector;
// Start is called before the first frame update
    void Start()
    {
        mainCamera.SetActive(true);
        moveCamera.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void play()
    {
        playableDirector.Play();
    }
    void backplay()
    {
        back_playableDirector.Play();
    }
    void change()
    {
        mainCamera.SetActive(true);
        moveCamera.SetActive(false);
    }
    public void onClick_main()
    {
        backplay();       
        Invoke("change",2f);
    }
    public void onClick_move()
    {
        mainCamera.SetActive(false);
        moveCamera.SetActive(true);
        // animation.Play();
        play();
    }

    
}
