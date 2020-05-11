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
    Sprite BlackChess_Remove;
    [SerializeField]
    Sprite WhiteChess;
    [SerializeField]
    Sprite WhiteChess_Highlight;
    [SerializeField]
    Sprite WhiteChess_Selecting;
    [SerializeField]
    Sprite WhiteChess_Remove;

    public Sprite GetCheckImageBySide(bool side)
    {
        return (side) ? BlackCheck : WhiteCheck;
    }

    public (Sprite normal, Sprite highlight, Sprite selecting, Sprite remove) GetChessImageBySide(bool side)
    {
        return (side) ? (BlackChess, BlackChess_Highlight, BlackChess_Selecting, BlackChess_Remove) : (WhiteChess, WhiteChess_Highlight, WhiteChess_Selecting, WhiteChess_Remove);
    }
}
