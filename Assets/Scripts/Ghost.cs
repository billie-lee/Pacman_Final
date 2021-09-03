using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost : MonoBehaviour {

    public float speed = 3.9f;
    public float normalSpeed = 3.9f;
    public float previousMoveSpeed;
    public float frightenedMoveSpeed = 2.0f;
    public float consumedMoveSpeed = 15f;

    public bool canMove = true;

    public float pinkyReleaseTimer = 5;
    public float blueyReleaseTimer = 21;
    public float orangeyReleaseTimer = 14;
    public float ghostReleaseTimer = 0;

    public bool isInGhostHouse = false;

    public Node startingPosition;
    public Node homeNode;
    public Node ghostHome;

    public int scatterModeTimer1 = 7;
    public int chaseModeTimer1 = 20;
    public int scatterModeTimer2 = 7;
    public int chaseModeTimer2 = 20;
    public int scatterModeTimer3 = 5;
    public int chaseModeTimer3 = 20;
    public int scatterModeTimer4 = 5;
    public int chaseModeTimer4 = 20;

    public int frightenedMode = 10;
    public int startBlinkingAt = 7;

    private int modeChangeIteration = 1;
    private float modeChangeTimer = 0;

    private float frightenedModeTimer = 0;
    private float blinkTimer = 0;

    private bool frightenedModeIsWhite = false;

    public enum Mode {
        Chase,
        Scatter,
        Frightened,
        Consumed
    }

    public enum GhostType {
        Red,
        Pink,
        Orange,
        Blue
    }

    public GhostType ghostType = GhostType.Red;

    Mode currentMode = Mode.Scatter;
    Mode previousMode;

    private GameObject pacMan;

    private Node currentNode, targetNode, previousNode;
    private Vector2 direction, nextDirection;


    //changing sprite
    public SpriteRenderer spriteRenderer;
    public Sprite whiteGhostSprite;
    public Sprite frightenedGhostSprite;
    public Sprite normalGhostSprite;
    public Sprite eyesSprite;

    // Start is called before the first frame update
    void Start() {
        pacMan = GameObject.FindGameObjectWithTag("PacMan");
        Node node = GetNodeAtPosition(transform.localPosition);

        if(node != null){
            currentNode = node;
        }

        if(isInGhostHouse){
            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];
        } else {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }

        //direction = Vector2.left;

        previousNode = currentNode;


    }

    public void MoveToStartingPosition(){

        transform.position = startingPosition.transform.position;
        
        if(transform.name != "ghost_1"){
            isInGhostHouse = true;
        }

        if(isInGhostHouse){
            direction = Vector2.up;
        } else {
            direction = Vector2.left;
        }

        UpdateSprite();

    }

    public void Restart(){
        canMove = true;


        currentMode = Mode.Scatter;

        speed = normalSpeed;

        previousMoveSpeed = 0;

        transform.position = startingPosition.transform.position;

        ghostReleaseTimer = 0;
        modeChangeTimer = 0;
        modeChangeIteration = 1;

        

        if(transform.name != "ghost_1"){
            isInGhostHouse = true;
        }
        currentNode = startingPosition;

        if(isInGhostHouse){
            direction = Vector2.up;
            targetNode = currentNode.neighbors[0];
        } else {
            direction = Vector2.left;
            targetNode = ChooseNextNode();
        }

        
        previousNode = currentNode;
        UpdateSprite();
        
    }


    // Update is called once per frame
    void Update() {

        if(canMove){
            ModeUpdate();

            Move();
            
            ReleaseGhost();

            DetectCollision();

            CheckIsInGhostHouse();
        }
        

    }

    void CheckIsInGhostHouse(){

        if(currentMode == Mode.Consumed){

            GameObject tile = GetTileAtPosition(transform.position);

            if(tile != null){
                if(tile.transform.GetComponent<Tile>() != null){
                    if(tile.transform.GetComponent<Tile>().isGhostHouse){
                        speed = normalSpeed;

                        Node node = GetNodeAtPosition(transform.position);

                        if(node != null){
                            currentNode = node;

                            direction = Vector2.up;
                            targetNode = currentNode.neighbors[0];

                            previousNode = currentNode;

                            currentMode = Mode.Chase;

                            ModeUpdate();
                        }
                    }
                }
            }
        }
    }

    void Move(){
        if(targetNode != currentNode && targetNode != null && !isInGhostHouse){
            if(OverShotTarget()){
                currentNode = targetNode;
                transform.localPosition = currentNode.transform.position;

                GameObject otherPortal = GetPortal(currentNode.transform.position);

                if(otherPortal != null){
                    transform.localPosition = otherPortal.transform.localPosition;

                    currentNode = otherPortal.GetComponent<Node>();
                }

                targetNode = ChooseNextNode();
                previousNode = currentNode;
                currentNode = null;

            } else {
                transform.localPosition += (Vector3)(direction * speed) * Time.deltaTime;
            }
        }
	}

    void ModeUpdate(){
        UpdateSprite();
        if(currentMode != Mode.Frightened){
            modeChangeTimer += Time.deltaTime;

            if(modeChangeIteration == 1){
                if(currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer1){
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if(currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer1){
                    modeChangeIteration = 2;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            } else if(modeChangeIteration == 2){
                if(currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer2){
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if(currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer2){
                    modeChangeIteration = 3;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }
            } else if(modeChangeIteration == 3){
                if(currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer3){
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
                if(currentMode == Mode.Chase && modeChangeTimer > chaseModeTimer3){
                    modeChangeIteration = 4;
                    ChangeMode(Mode.Scatter);
                    modeChangeTimer = 0;
                }

            } else if(modeChangeIteration == 4){
                if(currentMode == Mode.Scatter && modeChangeTimer > scatterModeTimer4){
                    ChangeMode(Mode.Chase);
                    modeChangeTimer = 0;
                }
            }
        } else if(currentMode == Mode.Frightened){
            frightenedModeTimer += Time.deltaTime;

            if(frightenedModeTimer >= frightenedMode) {
                frightenedModeTimer = 0;
                spriteRenderer.sprite = normalGhostSprite;
                ChangeMode(previousMode);
            }

            if(frightenedModeTimer >= startBlinkingAt){
                blinkTimer += Time.deltaTime;
                if(blinkTimer >= 0.1f){
                    blinkTimer = 0f;
                    if(frightenedModeIsWhite){
                        spriteRenderer.sprite = frightenedGhostSprite;
                        frightenedModeIsWhite = false;
                    } else {
                        spriteRenderer.sprite = whiteGhostSprite;
                        frightenedModeIsWhite = true;
                    }
                }
            }
        }
    }

    void UpdateSprite(){
        if(currentMode != Mode.Frightened && currentMode != Mode.Consumed){
            spriteRenderer.sprite = normalGhostSprite;
            speed = normalSpeed;
        } else if(currentMode == Mode.Frightened){
            spriteRenderer.sprite = frightenedGhostSprite;
            speed = frightenedMoveSpeed;
        } else if(currentMode == Mode.Consumed){
            spriteRenderer.sprite = eyesSprite;
        }

    }

    void DetectCollision(){
        Rect ghostRect = new Rect(transform.position, transform.GetComponent<SpriteRenderer>().sprite.bounds.size/4);
        Rect pacmanRect = new Rect(pacMan.transform.position, pacMan.transform.GetComponent<SpriteRenderer>().sprite.bounds.size/4);
        
        if(ghostRect.Overlaps(pacmanRect)){
            if(currentMode == Mode.Frightened){
                Consumed();
            } else if(currentMode == Mode.Scatter || currentMode == Mode.Chase){
                GameObject.Find("Game").transform.GetComponent<GameBoard>().StartDeath();
            }
        }
        
    }

    void ChangeMode(Mode m){

        if(currentMode != m){
            previousMode = currentMode;
            currentMode = m;
        }        
        

        UpdateSprite();

    }

    public void StartFrightenedMode(){

        if(currentMode != Mode.Consumed){
            frightenedModeTimer = 0;
            ChangeMode(Mode.Frightened);
        }
        
    }

    void Consumed(){

        GameBoard.score += 200;

        currentMode = Mode.Consumed;
        previousMoveSpeed = speed;
        speed = consumedMoveSpeed;

        UpdateSprite();

        GameObject.Find("Game").transform.GetComponent<GameBoard>().StartConsumed(this.GetComponent<Ghost>());
    }

    Vector2 GetRedGhostTargetTile(){

        Vector2 pacManPosition = pacMan.transform.position;
        Vector2 targetTile = new Vector2(Mathf.RoundToInt(pacManPosition.x), Mathf.RoundToInt(pacManPosition.y));

        return targetTile;

    }

    Vector2 GetPinkGhostTargetTile(){

        Vector2 pacManPosition = pacMan.transform.position;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;

        int pacManPosX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPosY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPosX, pacManPosY);
        Vector2 targetTile = pacManTile + (4 * pacManOrientation);

        return targetTile;
        
    }

    Vector2 GetBlueGhostTargetTile(){
        Vector2 pacManPosition = pacMan.transform.position;
        Vector2 pacManOrientation = pacMan.GetComponent<PacMan>().orientation;

        int pacManPosX = Mathf.RoundToInt(pacManPosition.x);
        int pacManPosY = Mathf.RoundToInt(pacManPosition.y);

        Vector2 pacManTile = new Vector2(pacManPosX, pacManPosY);
        Vector2 targetTile = pacManTile + (2 * pacManOrientation);

        Vector2 tempRedGhostPos = GameObject.Find("Ghost").transform.localPosition;

        int redPosX = Mathf.RoundToInt(tempRedGhostPos.x);
        int redPosY = Mathf.RoundToInt(tempRedGhostPos.y);

        tempRedGhostPos = new Vector2(redPosX, redPosY);

        float distance = GetDistance(tempRedGhostPos, targetTile);

        distance *= 2;

        targetTile = new Vector2(tempRedGhostPos.x + distance, tempRedGhostPos.y + distance);

        return targetTile;

    }

    Vector2 GetOrangeGhostTargetTile(){
        Vector2 pacManPosition = pacMan.transform.position;
        
        int pacmanPosx = Mathf.RoundToInt(pacManPosition.x);
        int pacmanPosy = Mathf.RoundToInt(pacManPosition.y);

        float distance = GetDistance(transform.localPosition, pacManPosition);

        Vector2 targetTile = Vector2.zero;

        if(distance > 8){
            targetTile = new Vector2(pacmanPosx,pacmanPosy);
        } else if(distance < 8){
            targetTile = homeNode.transform.position;
        }

        return targetTile;
    }

    Vector2 GetTargetTile(){
        Vector2 targetTile = Vector2.zero;

        if(ghostType == GhostType.Red){
            targetTile = GetRedGhostTargetTile();
        } else if(ghostType == GhostType.Pink){
            targetTile = GetPinkGhostTargetTile();
        }

        return targetTile;

    }

    Vector2 GetRandomTile(){

        int x = Random.Range (0, 28);
        int y = Random.Range (0, 36);

        return new Vector2(x,y);
    }

    void ReleasePinkGhost(){
        if(ghostType == GhostType.Pink && isInGhostHouse){
            isInGhostHouse = false;
        }
    }

    void ReleaseBlueGhost(){
        if(ghostType == GhostType.Blue && isInGhostHouse){
            isInGhostHouse = false;
        }
    }

    void ReleaseOrangeGhost(){
        if(ghostType == GhostType.Orange && isInGhostHouse){
            isInGhostHouse = false;
        }
    }

    void ReleaseGhost(){
        ghostReleaseTimer += Time.deltaTime;

        if(ghostReleaseTimer > pinkyReleaseTimer){
            ReleasePinkGhost();
        }
        if(ghostReleaseTimer > blueyReleaseTimer){
            ReleaseBlueGhost();
        }
        if(ghostReleaseTimer > orangeyReleaseTimer){
            ReleaseOrangeGhost();
        }
    }

    Node ChooseNextNode(){
        Vector2 targetTile = Vector2.zero;

        if(currentMode == Mode.Chase){
            targetTile = GetTargetTile();
        } else if (currentMode == Mode.Scatter){
            targetTile = homeNode.transform.position;
        } else if (currentMode == Mode.Frightened){
            targetTile = GetRandomTile();
        } else if (currentMode == Mode.Consumed){
            targetTile = ghostHome.transform.position;
        }
        

        Node moveToNode = null;
        Node[] foundNodes = new Node[4];
        Vector2[] foundNodesDirection = new Vector2[4];

        int nodeCounter = 0;

        for(int i = 0; i < currentNode.neighbors.Length; i++){

            if(currentNode.validDirections[i] != direction * -1){

                if(currentMode != Mode.Consumed){

                    GameObject tile = GetTileAtPosition(currentNode.transform.position);
                    if(tile.transform.GetComponent<Tile>().isGhostHouseEntrance == true){
                        //found ghost house
                        if(currentNode.validDirections[i] != Vector2.down){
                            foundNodes[nodeCounter] = currentNode.neighbors[i];
                            foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                            nodeCounter++;
                        }
                    } else {
                        foundNodes[nodeCounter] = currentNode.neighbors[i];
                        foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                        nodeCounter++;
                    }
                } else {
                    foundNodes[nodeCounter] = currentNode.neighbors[i];
                    foundNodesDirection[nodeCounter] = currentNode.validDirections[i];
                    nodeCounter++;
                }
                
                
            }
        }

        if(foundNodes.Length == 1){
            moveToNode = foundNodes[0];
            direction = foundNodesDirection[0];
        }

        if(foundNodes.Length > 1){

            float leastDistance = 1000000f;
            for(int i = 0; i<foundNodes.Length; i++){
                
                if(foundNodesDirection[i] != Vector2.zero){
                    float distance = GetDistance(foundNodes[i].transform.position, targetTile);

                    if(distance < leastDistance){
                        leastDistance = distance;
                        moveToNode = foundNodes[i];
                        direction = foundNodesDirection[i];
                    }
                }
            }
        }

        return moveToNode;
    }

    Node GetNodeAtPosition(Vector2 pos){
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board [(int)pos.x, (int)pos.y];

        if(tile != null){
            if(tile.GetComponent<Node>() != null){
                return tile.GetComponent<Node>();
            }
        }

        return null;
    }

    GameObject GetTileAtPosition(Vector2 pos){

        int tileX = Mathf.RoundToInt(pos.x);
        int tileY = Mathf.RoundToInt(pos.y);

        GameObject tile = GameObject.Find("Game").transform.GetComponent<GameBoard>().board[tileX,tileY];

        if(tile != null){
            return tile;
        }

        return null;
    }

    GameObject GetPortal (Vector2 pos){
        GameObject tile = GameObject.Find("Game").GetComponent<GameBoard>().board [(int)pos.x, (int)pos.y];

        if(tile != null){
            if(tile.GetComponent<Tile>().isPortal){
                GameObject otherPortal = tile.GetComponent<Tile>().portalReceiver;
                return otherPortal;
            }
        }
        return null;
    }

    bool OverShotTarget(){
        float nodeToTarget = LengthFromNode (targetNode.transform.position);
        float nodeToSelf = LengthFromNode (transform.localPosition);

        return nodeToSelf > nodeToTarget;
    }

    float LengthFromNode(Vector2 targetPosition){
        Vector2 vec = targetPosition - (Vector2)previousNode.transform.position;
        return vec.sqrMagnitude;
    }

    float GetDistance(Vector2 posA, Vector2 posB){
        float dx = posA.x - posB.x;
        float dy = posA.y - posB.y;

        float distance = Mathf.Sqrt(dx*dx+dy*dy);

        return distance;
    }
}
