using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1f;
    }

    public void Play()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click1");
        SceneManager.LoadScene(1);
    }
}
