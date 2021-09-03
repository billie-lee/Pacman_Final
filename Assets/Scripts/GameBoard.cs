using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameBoard : MonoBehaviour {

    private static int boardWidth = 28;
    private static int boardHeight = 36;

    private bool didStartDeath = false;
    private bool didStartConsume = false;

    private static int playerLevel = 1;

    public int totalPelletsConsumed = 0;

    public int totalPellets = 0;
    public static int score = 0;
    public int pacManLives = 3;
    public bool shouldBlink = false;

    public float blinkIntervalTime = 0.1f;
    private float blinkIntervalTimer = 0;

    public Text readyText;
    public Text playerText;
    public Text UIScore;
    public Text consumedGhostText;
    public Image livesTwo;
    public Image livesThree;

    public GameObject[,] board = new GameObject[boardWidth, boardHeight];

    public Sprite mazeBlue;
    public Sprite mazeWhite;

    // Start is called before the first frame update
    void Start() {

        Object[] objects = GameObject.FindObjectsOfType (typeof(GameObject));

        foreach (GameObject o in objects) {

            Vector2 pos = o.transform.position;
            if(o.name != "PacMan" && o.tag != "Ghost" && o.tag != "GhostHome" && o.tag != "UIElement") {
                
                if(o.GetComponent<Tile>() != null){
                    if(o.GetComponent<Tile>().isPellet || o.GetComponent<Tile>().isSuperPellet)
                    totalPellets++;
                }

                board [(int)pos.x, (int)pos.y] = o;

            } else {
                Debug.Log("Found Pacman at: " +pos);
            }

        }
        StartGame();
    }

    public void StartGame(){
        //Hide ghosts and pacman
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
            ghost.transform.GetComponent<Ghost>().canMove = false;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        pacMan.transform.GetComponent<PacMan>().canMove = false;

        StartCoroutine (ShowSpritesAfter(2));
    }

    IEnumerator StartBlinking(Text blnkTxt){
        yield return new WaitForSeconds(0.25f);

        blnkTxt.GetComponent<Text>().enabled = !blnkTxt.GetComponent<Text>().enabled;

        StartCoroutine(StartBlinking(blnkTxt));
    }

    public void StartConsumed(Ghost consumedGhost){
        if(!didStartConsume){
            didStartConsume = true;

            //Pause Ghost and pacman
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

            foreach(GameObject ghost in ghosts){
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }
            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            //Hide Pacman
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            //Hide Consumed Ghost
            consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = false;


            //Show score of consumed ghost
            Vector2 consumedGhostPosition = consumedGhost.transform.position;

            Vector2 viewPortPoint = Camera.main.WorldToViewportPoint(consumedGhostPosition);

            consumedGhostText.GetComponent<RectTransform>().anchorMin = viewPortPoint;
            consumedGhostText.GetComponent<RectTransform>().anchorMax = viewPortPoint;

            consumedGhostText.GetComponent<Text>().enabled = true;

            StartCoroutine(ProcessConsumedGhostAfter(0.75f, consumedGhost));
        }
    }

    IEnumerator ProcessConsumedGhostAfter(float delay, Ghost consumedGhost){

        yield return new WaitForSeconds(delay);

        //Hide scores
        consumedGhostText.GetComponent<Text>().enabled = false;

        //Show Pacman
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        //Show Ghost
        consumedGhost.transform.GetComponent<SpriteRenderer>().enabled = true;

        //Resume all ghosts and pacman
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        pacMan.transform.GetComponent<PacMan>().canMove = true;

        didStartConsume = false;
    }

    IEnumerator ShowSpritesAfter(float delay){
        yield return new WaitForSeconds(delay);

        //Show ghosts and pacman
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;

        playerText.transform.GetComponent<Text>().enabled = false;

        StartCoroutine(StartGameAfter(2));

    }

    IEnumerator StartGameAfter(float delay){
        yield return new WaitForSeconds(delay);

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<Ghost>().canMove = true;
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = true;

        readyText.transform.GetComponent<Text>().enabled = false;
    }

    public void StartDeath(){
        if(!didStartDeath){

            didStartDeath = true;

            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

            foreach(GameObject ghost in ghosts){
                ghost.transform.GetComponent<Ghost>().canMove = false;
            }

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<PacMan>().canMove = false;

            pacMan.transform.GetComponent<Animator>().enabled = false;

            StartCoroutine(ProcessDeathAfter(1));
        }

    }

    IEnumerator ProcessDeathAfter(float delay){
        yield return new WaitForSeconds (delay);
       
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        StartCoroutine(ProcessDeathAnimation(1.2f));

    }

    IEnumerator ProcessDeathAnimation(float delay){
        GameObject pacMan = GameObject.Find("PacMan");

        pacMan.transform.localScale = new Vector3(1, 1, 1);
        pacMan.transform.localRotation = Quaternion.Euler(0, 0, 0);

        pacMan.transform.GetComponent<Animator>().runtimeAnimatorController = pacMan.transform.GetComponent<PacMan>().deathAnimation;
        pacMan.transform.GetComponent<Animator>().enabled = true;

        yield return new WaitForSeconds(delay);

        StartCoroutine(ProcessRestart(1));

    }

    IEnumerator ProcessRestart(float delay){

        pacManLives -= 1;

        if(pacManLives == 0){
            playerText.transform.GetComponent<Text>().enabled = true;

            readyText.transform.GetComponent<Text>().text = "Game Over";
            readyText.transform.GetComponent<Text>().color = Color.red;

            readyText.transform.GetComponent<Text>().enabled = true;

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            StartCoroutine(ProcessGameOver(2));

        } else {

            playerText.transform.GetComponent<Text>().enabled = true;
            readyText.transform.GetComponent<Text>().enabled = true;

            GameObject pacMan = GameObject.Find("PacMan");
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

            yield return new WaitForSeconds(delay);

            StartCoroutine(ProcessRestartShowSprites(1));
        
        }
    }

    IEnumerator ProcessGameOver(float delay){

        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene("Interface");
    }

    IEnumerator ProcessRestartShowSprites(float delay){
        playerText.transform.GetComponent<Text>().enabled = false;

        //Show ghosts and pacman
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<SpriteRenderer>().enabled = true;
            ghost.transform.GetComponent<Ghost>().MoveToStartingPosition();
        }

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = true;
        pacMan.transform.GetComponent<PacMan>().MoveToStartingPosition();

        yield return new WaitForSeconds(delay);

        Restart();
    }

    public void Restart(){

        readyText.transform.GetComponent<Text>().enabled = false;

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().Restart();

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<Ghost>().Restart();
        }

        didStartDeath = false;
    }

    // Update is called once per frame
    void Update(){
        UpdateUI();
        CheckPelletsConsumed();
        CheckShouldBlink();
    }

    void UpdateUI(){
        UIScore.text = score.ToString();

        if(pacManLives == 3){
            livesThree.GetComponent<Image>().enabled = true;
            livesTwo.GetComponent<Image>().enabled = true;
        } else if(pacManLives == 2){
            livesThree.GetComponent<Image>().enabled = false;
            livesTwo.GetComponent<Image>().enabled = true;
        } else if(pacManLives == 1){
            livesThree.GetComponent<Image>().enabled = false;
            livesTwo.GetComponent<Image>().enabled = false;
        }
    }

    void CheckPelletsConsumed(){
        if(totalPellets == totalPelletsConsumed){
            PlayerWin();
        }
    }

    void PlayerWin(){
        playerLevel ++;

        StartCoroutine(ProcessWin(2));
    }

    IEnumerator ProcessWin(float delay){

        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<PacMan>().canMove = false;
        pacMan.transform.GetComponent<Animator>().enabled = false;

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            ghost.transform.GetComponent<Ghost>().canMove = false;
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        yield return new WaitForSeconds(delay);

        StartCoroutine(BlinkBoard(2));
    }

    IEnumerator BlinkBoard(float delay){
        GameObject pacMan = GameObject.Find("PacMan");
        pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;

        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        foreach(GameObject ghost in ghosts){
            pacMan.transform.GetComponent<SpriteRenderer>().enabled = false;
        }

        shouldBlink = true;
        yield return new WaitForSeconds(delay);
        shouldBlink = false;

        LoadInterface();
    }

    void LoadInterface(){
        // Call main menu
        SceneManager.LoadScene("Interface");
    }

    private void CheckShouldBlink(){
        if(shouldBlink){
            if(blinkIntervalTimer < blinkIntervalTime){
                blinkIntervalTimer += Time.deltaTime;
            } else {
                blinkIntervalTimer = 0;

                if(GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite == mazeBlue){
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeWhite;
                } else {
                    GameObject.Find("Maze").transform.GetComponent<SpriteRenderer>().sprite = mazeBlue;
                }
            }
        }
    }
}
