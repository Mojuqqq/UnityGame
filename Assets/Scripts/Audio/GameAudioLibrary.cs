using UnityEngine;


[CreateAssetMenu(
    fileName = "GameAudioLibrary",
    menuName = "Puzzle/Audio Library"
)]
public class GameAudioLibrary :
    ScriptableObject
{
    [Header("UI")]

    [SerializeField]
    private AudioClip uiClick;


    [Header("Pieces")]

    [SerializeField]
    private AudioClip piecePickup;


    [SerializeField]
    private AudioClip piecePlace;


    [SerializeField]
    private AudioClip invalidDrop;


    [SerializeField]
    private AudioClip pieceRotate;


    [Header("Gameplay")]

    [SerializeField]
    private AudioClip hint;


    [SerializeField]
    private AudioClip levelComplete;


    [SerializeField]
    private AudioClip reward;


    public AudioClip GetClip(
        GameSfx type
    )
    {
        switch (type)
        {
            case GameSfx.UiClick:
                return uiClick;


            case GameSfx.PiecePickup:
                return piecePickup;


            case GameSfx.PiecePlace:
                return piecePlace;


            case GameSfx.InvalidDrop:
                return invalidDrop;


            case GameSfx.PieceRotate:
                return pieceRotate;


            case GameSfx.Hint:
                return hint;


            case GameSfx.LevelComplete:
                return levelComplete;


            case GameSfx.Reward:
                return reward;
        }


        return null;
    }
}