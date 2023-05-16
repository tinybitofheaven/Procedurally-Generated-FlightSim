using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public Player_Controller controller;
    public GameObject music;
    public GameObject flying;

    public bool startedMusic = false;

    private void Update()
    {
        if (!controller.onGround)
        {
            flying.SetActive(true);
        }
        else
        {
            flying.SetActive(false);
        }

        if (!startedMusic && Input.GetKeyDown(KeyCode.Space))
        {
            startedMusic = true;
            music.SetActive(true);
        }
    }
}
