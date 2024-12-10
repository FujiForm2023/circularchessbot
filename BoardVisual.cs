using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Collections.Generic;

public class BoardVisual : MonoBehaviour
{
    [Min(0.01f)]
    public float outerRadius = 6f;
    [Min(0)]
    public float innerRadius = 1f;
    private int rankCount = 8;
    private int fileCount = 20;
    [Min(1)]
    public int detailLevel = 1;

    public bool circularMode = true;
    public Color boardWhiteColor = new Color(0.92f, 0.92f, 0.81f, 1f);
    public Color boardBlackColor = new Color(0.45f, 0.58f, 0.32f, 1f);
    public GameObject[] prefabs = new GameObject[15];
    public bool flipX = false;
    public bool flipY = false;
    public GameObject[] tileObjects = new GameObject[160];
    public byte[][] movedTile = new byte[2][];
    public byte[] selectedTile = new byte[2]{255,255};
    public byte movementType = 0;
    public byte[] promoteAt = new byte[2]{255,255};
    public GameObject gameStatusCanvas;
    public GameObject gameStatusText;
    public BoardBot boardBot;

    void Start()
    {
        boardBot = GetComponent<BoardBot>();
        boardBot.CircularChessLoader("VVVVVVVV/8/8/2K2Nr1/1QR3r1/5k2/5b2/4r3/8/q1B5/6N1/BbN1n3/8/3B1Nbn/nB1b1B2/3n1Q2/b2n4/8/8/VVVVVVVV w - 0 1");
        DrawBoard();

    }

    void DrawBoard()
    {
        if (rankCount < 1 || fileCount < 1 || detailLevel < 1 || innerRadius >= outerRadius || outerRadius < 0.01 || innerRadius < 0)
        {
            return;
        }

        if (circularMode)
        {
            CreateCircularBoard();
        }
        else
        {
            CreateRectangularBoard();
        }
        if (selectedTile[0] != 255 && selectedTile[1] != 255){
            CallTile(selectedTile[0],selectedTile[1]).GetComponent<Tile>().UponChangeMode();
        }
        if (movedTile[0] != null){
           CallTile(movedTile[0][0],movedTile[0][1]).GetComponent<Tile>().ActivateHighlightTile();
           CallTile(movedTile[1][0],movedTile[1][1]).GetComponent<Tile>().ActivateHighlightTile();
        }

    }
    void CreateCircularBoard(){

        float angleStep = Mathf.PI / fileCount * 2;
        float radiusStep = (outerRadius - innerRadius) / rankCount;
        float detailStep = angleStep / detailLevel;

        // MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        // MeshRenderer meshRenderer = this.GetComponent<MeshRenderer>();

        // Mesh mesh = new Mesh();
        // meshFilter.mesh = mesh;

        // int vertexIndex = 0;
        float ANGLEADJUST = Mathf.PI / fileCount * 17;
        float angle;
        // // int[] triangles = new int[detailPerSegment * 6];

        tileObjects = new GameObject[fileCount * rankCount];

        for (byte i = 0; i < fileCount; ++i)
        {
            // // Test
            // if (i > 1)
            // {
            //     break;
            // }

            for (byte j = 0; j < rankCount; ++j)
            {
                // // Test
                // if (j > 2)
                // {
                //     break;
                // }

                if (boardBot.isVoid(boardBot.getPiece(boardBot.currentPosition, j, i)))
                {
                    tileObjects[i * rankCount + j] = null;
                    continue;
                }

                angle = angleStep * i - ANGLEADJUST;

                GameObject newObject = new GameObject("Tile R" + (rankCount-j-1) + " C" + i, typeof(MeshRenderer), typeof(MeshFilter));
                newObject.transform.parent = this.transform;

                newObject.transform.position = new Vector3(Mathf.Cos(angle) * (innerRadius + radiusStep * j), Mathf.Sin(angle) * (innerRadius + radiusStep * j), 0);

                tileObjects[i * rankCount + j] = newObject;

                Mesh mesh = new Mesh();
                newObject.GetComponent<MeshFilter>().mesh = mesh;

                Vector3[] vertices = new Vector3[detailLevel * 2 + 2];
                int[] triangles = new int[detailLevel * 6];

                for (int k = 0; k <= detailLevel; ++k)
                {
                    vertices[k * 2] = new Vector3(
                        Mathf.Cos(angle - angleStep * 0.5f + detailStep * k) * (innerRadius + radiusStep * (j-0.5f)),
                        Mathf.Sin(angle - angleStep * 0.5f + detailStep * k) * (innerRadius + radiusStep * (j-0.5f)), 0) - newObject.transform.position;

                    vertices[k * 2 + 1] = new Vector3(
                        Mathf.Cos(angle - angleStep * 0.5f + detailStep * k) * (innerRadius + radiusStep * (j+0.5f)),
                        Mathf.Sin(angle - angleStep * 0.5f + detailStep * k) * (innerRadius + radiusStep * (j+0.5f)), 0) - newObject.transform.position;
                }

                for (int k = 0; k < detailLevel; ++k)
                {
                    triangles[k * 6] = k * 2;
                    triangles[k * 6 + 1] = k * 2 + 1;
                    triangles[k * 6 + 2] = k * 2 + 2;
                    triangles[k * 6 + 3] = k * 2 + 2;
                    triangles[k * 6 + 4] = k * 2 + 1;
                    triangles[k * 6 + 5] = k * 2 + 3;
                }

                mesh.vertices = vertices;
                mesh.triangles = triangles;

                Material material = new Material(Shader.Find("Sprites/Default"));
                Color color = ((i+j) % 2 == 0) ? boardWhiteColor : boardBlackColor;
                material.color = color;
                newObject.GetComponent<MeshRenderer>().material = material;

                mesh.RecalculateNormals();

                // Add tile script
                Tile tile = newObject.AddComponent<Tile>();
                tile.rank = j;
                tile.file = i;

                // Test
                // if (i != 0 || j != 0)
                // {
                //     continue;
                // }

                // Add collider
                PolygonCollider2D polygonCollider = newObject.AddComponent<PolygonCollider2D>();

                Vector2[] path = new Vector2[detailLevel * 2 + 2];
                for (int k = 0; k <= detailLevel; ++k)
                {
                    path[k] = new Vector2(
                        Mathf.Cos(angle - angleStep * 0.5f + detailStep * k) * (innerRadius + radiusStep * (j-0.5f)) - newObject.transform.position.x,
                        Mathf.Sin(angle - angleStep * 0.5f + detailStep * k) * (innerRadius + radiusStep * (j-0.5f)) - newObject.transform.position.y);
                }

                for (int k = 0; k <= detailLevel; ++k)
                {
                    path[detailLevel + k + 1] = new Vector2(
                        Mathf.Cos(angle - angleStep * 0.5f + detailStep * (detailLevel-k)) * (innerRadius + radiusStep * (j+0.5f)) - newObject.transform.position.x,
                        Mathf.Sin(angle - angleStep * 0.5f + detailStep * (detailLevel-k)) * (innerRadius + radiusStep * (j+0.5f)) - newObject.transform.position.y);
                }

                polygonCollider.SetPath(0, path);

                // Draw piece
                byte piece = boardBot.getPiece(boardBot.currentPosition,j, i);
                if (boardBot.isVoid(piece) || boardBot.isBlank(piece))
                {
                    continue;
                }

                switch (piece)
                {
                    case 1:
                        piece = 0;
                        break;
                    case 2:
                        piece = 1;
                        break;
                    case 3:
                        piece = 2;
                        break;
                    case 4:
                        piece = 3;
                        break;
                    case 5:
                        piece = 4;
                        break;
                    case 6:
                        piece = 5;
                        break;
                    case 9:
                        piece = 6;
                        break;
                    case 10:
                        piece = 7;
                        break;
                    case 11:
                        piece = 8;
                        break;
                    case 12:
                        piece = 9;
                        break;
                    case 13:
                        piece = 10;
                        break;
                    case 14:
                        piece = 11;
                        break;
                    default:
                        continue;
                }
                
                GameObject pieceObject = Instantiate(prefabs[piece], newObject.transform);
                pieceObject.transform.position = newObject.transform.position;

                // Set piece
                tile.setPiece(pieceObject);
            }
        }

        // mesh.vertices = vertices;
        // mesh.triangles = triangles;


        // Material material = new Material(Shader.Find("Sprites/Default"));
        // Color color = (layerIndex % 2 == 0) ? Color.white : Color.black;
        // material.color = color;
        // meshRenderer.material = material;

        // mesh.RecalculateNormals();
    }

    void CreateRectangularBoard(){
                
        float width = outerRadius * 2;
        float height = outerRadius * 2;
        float rankHeight = height / rankCount;
        float fileWidth = width / fileCount;
        float minSize = Mathf.Max(rankHeight, fileWidth) * 0.75f;

        tileObjects = new GameObject[fileCount * rankCount];

        for (byte i = 0; i < fileCount; ++i)
        {
            for (byte j = 0; j < rankCount; ++j)
            {
                // Ignore ghost tiles
                if (boardBot.isVoid(boardBot.getPiece(boardBot.currentPosition, j, i)))
                {
                    tileObjects[i * rankCount + j] = null;
                    continue;
                }

                // Create tile
                GameObject newObject = new GameObject("Tile R" + (rankCount-j-1) + " C" + i, typeof(MeshRenderer), typeof(MeshFilter));
                newObject.transform.parent = this.transform;

                // Move
                newObject.transform.position = new Vector3(minSize * i - width / 2 + minSize / 2 - 4.25f, minSize * (7-j) - height / 2 + minSize / 2 + 1.25f, 0);

                tileObjects[i * rankCount + j] = newObject;

                // Create mesh
                Mesh mesh = new Mesh();
                newObject.GetComponent<MeshFilter>().mesh = mesh;

                Vector3[] vertices = new Vector3[4];
                int[] triangles = new int[6];

                vertices[0] = new Vector3(-minSize / 2, -minSize / 2, 0);
                vertices[1] = new Vector3(minSize / 2, -minSize / 2, 0);
                vertices[2] = new Vector3(-minSize / 2, minSize / 2, 0);
                vertices[3] = new Vector3(minSize / 2, minSize / 2, 0);

                triangles[0] = 0;
                triangles[1] = 2;
                triangles[2] = 1;
                triangles[3] = 2;
                triangles[4] = 3;
                triangles[5] = 1;

                mesh.vertices = vertices;
                mesh.triangles = triangles;

                Material material = new Material(Shader.Find("Sprites/Default"));
                Color color = ((i+j) % 2 == 0) ? boardWhiteColor : boardBlackColor;
                material.color = color;
                newObject.GetComponent<MeshRenderer>().material = material;

                mesh.RecalculateNormals();
                
                // Add tile script
                Tile tile = newObject.AddComponent<Tile>();
                tile.rank = j;
                tile.file = i;

                // Add collider
                PolygonCollider2D polygonCollider = newObject.AddComponent<PolygonCollider2D>();

                Vector2[] path = new Vector2[4];
                path[0] = new Vector2(-minSize / 2, -minSize / 2);
                path[1] = new Vector2(minSize / 2, -minSize / 2);
                path[2] = new Vector2(minSize / 2, minSize / 2);
                path[3] = new Vector2(-minSize / 2, minSize / 2);

                polygonCollider.SetPath(0, path);

                // Draw piece
                byte piece = boardBot.getPiece(boardBot.currentPosition, j, i);
                if (boardBot.isVoid(piece) || boardBot.isBlank(piece))
                {
                    continue;
                }

                piece--;
                if (piece > 5){
                    piece -= 2;
                }
                GameObject pieceObject = Instantiate(prefabs[piece], newObject.transform);
                pieceObject.transform.position = new Vector3(minSize * i - width / 2 + minSize / 2 - 4.25f, minSize * (7-j) - height / 2 + minSize / 2 + 1.25f, 0);
                pieceObject.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
                
                // Set piece
                tile.setPiece(pieceObject);
            }
        }
    }

    public void ResetChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetMode(bool mode)
    {
        if (circularMode == mode)
        {
            return;
        }

        circularMode = mode;
        ResetChildren();
        DrawBoard();
    }

    public void SetOuterRadius(float radius)
    {
        outerRadius = radius;
        ResetChildren();
        DrawBoard();
    }

    public void SetInnerRadius(float radius)
    {
        innerRadius = radius;
        ResetChildren();
        DrawBoard();
    }

    public void SwitchMode()
    {
        circularMode = !circularMode;
        ResetChildren();
        DrawBoard();
    }

    public void SetDetailLevel(int level)
    {
        detailLevel = level;
        ResetChildren();
        DrawBoard();
    }

    public void SetBoardWhiteColor(Color color)
    {
        boardWhiteColor = color;
        ResetChildren();
        DrawBoard();
    }

    public void SetBoardBlackColor(Color color)
    {
        boardBlackColor = color;
        ResetChildren();
        DrawBoard();
    }
    public GameObject CallTile(byte rank, byte file){
        return tileObjects[file * rankCount + rank];
    }
    public GameObject CallTile(int rank, int file){
        return tileObjects[file * rankCount + rank];
    }
    public void HigherDetail(){
        ++detailLevel;
        ResetChildren();
        DrawBoard();
    }
    public void LowerDetail(){
        if (detailLevel == 1){
            return;
        }
        --detailLevel;
        ResetChildren();
        DrawBoard();
    }

    public byte PieceToPrefab(byte piece){
        if (piece <= 6){
            --piece;
        } else {
            piece -= 3;
        }
        return piece;
    }

    public void PromotePawn(byte piece){
        // Data
        CallTile(movedTile[0][0], movedTile[0][1]).GetComponent<Tile>().DeactivateHighlightTile();
        CallTile(movedTile[1][0], movedTile[1][1]).GetComponent<Tile>().DeactivateHighlightTile();

        boardBot.pawnPromotion(selectedTile[0], selectedTile[1], promoteAt[0], promoteAt[1], piece);

        // Visual
        Tile pawnTile = CallTile(selectedTile[0],selectedTile[1]).GetComponent<Tile>();
        pawnTile.UponPromote();
        pawnTile.setPiece(null);
        Tile promoteTile = CallTile(promoteAt[0],promoteAt[1]).GetComponent<Tile>();
        GameObject pieceObject = Instantiate(prefabs[PieceToPrefab(piece)], promoteTile.transform);
        if (!circularMode){
            pieceObject.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
        }
        pieceObject.transform.position = promoteTile.transform.position;
        promoteTile.UponPromote();
        promoteTile.setPiece(pieceObject);

        pawnTile.ActivateHighlightTile();
        promoteTile.ActivateHighlightTile();

        movedTile[0] = selectedTile.Clone() as byte[];
        movedTile[1] = promoteAt.Clone() as byte[];
        
        selectedTile[0] = 255;
        selectedTile[1] = 255;

        promoteAt[0] = 255;
        promoteAt[1] = 255;
    }

    public void PromotePawnBot(byte rankFrom, byte fileFrom, byte rankTo, byte fileTo, byte piece){
        // Data
        CallTile(movedTile[0][0], movedTile[0][1]).GetComponent<Tile>().DeactivateHighlightTile();
        CallTile(movedTile[1][0], movedTile[1][1]).GetComponent<Tile>().DeactivateHighlightTile();

        boardBot.pawnPromotion(rankFrom, fileFrom, rankTo, fileTo, piece);

        // Visual
        Tile pawnTile = CallTile(rankFrom, fileFrom).GetComponent<Tile>();
        pawnTile.UponPromote();
        pawnTile.setPiece(null);
        Tile promoteTile = CallTile(rankTo, fileTo).GetComponent<Tile>();
        GameObject pieceObject = Instantiate(prefabs[PieceToPrefab(piece)], promoteTile.transform);
        if (!circularMode){
            pieceObject.transform.localScale = new Vector3(1.75f, 1.75f, 1.75f);
        }
        pieceObject.transform.position = promoteTile.transform.position;
        promoteTile.UponPromote();
        promoteTile.setPiece(pieceObject);

        pawnTile.ActivateHighlightTile();
        promoteTile.ActivateHighlightTile();

        movedTile[0] = new byte[]{rankFrom, fileFrom};
        movedTile[1] = new byte[]{rankTo, fileTo};
        
        selectedTile[0] = 255;
        selectedTile[1] = 255;

        promoteAt[0] = 255;
        promoteAt[1] = 255;
    }

    public void GameOver(){
        gameStatusCanvas.SetActive(true);
        gameStatusText.GetComponent<Text>().text = boardBot.currentPosition.gameStatus == 1 ? "White wins!" : boardBot.currentPosition.gameStatus == 2 ? "Black wins!" : "Draw!";
    }

    public void MakeMove(byte rank1, byte file1, byte rank2, byte file2){
        Tile tile1 = CallTile(rank1,file1).GetComponent<Tile>();
        Tile tile2 = CallTile(rank2,file2).GetComponent<Tile>();
        tile1.TileMovePieceVisual(tile2);
    }
}
