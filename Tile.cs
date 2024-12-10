using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, IDragHandler, IBeginDragHandler
{
    public byte rank;
    public byte file;
    public GameObject? piece;
    private GameObject? pawnPromoteUI;
    private Camera camera;
    private BoardBot boardBot;
    private MeshRenderer meshRenderer;
    private Material material;
    private Color color;
    private bool AAA = false;
    private bool BBB = false;
    private GameObject highlightMove;
    private GameObject activeHighlight;
    private BoardVisual boardVisual;
    private byte[][] movements = new byte[0][];

    public void Awake(){
        camera = Camera.main;
        boardBot = transform.parent.GetComponent<BoardBot>();
        boardVisual = transform.parent.GetComponent<BoardVisual>();
        highlightMove = boardVisual.prefabs[12];
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        color = material.color;
    }

    public void ManualStart(){
        camera = Camera.main;
        boardBot = transform.parent.GetComponent<BoardBot>();
        boardVisual = transform.parent.GetComponent<BoardVisual>();
        highlightMove = boardVisual.prefabs[12];
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        color = material.color;
    }
    
    public void setPiece(GameObject piece){
        if (this.piece != null){
            Destroy(this.piece);
        }
        this.piece = piece;
    }

    public void OnBeginDrag(PointerEventData eventData){
        eventData.Use();
    }
    public void OnDrag(PointerEventData eventData){
        if (piece != null){
            piece.transform.position = GetMouseWorldPosition();
        }
        eventData.Use();
    }

    private Vector3 GetMouseWorldPosition(){
        Vector3 vec = Input.mousePosition;
        vec.z = camera.nearClipPlane;
        return camera.ScreenToWorldPoint(vec);
    }

    public void OnPointerClick(PointerEventData eventData){
        if (piece != null && (boardBot.isWhite(boardBot.getPiece(rank, file)) == boardBot.currentPosition.whiteToMove)){
            if (boardVisual.selectedTile[0] != 255 && boardVisual.selectedTile[1] != 255){
                
            }
            piece.transform.position = transform.position;
            if (AAA){
                if (!BBB){
                    BBB = true;
                    boardVisual.selectedTile = new byte[]{rank, file};
                } else {
                    BBB = false;
                    AAA = false;
                    material.color = color;
                    DeleteMovements();
                }
            }
        }
        eventData.Use();
    }

    public void OnPointerDown(PointerEventData eventData){
        if (pawnPromoteUI != null){
            return;
        }
        if (boardVisual.selectedTile[0] != 255 && boardVisual.selectedTile[1] != 255){
            if ((boardVisual.selectedTile[0] != rank) || (boardVisual.selectedTile[1] != file)){
                if (boardBot.isAttackable(boardBot.getPiece(boardBot.currentPosition, boardVisual.selectedTile[0], boardVisual.selectedTile[1]), boardBot.getPiece(boardBot.currentPosition, rank, file))){
                    if (activeHighlight != null){
                        
                        // Get tile properties
                        Tile startTileProperties = boardVisual.CallTile(boardVisual.selectedTile[0], boardVisual.selectedTile[1]).GetComponent<Tile>();

                        // Interrupted promotion
                        // If is a pawn
                        if (rank == 0){
                            if (boardBot.getPiece(boardBot.currentPosition, boardVisual.selectedTile[0], boardVisual.selectedTile[1]) == 1){
                                boardVisual.promoteAt = new byte[]{rank, file};
                                pawnPromoteUI = Instantiate(boardVisual.prefabs[13], transform.position, Quaternion.identity);
                                pawnPromoteUI.transform.SetParent(transform);
                                pawnPromoteUI.transform.position = new Vector3(pawnPromoteUI.transform.position.x, pawnPromoteUI.transform.position.y, -0.1f);
                                return;
                            }
                            else if (boardBot.getPiece(boardBot.currentPosition, boardVisual.selectedTile[0], boardVisual.selectedTile[1]) == 9){
                                boardVisual.promoteAt = new byte[]{rank, file};
                                pawnPromoteUI = Instantiate(boardVisual.prefabs[14], transform.position, Quaternion.identity);
                                pawnPromoteUI.transform.SetParent(transform);
                                pawnPromoteUI.transform.position = new Vector3(pawnPromoteUI.transform.position.x, pawnPromoteUI.transform.position.y, -0.1f);
                                return;
                            }
                        }

                        // Move piece (Visual)
                        startTileProperties.piece.transform.position = transform.position;
                        startTileProperties.piece.transform.SetParent(transform);
                        setPiece(startTileProperties.piece);
                        startTileProperties.piece = null;

                        // Delete movements
                        startTileProperties.DeleteMovements();

                        // Move piece
                        startTileProperties.TileMovePiece(this);

                        // Deselect tile
                        boardVisual.selectedTile = new byte[]{255, 255};
                        eventData.Use();
                        return;
                    }
                    else if (boardBot.isFriendly(boardBot.getPiece(boardBot.currentPosition, rank, file), boardBot.getPiece(boardBot.currentPosition, boardVisual.selectedTile[0], boardVisual.selectedTile[1]))){
                        boardVisual.CallTile(boardVisual.selectedTile[0], boardVisual.selectedTile[1]).GetComponent<Tile>().DeselectTile();
                    }
                }
            }
        }

        if (piece != null){
            piece.transform.position = GetMouseWorldPosition();
            if (boardBot.isWhite(boardBot.getPiece(rank, file)) == boardBot.currentPosition.whiteToMove){
                if (!AAA){
                    material.color = new Color(0.96f, 0.96f, 0.51f, 1.0f);
                    AAA = true;
                }
                if (movements.Length == 0){
                    if (boardVisual.selectedTile[0] != 255 && boardVisual.selectedTile[1] != 255){
                        boardVisual.CallTile(boardVisual.selectedTile[0], boardVisual.selectedTile[1]).GetComponent<Tile>().DeactivateHighlightTile();
                        boardVisual.CallTile(boardVisual.selectedTile[0], boardVisual.selectedTile[1]).GetComponent<Tile>().DeleteMovements();
                    }
                    movements = boardBot.deconstructMoves(boardBot.getMovement(boardBot.currentPosition, rank, file));
                    foreach (byte[] movement in movements){
                        boardVisual.CallTile(movement[0], movement[1]).GetComponent<Tile>().ActivateHighlightMove();
                    }
                    boardVisual.selectedTile = new byte[]{rank, file};
                }
            }
        }
        eventData.Use();
    }

    public void OnPointerUp(PointerEventData eventData){
        if (pawnPromoteUI != null){
            return;
        }
        if (boardVisual.selectedTile[0] != 255 && boardVisual.selectedTile[1] != 255){
            if (boardVisual.selectedTile[0] == rank && boardVisual.selectedTile[1] == file){
            } else {
                if (piece != null){
                    piece.transform.position = transform.position;
                }
                return;
            }
        }
        RaycastHit2D hit = Physics2D.Raycast(GetMouseWorldPosition(), Vector2.zero);
        if (hit.collider != null){
            
            // Get tile properties
            GameObject placeTile = hit.collider.gameObject;
            Tile tileProperties = placeTile.GetComponent<Tile>();

            if (piece != null){
                if (tileProperties.activeHighlight != null){
                    
                    // Interrupted promotion
                    // If is a pawn
                    if (rank == 1){
                        if (boardBot.getPiece(boardBot.currentPosition, rank, file) == 1){
                            boardVisual.promoteAt = new byte[]{tileProperties.rank, tileProperties.file};
                            pawnPromoteUI = Instantiate(boardVisual.prefabs[13], placeTile.transform.position, Quaternion.identity);
                            pawnPromoteUI.transform.SetParent(placeTile.transform);
                            pawnPromoteUI.transform.position = new Vector3(pawnPromoteUI.transform.position.x, pawnPromoteUI.transform.position.y, -0.1f);
                            piece.transform.position = transform.position;
                            return;
                        }
                        else if (boardBot.getPiece(boardBot.currentPosition, rank, file) == 9){
                            boardVisual.promoteAt = new byte[]{tileProperties.rank, tileProperties.file};
                            pawnPromoteUI = Instantiate(boardVisual.prefabs[14], placeTile.transform.position, Quaternion.identity);
                            pawnPromoteUI.transform.SetParent(transform);
                            pawnPromoteUI.transform.position = new Vector3(pawnPromoteUI.transform.position.x, pawnPromoteUI.transform.position.y, -0.1f);
                            piece.transform.position = transform.position;
                            return;
                        }
                    }

                    // Move piece (Visual)
                    piece.transform.position = placeTile.transform.position;
                    piece.transform.SetParent(placeTile.transform);
                    tileProperties.setPiece(piece);
                    this.piece = null;

                    // Delete movements
                    DeleteMovements();

                    // Move piece
                    TileMovePiece(tileProperties);

                    // Deselect tile
                    boardVisual.selectedTile = new byte[]{255, 255};

                } else {
                    // Is the same tile. Back to original position
                    piece.transform.position = transform.position;
                    if (placeTile == this.gameObject){
                        boardVisual.selectedTile = new byte[]{255, 255};
                    } else {
                        if (!BBB){
                            BBB = true;
                        }
                    }

                }
            }
        } else {
            // Back to original position
            if (piece != null){
                piece.transform.position = transform.position;
                AAA = false;
                material.color = color;
                DeleteMovements();
                boardVisual.selectedTile = new byte[]{255, 255};
            }
        }
        eventData.Use();
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (!AAA){
            material.color = new Color(
            Mathf.Clamp01(color.r - 0.05f),
            Mathf.Clamp01(color.g - 0.05f),
            Mathf.Clamp01(color.b - 0.05f),
            color.a
            );
        }
        eventData.Use();
    }

    public void OnPointerExit(PointerEventData eventData){
        if (!AAA){
            material.color = color;
        }
        eventData.Use();
    }

    public void ActivateHighlightTile(){
        material.color = new Color(0.96f, 0.96f, 0.51f, 1.0f);
        AAA = true;
    }

    public void DeactivateHighlightTile(){
        material.color = color;
        AAA = false;
        BBB = false;
    }

    public void ActivateHighlightMove(){
        if (activeHighlight != null){
            return;
        }
        activeHighlight = Instantiate(highlightMove, transform.position, Quaternion.identity);
        activeHighlight.transform.SetParent(transform);
        activeHighlight.transform.position = new Vector3(transform.position.x, transform.position.y, -0.1f);
    }

    public void DeactivateHighlightMove(){
        if (activeHighlight != null){
            Destroy(activeHighlight.gameObject);
        }
        activeHighlight = null;
    }
    public void SelectTile(){
        ActivateHighlightTile();
        BBB = true;
        movements = boardBot.deconstructMoves(boardBot.getMovement(boardBot.currentPosition, rank, file));
        foreach (byte[] movement in movements){
            boardVisual.CallTile(movement[0], movement[1]).GetComponent<Tile>().ActivateHighlightMove();
        }
    }
    public void DeselectTile(){
        DeactivateHighlightTile();
        DeleteMovements();
    }
    public void DeleteMovements(){
        foreach (byte[] movement in movements){
            boardVisual.CallTile(movement[0], movement[1]).GetComponent<Tile>().DeactivateHighlightMove();
        }
        movements = new byte[0][];
    }

    public void TileMovePiece(Tile tileProperties){
        // Move piece (Visual)
        if (boardVisual.movedTile[0] != null){
            boardVisual.CallTile(boardVisual.movedTile[0][0], boardVisual.movedTile[0][1]).GetComponent<Tile>().DeactivateHighlightTile();
            boardVisual.CallTile(boardVisual.movedTile[1][0], boardVisual.movedTile[1][1]).GetComponent<Tile>().DeactivateHighlightTile();
        }

        // Activate highlight new moved tiles
        this.ActivateHighlightTile();
        tileProperties.ActivateHighlightTile();

        // Set new moved tiles
        boardVisual.movedTile[0] = new byte[]{rank, file};
        boardVisual.movedTile[1] = new byte[]{tileProperties.rank, tileProperties.file};
        
        // Move piece (Data)
        boardBot.movePiece(rank, file, tileProperties.rank, tileProperties.file);

        // Castling
        if (boardBot.currentPosition.isCastle){
            if (boardBot.getPiece(boardBot.currentPosition, tileProperties.rank, tileProperties.file) == 6){ // Is white king
                if (tileProperties.file == 6){
                    boardVisual.CallTile(7, 7).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 5));
                } else if (tileProperties.file == 2){
                    boardVisual.CallTile(7, 0).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 3));
                }
            } else if (boardBot.getPiece(boardBot.currentPosition, rank, tileProperties.file) == 14){ // Is black king
                if (tileProperties.file == 11){
                    boardVisual.CallTile(7, 10).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 12));
                } else if (tileProperties.file == 15){
                    boardVisual.CallTile(7, 17).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 14));
                }
            }
        }
        if (boardBot.currentPosition.gameStatus != 0){
            boardVisual.GameOver();
        }
    }

    public void TileMovePieceVisual(Tile tileProperties){
        
        // Move piece (Visual)
        if (boardVisual.movedTile[0] != null){
            boardVisual.CallTile(boardVisual.movedTile[0][0], boardVisual.movedTile[0][1]).GetComponent<Tile>().DeactivateHighlightTile();
            boardVisual.CallTile(boardVisual.movedTile[1][0], boardVisual.movedTile[1][1]).GetComponent<Tile>().DeactivateHighlightTile();
        }

        // Move the piece (Visual)
        this.piece.transform.position = tileProperties.transform.position;
        this.piece.transform.SetParent(tileProperties.transform);
        tileProperties.setPiece(this.piece);
        this.piece = null;

        // Activate highlight new moved tiles
        this.ActivateHighlightTile();
        tileProperties.ActivateHighlightTile();

        // Set new moved tiles
        boardVisual.movedTile[0] = new byte[]{rank, file};
        boardVisual.movedTile[1] = new byte[]{tileProperties.rank, tileProperties.file};

        // Castling
        if (boardBot.currentPosition.isCastle){
            if (boardBot.getPiece(boardBot.currentPosition, tileProperties.rank, tileProperties.file) == 6){ // Is white king
                if (tileProperties.file == 6){
                    boardVisual.CallTile(7, 7).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 5));
                } else if (tileProperties.file == 2){
                    boardVisual.CallTile(7, 0).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 3));
                }
            } else if (boardBot.getPiece(boardBot.currentPosition, rank, tileProperties.file) == 14){ // Is black king
                if (tileProperties.file == 11){
                    boardVisual.CallTile(7, 10).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 12));
                } else if (tileProperties.file == 15){
                    boardVisual.CallTile(7, 17).GetComponent<Tile>().UponCastle(boardVisual.CallTile(7, 14));
                }
            }
        }
        if (boardBot.currentPosition.gameStatus != 0){
            boardVisual.GameOver();
        }
    }

    public void UponCastle(GameObject tile){
        piece.transform.position = tile.transform.position;
        piece.transform.SetParent(tile.transform);
        tile.GetComponent<Tile>().setPiece(piece);
        this.piece = null;
    }

    public void UponChangeMode(){
        ManualStart();
        SelectTile();
    }
    
    public void UponPromote(){
        if (piece != null){
            Destroy(piece.gameObject);
        }
        if (pawnPromoteUI != null){
            Destroy(pawnPromoteUI.gameObject);
        }
        if (movements.Length != 0){
            DeleteMovements();
        }
    }
}
