using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PromoteButton : MonoBehaviour, IPointerClickHandler
{
    public BoardVisual boardVisual;
    public byte piece = 0;
    void Awake()
    {
        boardVisual = GameObject.FindObjectOfType<BoardVisual>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        boardVisual.PromotePawn(piece);
        Destroy(this.transform.parent);
        eventData.Use();
    }
}
