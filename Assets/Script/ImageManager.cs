using UnityEngine;

public class ImageManager : MonoSingleton<ImageManager>
{
    [SerializeField]
    Sprite BlackCheck;
    [SerializeField]
    Sprite WhiteCheck;

    [SerializeField]
    Sprite BlackChess;
    [SerializeField]
    Sprite BlackChess_Highlight;
    [SerializeField]
    Sprite BlackChess_Selecting;
    [SerializeField]
    Sprite WhiteChess;
    [SerializeField]
    Sprite WhiteChess_Highlight;
    [SerializeField]
    Sprite WhiteChess_Selecting;

    public Sprite GetCheckImageBySide(bool side)
    {
        return (side) ? BlackCheck : WhiteCheck;
    }

    public (Sprite normal, Sprite highlight, Sprite selecting) GetChessImageBySide(bool side)
    {
        return (side) ? (BlackChess, BlackChess_Highlight, BlackChess_Selecting) : (WhiteChess, WhiteChess_Highlight, WhiteChess_Selecting);
    }
}
