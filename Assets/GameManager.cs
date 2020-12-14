using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //XML variables
    [Header("Levels")]
    public TextAsset levelsAsset;
    public int levelChosen = 0;
    private List<Dictionary<string, string>> levels = new List<Dictionary<string, string>>();
    private Dictionary<string, string> obj;

    //Variables from CircleSegmentManager.cs
    [Header("Level parameters")]
    public int nLayer = 6;
    public int nSlice = 6;
    public int numberLane = 4; // Number of lanes to play with
    public Color[] segmentColors;
    public int heightFilling = 5;
    public int probabilityBlackLastLayer = 3;
    public string fillingMode = "normal";//easy": maximize the chance to have two blocks next to each other
                                         //"normal": 2 blocks of the same color can be next to each other
                                         //"hard": 2 blocks of the same color can't be next to each other
    
    [Header("Objects")]
    public GameObject planetTop;
    public GameObject planetCore;
    public GameObject projectile;
    public GameObject follower;
    public GameObject maskUI;
    public GameObject gameOverText;
    public GameObject gameWonText;
    public GameObject gamePausedText;
    public CameraController cameraController;

    //Variables from PlayerController.cs
    [Header("Player")]
    public bool enableGravity = true;
    public float gravity = 2000;
    public float jumpForce = 5000;
    public float speed = 2;
    public float throwForce = 2;  // For throwing a projectile

    // Special variable for GameManager
    [Header("Boss")]
    public int lifePoints = 3;
    
    // EndGame
    private bool _gameOver; // true if win or lose
    private bool _gamePaused;

    private void Awake()
    {
        levelChosen = GameConfig.Level;
        
        GetLevel();

        //Level initialization
        Dictionary<string, string> level = levels[levelChosen];
        nSlice = int.Parse(level["nSlice"]);
        nLayer = int.Parse(level["nLayer"]);
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        StartCoroutine(nameof(LateStart));

        /*
        foreach (string key in levels[0].Keys)
        {
            Debug.Log(key);
        }
        */
    }

    IEnumerator LateStart() 
    {
        yield return new WaitForSeconds(0.1f);
        FindObjectOfType<AudioManager>().PlayMusic("Potato");
    }

    private void Update()
    {
        // Pause & Resume game
        if (Input.GetKeyDown(KeyCode.Escape) && !_gameOver)
        {
            PauseGame();
        }
        
        // Resume game
        if (Input.GetKeyDown(KeyCode.Space) && _gamePaused && !_gameOver)
        {
            PauseGame();
        }

        // Restart game
        if (Input.GetKeyDown(KeyCode.Space) && _gameOver)
        {
            SceneManager.LoadScene(1);
        }

        // Return to main menu
        if (Input.GetKeyDown(KeyCode.Q) && (_gameOver || _gamePaused))
        {
            SceneManager.LoadScene(0);
        }
    }

    //Script to load the levels from the XML file
    public void GetLevel()
    {
        XmlDocument xmlDoc = new XmlDocument(); // xmlDoc is the new xml document.
        xmlDoc.LoadXml(levelsAsset.text); // load the file.
        XmlNodeList levelsList = xmlDoc.GetElementsByTagName("level"); // array of the level nodes.

        foreach (XmlNode levelInfo in levelsList)
        {
            XmlNodeList levelcontent = levelInfo.ChildNodes;
            obj = new Dictionary<string, string>(); // Create a object(Dictionary) to colect the both nodes inside the level node and then put into levels[] array.

            foreach (XmlNode levelsItens in levelcontent) // levels itens nodes.
            {

                if (levelsItens.Name == "player")
                {
                    obj.Add("enableGravity", levelsItens.Attributes["enableGravity"].Value); // put this in the dictionary.
                    obj.Add("gravity", levelsItens.Attributes["gravity"].Value); // put this in the dictionary.
                    obj.Add("jumpForce", levelsItens.Attributes["jumpForce"].Value); // put this in the dictionary.
                    obj.Add("speed", levelsItens.Attributes["speed"].Value);; // put this in the dictionary.
                    obj.Add("throwForce", levelsItens.Attributes["throwForce"].Value); ; // put this in the dictionary.
                }

                else
                {
                    obj.Add(levelsItens.Name, levelsItens.Attributes["value"].Value); // put this in the dictionary.
                }
            }
            levels.Add(obj); // add whole obj dictionary in the levels[].
        }
    }


    // Update boss fight phase when Boss is hitted
    public void OnHitBoss(){
        lifePoints = lifePoints - 1;
        CircleSegmentManager _circleSegmentManager = GameObject.Find("Planet Bottom").GetComponent<CircleSegmentManager>();

        if (lifePoints == 0){
            PlayerWins();
        }
        else {
            // Animation
            cameraController.MediumShake();
            FindObjectOfType<AudioManager>().PlaySound("Punch");
            
            // Increase some variables
            speed += 0.5f;

            // Reset puzzle
            _circleSegmentManager.reInit();
        }
    }


    // Win
    public void PlayerWins(){
        FindObjectOfType<AudioManager>().StopSounds();
        FindObjectOfType<AudioManager>().PlaySound("Win");
        Time.timeScale = 0; // Stop game
        maskUI.SetActive(true);
        gameWonText.SetActive(true); // Pop a end menu instead of this
        _gameOver = true;
    }

    // Game Over
    public void PlayerLoses(){
        FindObjectOfType<AudioManager>().StopSounds();
        FindObjectOfType<AudioManager>().PlaySound("Lose");
        Time.timeScale = 0; // Stop game
        maskUI.SetActive(true);
        gameOverText.SetActive(true); // Pop a end menu instead of this
        _gameOver = true;
    }

    public void PauseGame()
    {
        if (!_gamePaused)
        {
            FindObjectOfType<AudioManager>().StopSounds();
            FindObjectOfType<AudioManager>().PlaySound("Pause");
            Time.timeScale = 0; // Pause game
            maskUI.SetActive(true);
            gamePausedText.SetActive(true);
            _gamePaused = true;
        }
        else
        {
            FindObjectOfType<AudioManager>().PlayMusic("Potato");
            Time.timeScale = 1; // Resume game
            maskUI.SetActive(false);
            gamePausedText.SetActive(false);
            _gamePaused = false;
        }
        
    }

}
