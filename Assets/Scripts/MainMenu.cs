using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject levelSelectionMenu;
    public GameObject tutorialMenu;

    private void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(nameof(LateStart));
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(0.1f);
        FindObjectOfType<AudioManager>().PlayMusic("Chunky_Monkey");
    }

    public void Play()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click1");
        levelSelectionMenu.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void SelectLevel(int level)
    {
        FindObjectOfType<AudioManager>().StopSounds();
        FindObjectOfType<AudioManager>().PlaySound("AchievementBell");
        GameConfig.Level = level;
        SceneManager.LoadScene(1);
    }

    public void ShowTutorial()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click1");
        mainMenu.SetActive(false);
        tutorialMenu.SetActive(true);
    }

    public void Back()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click1");
        mainMenu.SetActive(true);
        levelSelectionMenu.SetActive(false);
        tutorialMenu.SetActive(false);
    }

    public void Exit()
    {
        FindObjectOfType<AudioManager>().StopSounds();
        Application.Quit();
    }
}