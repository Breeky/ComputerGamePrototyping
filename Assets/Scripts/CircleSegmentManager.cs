using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random=UnityEngine.Random;
using TMPro;

public class CircleSegmentManager : MonoBehaviour
{
    private GameManager gameManager;

    // Attributes
    [SerializeField] private GameObject circleDelimiterPrefab;
    [SerializeField] private GameObject lineSegmentPrefab;
    [SerializeField] private GameObject circleSegmentPrefab;
    [SerializeField] private GameObject spawnerPrefab;
    [SerializeField] private GameObject spawnerSegmentPrefab;

    public Color[,] colorBlocks; // Array to keep track of the color by slice and layer
    public CircleSegment[,] segmentsOrdered; // Array to access segments by slice and layer
    private List<float> lanesDist; // To store domain for each lane

    // Variables for filling mode
    private int heightFilling;
    private int probabilityBlackLastLayer;
    private string fillingMode = "normal"; //easy": maximize the chance to have two blocks next to each other
                                          //"normal": 2 blocks of the same color can be next to each other
                                          //"hard": 2 blocks of the same color can't be next to each other

    // In GameManager
    private GameObject planetTop;
    private GameObject planetCore;
    private int numberLane; // Number of lanes to play with
    private TextMeshProUGUI gameOverText;
    private int nLayer;
    private int nSlice;

    private Color[] segmentColors; //Convention: first color = "empty" color (Black by default)

    /* -------------------------------------------------------------------------------------------------------------------------------------------- 
    -------------------------------------------------- Creating the puzzle game -------------------------------------------------------------------
    -------------------------------------------------------------------------------------------------------------------------------------------- */

    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        lanesDist = new List<float>();

        nLayer = gameManager.nLayer;
        nSlice = gameManager.nSlice;
        segmentColors = gameManager.segmentColors;
        heightFilling = gameManager.heightFilling;
        probabilityBlackLastLayer = gameManager.probabilityBlackLastLayer;
        fillingMode = gameManager.fillingMode;
        planetTop = gameManager.planetTop;
        planetCore = gameManager.planetCore;
        numberLane = gameManager.numberLane;
        gameOverText = gameManager.gameOverText;

        GenerateCircleSegments();
        GenerateLineSegmentAndSpawners();
        GenerateCircleDelimiter();

    }


    public void reInit()
    {
        GameObject[] segmentsPrefab = GameObject.FindGameObjectsWithTag("segment");
        for(int i=0; i< segmentsPrefab.Length; i++){GameObject.Destroy(segmentsPrefab[i]);}

        // Init them again
        gameManager = GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>();
        lanesDist = new List<float>();
        nLayer = gameManager.nLayer;
        nSlice = gameManager.nSlice;
        segmentColors = gameManager.segmentColors;
        heightFilling = gameManager.heightFilling;
        probabilityBlackLastLayer = gameManager.probabilityBlackLastLayer;
        fillingMode = gameManager.fillingMode;
        planetTop = gameManager.planetTop;
        planetCore = gameManager.planetCore;
        numberLane = gameManager.numberLane;
        gameOverText = gameManager.gameOverText;

        GenerateCircleSegments();
    }

    private void GenerateCircleDelimiter()
    {
        var radius = planetCore.transform.localScale.x / 2;
        var unitScale = (0.5 - radius) / nLayer;
        radius += (float)unitScale;
        for (int i = 0; i < nLayer; i++)
        {
            var circleDelimiter = Instantiate(circleDelimiterPrefab, transform);
            var renderer = circleDelimiter.GetComponent<CircleLineRenderer>();
            renderer.radius = radius;

            radius += (float)unitScale;
        }
    }

    private void GenerateLineSegmentAndSpawners()
    {
        float innerRadius = planetTop.GetComponent<InnerCircleCollider>().Radius * GameObject.Find("Planet Bottom").transform.localScale.x;
        float outerRadius = planetTop.GetComponent<InnerCircleCollider>().Radius * GameObject.Find("Planet Top").transform.localScale.x;
        float distStep = (outerRadius - innerRadius) / numberLane;
        for (int i = 0; i < numberLane; i++)
        {
            lanesDist.Add(innerRadius + 0.5f * distStep + i * distStep);
        }

        var unitAngle = 2*Math.PI / nSlice;
        var radius = GetComponent<SpriteRenderer>().bounds.size[0] / 2;
        for (int i = 0; i < nSlice; i++)
        {
            var angle = i * unitAngle + Math.PI/2;
            var line = Instantiate(lineSegmentPrefab, transform);
            var renderer = line.GetComponent<LineRenderer>();
            renderer.SetPosition(1, new Vector3(
                radius * Mathf.Cos((float)angle),
                radius * Mathf.Sin((float)angle),
                0f
                ));

            //Instantiate a line of spawners, with a SpawnerSegment on laneDist[0]

            Vector3 spawnPosition = new Vector3(
               lanesDist[0] * Mathf.Cos((float)(angle + unitAngle / 2)),
               lanesDist[0] * Mathf.Sin((float)(angle + unitAngle / 2)),
               0f);
            GameObject spawnerSegment = Instantiate(spawnerSegmentPrefab, spawnPosition, Quaternion.identity);
           

            foreach (float dist in lanesDist)
            {
                spawnPosition = new Vector3(
                dist * Mathf.Cos((float)(angle + unitAngle/2)),
                dist * Mathf.Sin((float)(angle + unitAngle / 2)),
                0f);
                GameObject newSpawner = Instantiate(spawnerPrefab, spawnPosition, Quaternion.identity);
                newSpawner.transform.parent = spawnerSegment.transform;
            }
        }
    }

    public void GenerateCircleSegments()
    {
        float unitAngle = (float) (Math.PI / nSlice);
        float unitScale = (1 - planetCore.transform.localScale.x) / nLayer;
        int order = nLayer*nSlice + 1;  // Careful : planetCore orderInLayer must be greater than this one

        colorBlocks = new Color[nSlice, nLayer]; // Generate 2D array of colors
        segmentsOrdered = new CircleSegment[nSlice, nLayer]; // Generate 2D array of segments

        Color[,] colorMapping = CreateInitialMapping(heightFilling, probabilityBlackLastLayer, fillingMode); // Generate initial mapping

        for (int i = 0; i < nLayer; i++)
        {
            for (int j = 0; j < nSlice; j++)
            {
                GameObject segment = Instantiate(circleSegmentPrefab, transform);
                SpriteRenderer renderer = segment.GetComponent<SpriteRenderer>();
                Material segmentMaterial = renderer.material;
                CircleSegment circleSegment = segment.GetComponent<CircleSegment>();

                // Size
                segment.transform.localScale =
                    planetCore.transform.localScale + (i + 1) * new Vector3(unitScale, unitScale, unitScale);

                // Layer
                renderer.sortingOrder = order;

                // Shader for creating an arc
                segmentMaterial.SetFloat("_Angle", unitAngle);

                // Setting the arc at the right position
                float rotate = -2 * j * unitAngle - (float) unitAngle;
                segment.transform.rotation = Quaternion.Euler(0f, 0f, rotate * Mathf.Rad2Deg);
                
                // Setting the collider
                // CircleSegment circleSegment = segment.GetComponent<CircleSegment>();
                float angle = j*2*unitAngle;
                circleSegment.Initialize(
                    0.5f*(planetCore.transform.localScale.x + i * unitScale)/(segment.transform.localScale.x), // innerRadius
                    0.5f*(planetCore.transform.localScale.x + (i+1) * unitScale)/(segment.transform.localScale.x), // outerRadius
                    angle+rotate+Mathf.PI/2,// startAngle
                    angle+rotate+Mathf.PI/2 + 2*unitAngle, // endAngle
                    j, // slice
                    i //layer
                    );
                
                // Color
                // renderer.color = segmentColors[order % segmentColors.Length]; // Replace by a better chosen color
                circleSegment.ChangeColor(colorMapping[j,i], false);
                colorBlocks[j,i] = colorMapping[j,i]; // Update color 
                colorBlocks[j,i] = colorBlocks[j,i];

                order -= 1;

                // Save segment in array
                segmentsOrdered[j,i] = circleSegment;

            }
        }
    }

    public Color[,] CreateInitialMapping(int height, int pUpper, String fillingMode = "normal")
    {
        /* This function takes as input:
                * height: the number of layers to fill with boxes 
                * fillingMode: the mode to fill blocks: 
                        - "easy": maximize the chance to have two blocks next to each other
                        - "normal": 2 blocks of the same color can be next to each other
                        - "hard": 2 blocks of the same color can't be next to each other
                * pUpper: 1/pUpper probbility for a block of the upper layer to be an empty block
           And Output:
                * Array with colors by slice and layer

        
        Some other easy improvements that could be added to complexify the levels:
            * (No) Add the possibility to not use every color of the colorBox (allow to increase the number of colors we play with during different phases of a boss fight) 
            * (--) Add some obstacles (tiles of a color not matching any ball color)
        */

        // Be default, the filling mode is normal
        if ((fillingMode != "easy") && (fillingMode != "normal") && (fillingMode != "hard")){
            fillingMode = "normal";
        }

        // --------- Initialization of variables --------- //
        // General
        int nColor = segmentColors.Length;
        Color randomColor;
        Color[,] colorMapping = new Color[nSlice, nLayer];

        // Easy filling mode
        int e; 

        // Normal filling mode
        int[,] colorOccurences = new int[nSlice, nLayer]; // Array of occurences of the color of the block at the position (j,i) in its neighboroud 
        Dictionary<Color, int> occurenceEachColor = new Dictionary<Color, int>(); // Helper to compute the occurence of neighbor color of a block
        Dictionary<Color, Vector2Int> posEachColor =  new Dictionary<Color, Vector2Int>(); // To keep track of position of other colors to update their counter of occurences if needed
        if (fillingMode == "easy" || fillingMode == "normal"){
            for (int i=1; i < segmentColors.Length; i++) {
                occurenceEachColor.Add(segmentColors[i], 0);
                posEachColor.Add(segmentColors[i], new Vector2Int(-1, -1)); // Initialize with impossible values of Vec2 as null type doesn't exist for Vector2
            }
        }

        // Hard filling mode
        List<Color> colorImpossible = new List<Color>();
                
        // Fill Initial Mapping with color of empty blocks, and color occurences with 0 occurences (if normal fillingMode)
        for (int i = 0; i < nLayer; i++){
            for (int j = 0; j < nSlice; j++){
                colorMapping[j,i] = segmentColors[0];

                if (fillingMode == "easy" || fillingMode == "normal"){
                    colorOccurences[j,i] = 0; 
                }
            }
        }

        
        // In normal mode, we need at least 3 differents colors in addition to the empty color
        if ((fillingMode == "easy" || fillingMode == "normal") && (nColor < 5)){
            throw new Exception("Please use at least 4 differents colors in addition to the empty color to generate the Map in easy or normal mode");
        // In hard mode, we need at least 5 differents colors in addition to the empty color
        } else if ((fillingMode == "hard") && (nColor < 6)) {
            throw new Exception("Please use at least 5 differents colors in addition to the empty color to generate the Map in hard mode");
        }


        // Loop to assign a random Color to each Block
        for (int i = 0; i < height; i++){
            for (int j = 0; j < nSlice; j++){
                
                // --------- 1 on p_upper chance to pick an empty block on the upper layer --------- //
                if ((i == height - 1) && (Random.Range(0, pUpper)  == 0)) {
                    randomColor = segmentColors[0];
                } else {

                    // --------- Reinitialize variables --------- //
                    if (fillingMode == "easy" || fillingMode == "normal"){
                        for (int z=1; z < segmentColors.Length; z++) {
                            occurenceEachColor[segmentColors[z]] = 0;
                            posEachColor[segmentColors[z]] = new Vector2Int(-1, -1);
                        }
                        
                    } else if ((fillingMode == "hard")){
                        colorImpossible = new List<Color>();
                    }
                    
                    // --------- Count occurences of surrounding colors (normal) and find all surrounding impossible colors (hard) --------- //
                    // Right
                    if (fillingMode == "easy" || fillingMode == "normal") {
                        if (colorMapping[((j+1) % nSlice),i] != segmentColors[0]){
                            occurenceEachColor[colorMapping[((j+1) % nSlice),i]] += colorOccurences[((j+1) % nSlice),i];
                            posEachColor[colorMapping[((j+1) % nSlice),i]] = new Vector2Int(((j+1) % nSlice),i);
                        }
                    } else if (fillingMode == "hard") {
                        colorImpossible.Add(colorMapping[((j+1) % nSlice),i]);
                    }
                    

                    // Left
                    if (fillingMode == "easy" || fillingMode == "normal") {
                        if ((j-1) == -1) {
                            if (colorMapping[(nSlice - 1),i] != segmentColors[0]){
                                occurenceEachColor[colorMapping[(nSlice - 1),i]] += colorOccurences[(nSlice - 1),i];
                                posEachColor[colorMapping[(nSlice - 1),i]] = new Vector2Int((nSlice - 1),i);
                            }
                        } else {
                            if (colorMapping[j-1,i] != segmentColors[0]){
                                occurenceEachColor[colorMapping[j-1,i]] += colorOccurences[j-1,i];
                                posEachColor[colorMapping[j-1,i]] = new Vector2Int(j-1,i);
                            }
                        }
                    } else if (fillingMode == "hard") {
                        if ((j-1) == -1) {
                            colorImpossible.Add(colorMapping[(nSlice - 1),i]);
                        } else {
                            colorImpossible.Add(colorMapping[j-1,i]);
                        }
                    }    
                    

                    // Top
                    if (fillingMode == "easy" || fillingMode == "normal") {
                        if (colorMapping[j,i+1] != segmentColors[0]){
                            occurenceEachColor[colorMapping[j,i+1]] += colorOccurences[j,i+1];
                            posEachColor[colorMapping[j,i+1]] = new Vector2Int(j,i+1);
                        }
                    } else if (fillingMode == "hard") {
                        colorImpossible.Add(colorMapping[j,i+1]);
                    }
                    

                    // Bot
                    if (i - 1 != -1) {
                        if (fillingMode == "easy" || fillingMode == "normal") {
                            if (colorMapping[j,i-1] != segmentColors[0]){
                                occurenceEachColor[colorMapping[j,i-1]] += colorOccurences[j,i-1];
                                posEachColor[colorMapping[j,i-1]] = new Vector2Int(j,i-1);
                            }
                        } else if (fillingMode == "hard") {
                            colorImpossible.Add(colorMapping[j,i-1]);
                        }       
                    }

                    // --------- Pick an admissible color --------- //
                    randomColor = segmentColors[Random.Range(1, nColor)];


                    if (fillingMode == "easy"){
                        // Try to pick a color with 1 occurence 15 times, or take a color as in normal mode otherwise
                        e = 0;
                        while(occurenceEachColor[randomColor] != 1 && e < 15){
                            randomColor = segmentColors[Random.Range(1, nColor)];
                            e += 1;
                        }
                        if (occurenceEachColor[randomColor] != 1) {
                            while(occurenceEachColor[randomColor] > 1){
                                randomColor = segmentColors[Random.Range(1, nColor)];
                            }
                        }
                    } else if (fillingMode == "normal") {
                        // Pick a color with 1 occurence as a maximum
                        while(occurenceEachColor[randomColor] > 1){
                            randomColor = segmentColors[Random.Range(1, nColor)];
                        }

                    } else if (fillingMode == "hard") {
                        // Pick random color as long as it has not already been chosen for a surrounding block
                        while(colorImpossible.Contains(randomColor)){
                            randomColor = segmentColors[Random.Range(1, nColor)];
                        }
                    }
                }
               
                // --------- Update color --------- //
                colorMapping[j, i] = randomColor;
                if (fillingMode == "easy" || fillingMode == "normal") {
                    if (randomColor != segmentColors[0]) {
                        colorOccurences[j,i] = occurenceEachColor[randomColor] + 1;
                        //Debug.Log("Occurence is " + colorOccurences[j,i] + " for slice " + j + " for layer " + i);

                        // Update color occurence of neighbour if needed
                        if (posEachColor[randomColor].x != -1){
                            colorOccurences[posEachColor[randomColor].x, posEachColor[randomColor].y] += 1;

                        }
                    }
                }
            }
        }
        return colorMapping;
    }


    

    /* -------------------------------------------------------------------------------------------------------------------------------------------- 
    -----------------------------------------Management of the matching of blocks-------------------------------------------------
    -------------------------------------------------------------------------------------------------------------------------------------------- */


    public void ManageMatching(int slice, int layer)
    {
        /* This function takes as input the position of the box from which to search for matches
        and manage the matching of 3 (or more) Boxes 
        Careful: this script only works with the rule of 3 blocks matching */

        // -------------------- Initialization of Variables -------------------- //
        Color colorOfBlock = colorBlocks[slice, layer]; // Color to search for the matching
        int counter = 1; // Counter of number of boxes matching
        List<Vector2Int> positionsMatching = new List<Vector2Int>(); // To keep track of the positions of every segment matching
        positionsMatching.Add(new Vector2Int(slice, layer));

        // -------------------- Search in all potential directions -------------------- //
        (counter, positionsMatching) = SearchLeft(slice, layer, colorOfBlock, counter, positionsMatching);
        (counter, positionsMatching) = SearchRight(slice, layer, colorOfBlock, counter, positionsMatching);
        (counter, positionsMatching) = SearchBot(slice, layer, colorOfBlock, counter, positionsMatching);
        // There is no need to search to the top of the newly added block

        // -------------------- Remove matching blocks -------------------- //
        // Debug.Log("Value of counter after matching check: " + counter);

        /*
        // To debug positionMatching
        string result = "positionMatching contents: ";
        foreach (var item in positionsMatching)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result); 
        */
        
        if (counter >=3) {
            HashSet<Vector2Int> movedBlocks;
            movedBlocks = RemoveMatchingBlocks(positionsMatching); 
            
            /*
            // To debug movedBlocks
            string result1 = "movedBlocks contents: ";
            foreach (var item in movedBlocks)
            {
                result1 += item.ToString() + ", ";
            }
            Debug.Log(result1); 
            */

            // -------------------- Check and manage matching of moved blocks during the removing process (if any) -------------------- //
            
            // To store all HashSet of moved blocks
            List<HashSet<Vector2Int>> movedBlocksPile = new List<HashSet<Vector2Int>>(); 
            if (movedBlocks.Count != 0){
                movedBlocksPile.Add(movedBlocks);
            }

            // To store moved blocks that were already found to be involved in a match
            HashSet<Vector2Int> blacklistMovedBlocks; 

            // Continue as long as the pile of moved blocks to go through is not empty
            while (movedBlocksPile.Count != 0) {
                
                // Load the first list of moved blocks and remove it form the pile
                movedBlocks = movedBlocksPile[0];
                movedBlocksPile.RemoveAt(0);
                blacklistMovedBlocks = new HashSet<Vector2Int>();

                // Manage normally the matching
                foreach (Vector2Int positionToCheck in movedBlocks) {
                    
                    // There is no need to check if there is a matching for a block for which this has already been searched for
                    if (!blacklistMovedBlocks.Contains(positionToCheck)) {

                        // Reinitialize variables
                        colorOfBlock = colorBlocks[positionToCheck.x, positionToCheck.y]; // Color to search for the matching
                        counter = 1; // Counter of number of boxes matching
                        positionsMatching = new List<Vector2Int>(); // To keep track of the positions of every segment matching
                        positionsMatching.Add(positionToCheck);

                        // Research in all potential directions
                        (counter, positionsMatching) = SearchLeft(slice, layer, colorOfBlock, counter, positionsMatching);
                        (counter, positionsMatching) = SearchRight(slice, layer, colorOfBlock, counter, positionsMatching);
                        (counter, positionsMatching) = SearchBot(slice, layer, colorOfBlock, counter, positionsMatching);

                        // Blacklist (= don't search for a matching involving ...) a moved block if it is part of the current match
                        if (positionsMatching.Contains(positionToCheck)){
                            blacklistMovedBlocks.Add(positionToCheck);
                        }
                        
                        // If a new match is found, matching blocks are removed and a new list of moved blocks is added at the end of the pile
                        if (counter >=3) {
                            HashSet<Vector2Int> movedBlocksAdded;
                            movedBlocksAdded = RemoveMatchingBlocks(positionsMatching); 
                            movedBlocksPile.Add(movedBlocksAdded);
                        }
                    }
                }
            }
        }
    }



    private (int counter, List<Vector2Int> positionsMatching) SearchRight(int slice, int layer, Color colorOfBlock, int counter, List<Vector2Int> positionsMatching)
    {
        int j = slice + 1;
        j = j % nSlice;
        //Debug.Log("Slice visited Right: " + j);

        // -------------------- Search to the right -------------------- //
        // Search to the right as long as the color match
        // Potential infinite loop if we play with a grid of only 5 slices
        // Debug.Log("Color searched" + colorOfBlock);
        // Debug.Log("Color tested" + colorBlocks[j, layer]);
        while(colorBlocks[j, layer] == colorOfBlock) {
            
            // Update variables
            counter += 1;
            positionsMatching.Add(new Vector2Int(j, layer));

            // -------------------- Search to the top from new blocks found -------------------- //
            int i = layer + 1;
            if (i != nLayer) {

                // Search to the top as long as the color match
                while(colorBlocks[j, i] == colorOfBlock) {
                    
                    // Update variables
                    counter += 1;
                    positionsMatching.Add(new Vector2Int(j, i));

                    // Update layer
                    i += 1;

                    // Stop searching to the top if we are outside the grid
                    if (i == nLayer) {
                        break;
                    }
                }
            }

            // -------------------- Search to the bottom from new blocks found -------------------- //
            i = layer - 1;
            if (i != -1) {

                // Search to the bottom as long as the color match
                while(colorBlocks[j, i] == colorOfBlock) {
                    
                    // Update variables
                    counter += 1;
                    positionsMatching.Add(new Vector2Int(j, i));

                    // Update layer
                    i -= 1;

                    // Stop searching to the bottom if we are outside the grid
                    if (i == -1) {
                        break;
                    }
                }
            }

            // -------------------- Update slice -------------------- //
            j += 1;
            j = j % nSlice;
            //Debug.Log("Slice visited Right: " + j);

        }

        return (counter, positionsMatching);
    }



    private (int counter, List<Vector2Int> positionsMatching) SearchLeft(int slice, int layer, Color colorOfBlock, int counter, List<Vector2Int> positionsMatching)
    {
        int j = slice -1;
        if (j == -1) {
            j = nSlice - 1;
        }
        //Debug.Log("Slice visited Left: " + j);

        // -------------------- Search to the left -------------------- //
        // Search to the left as long as the color match
        // Potential infinite loop if we play with a grid of only 5 slices
        while(colorBlocks[j, layer] == colorOfBlock) {
            
            // Update variables
            counter += 1;
            positionsMatching.Add(new Vector2Int(j, layer));

            // -------------------- Search to the top from new blocks found -------------------- //
            int i = layer + 1;
            if (i != nLayer) {

                // Search to the top as long as the color match
                while(colorBlocks[j, i] == colorOfBlock) {
                    
                    // Update variables
                    counter += 1;
                    positionsMatching.Add(new Vector2Int(j, i));

                    // Update layer
                    i += 1;

                    // Stop searching to the top if we are outside the grid
                    if (i == nLayer) {
                        break;
                    }
                }
            }
            // -------------------- Search to the bottom from new blocks found -------------------- //
            i = layer - 1;
            if (i != -1) {

                // Search to the bottom as long as the color match
                while(colorBlocks[j, i] == colorOfBlock) {
                    
                    // Update variables
                    counter += 1;
                    positionsMatching.Add(new Vector2Int(j, i));

                    // Update layer
                    i -= 1;

                    // Stop searching to the bottom if we are outside the grid
                    if (i == -1) {
                        break;
                    }
                }
            }

            // -------------------- Update slice -------------------- //
            j -= 1;
            if (j == -1) {
                j = nSlice - 1;
            }
            //Debug.Log("Slice visited Left: " + j);

        }

        return (counter, positionsMatching);
    }



    private (int counter, List<Vector2Int> positionsMatching) SearchBot(int slice, int layer, Color colorOfBlock, int counter, List<Vector2Int> positionsMatching)
    {
        int i = layer - 1;
        if (i == -1) {
            return (counter, positionsMatching);
        }
        //Debug.Log("Layer visited Bottom: " + i);

        // Search to the bottom as long as the color match and we stay in the grid
        while(colorBlocks[slice, i] == colorOfBlock) {
            
            // Update variables
            counter += 1;
            positionsMatching.Add(new Vector2Int(slice, i));

            // -------------------- Search to the left from new blocks found -------------------- //
            int j = slice - 1;
            if (j == -1) {
                j = nSlice - 1;
            }

            // Search to the left as long as the color match
            while(colorBlocks[j, i] == colorOfBlock) {

                // TODO: check in positionsMatching that the block is not already counted ?
                // It is impossible in a 3 boxes matching game (the blocks would have already match previously)
                
                // Update variables
                counter += 1;
                positionsMatching.Add(new Vector2Int(j, i));

                // Update slice
                j -= 1;
                if (j == -1) {
                    j = nSlice - 1;
                }
                

            }

            // -------------------- Search to the right from new blocks found -------------------- //
            j = slice + 1;
            j = j % nSlice;

            // Search to the right as long as the color match
            while(colorBlocks[j, i] == colorOfBlock) {

                // TODO: check in positionsMatching that the block is not already counted ?
                // It is impossible in a 3 boxes matching game (the blocks would have already match previously)
                
                // Update variables
                counter += 1;
                positionsMatching.Add(new Vector2Int(j, i));

                // Update slice
                j += 1;
                j = j % nSlice;

            }

            // -------------------- Update layer -------------------- //
            i -= 1;
            // Stop searching to the bottom if we are outside the grid
            if (i == -1) {
                break;
            }
            //Debug.Log("Layer visited Bottom: " + i);
        }

        return (counter, positionsMatching);
    }


    private HashSet<Vector2Int> RemoveMatchingBlocks(List<Vector2Int> positionsMatching) 
    {   
        // Sort the list of matching blocks by position
        // The upper layer in a column has to be managed in first
        List<Vector2Int> positionsMatchingSorted = positionsMatching.OrderBy(x => x.x).ThenByDescending(x => x.y).ToList();

        // List to save position of all moved blocks during the process of removing matching blocks
        HashSet<Vector2Int> movedBlocks = new HashSet<Vector2Int>();

        foreach (Vector2Int position in positionsMatchingSorted) {

            // Replace the color of all Matching blocks by black
            colorBlocks[position.x, position.y] = segmentColors[0];
            segmentsOrdered[position.x, position.y].ChangeColor(segmentColors[0]);
            // Debug.Log("removed !");

            /* There is nothing to do if 
                * the removed block is at the top of a column
                * there is an empty block on top of the block to be removed */
            if (position.y != nLayer - 1) {
                if (colorBlocks[position.x, position.y + 1] != segmentColors[0]) {

                    // Downgrade position of all colored blocks on top of removed block
                    for (int i = position.y; i < nLayer - 1; i++) {

                        // If the block on top is colored, add it to the HashMap of movedBlocks to search for matching of moved blocks later
                        if (colorBlocks[position.x, i + 1] != segmentColors[0]) {
                            movedBlocks.Add(new Vector2Int(position.x, i));
                        }

                        // Update block color
                        colorBlocks[position.x, i] = colorBlocks[position.x, i + 1];
                        segmentsOrdered[position.x, i].ChangeColor(colorBlocks[position.x, i + 1]);

                        // Empty block on top
                        colorBlocks[position.x, i + 1] = segmentColors[0];
                        segmentsOrdered[position.x, i + 1].ChangeColor(segmentColors[0]);

                    }
                }
            }
        }
        // An empty block could be counted due to the way the position is downgraded
        // For instance in the case of two blocks to be removed with a moved block above them, the upper removed block position will be counted
        // This loop remove all miscounted blocks
        HashSet<Vector2Int> movedBlocksFinal = new HashSet<Vector2Int>();
        foreach (Vector2Int positionToCheck in movedBlocks) {
            if (colorBlocks[positionToCheck.x, positionToCheck.y] != segmentColors[0]) {
                movedBlocksFinal.Add(positionToCheck);
            }
        }
        /*
        foreach (Vector2Int positionToCheck in movedBlocks) {
            if (colorBlocks[positionToCheck.x, positionToCheck.y] == segmentColors[0]) {
                movedBlocks.Remove(positionToCheck);
            }
        }
        */

        return movedBlocksFinal;
        
    }
}
