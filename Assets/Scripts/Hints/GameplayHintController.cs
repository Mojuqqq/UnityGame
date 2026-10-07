using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameplayHintController :
    MonoBehaviour
{
    [Header("Gameplay")]

    [SerializeField]
    private GridManager gridManager;


    [Header("Popup")]

    [SerializeField]
    private GameObject hintPopup;


    [Header("Buttons")]

    [SerializeField]
    private Button revealPieceButton;

    [SerializeField]
    private Button revealEmptyCellsButton;


    [Header("Counter")]

    [SerializeField]
    private TMP_Text hintCountText;


    [Header("Piece Hint")]

    [SerializeField]
    [Range(0.05f, 1f)]
    private float pieceHintAlpha =
        0.45f;


    [Header("Empty Cells Hint")]

    [SerializeField]
    [Min(1)]
    private int emptyCellsPerUse =
        3;


    private LevelData currentLevel;


    private readonly HashSet<int>
        revealedPieceIndices =
            new HashSet<int>();


    private readonly HashSet<Vector2Int>
        revealedEmptyCells =
            new HashSet<Vector2Int>();


    private bool subscribedToGrid;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        LevelData levelData
    )
    {
        currentLevel =
            levelData;


        revealedPieceIndices.Clear();

        revealedEmptyCells.Clear();


        ClearAllCellHintVisuals();


        SubscribeToGrid();


        CloseHintPopup();

        RefreshUI();
    }


    // =====================================================
    // POPUP
    // =====================================================

    public void OpenHintPopup()
    {
        if (
            currentLevel == null ||
            hintPopup == null
        )
        {
            return;
        }


        RefreshUI();


        hintPopup.SetActive(
            true
        );
    }


    public void CloseHintPopup()
    {
        if (hintPopup == null)
        {
            return;
        }


        hintPopup.SetActive(
            false
        );
    }


    // =====================================================
    // HINT 1
    // REVEAL RANDOM PIECE
    // =====================================================

    public void RevealRandomPiece()
{
    if (currentLevel == null)
    {
        return;
    }


    List<int> candidates =
        GetPieceHintCandidates();


    if (candidates.Count == 0)
    {
        RefreshUI();

        return;
    }


    if (
        !PlayerResources.TrySpendHint(
            HintType.RevealPiece
        )
    )
    {
        RefreshUI();

        return;
    }


    PlayerStats.RecordHintUsed();


    int randomIndex =
        Random.Range(
            0,
            candidates.Count
        );


    int pieceIndex =
        candidates[randomIndex];


    revealedPieceIndices.Add(
        pieceIndex
    );


    RefreshRevealedPieceVisuals();

    RefreshUI();

    CloseHintPopup();
}
    
    private void RefreshRevealedPieceVisuals()
    {
        if (
            currentLevel == null ||
            gridManager == null
        )
        {
            return;
        }


        foreach (
            int pieceIndex
            in revealedPieceIndices
        )
        {
            LevelSolutionEntry entry =
                currentLevel
                    .GetSolutionEntryByPieceIndex(
                        pieceIndex
                    );


            if (entry == null)
            {
                continue;
            }


            Vector2Int[] cells =
                currentLevel
                    .GetSolutionCells(
                        entry
                    );


            bool filled =
                AreAllCellsOccupied(
                    cells
                );


            Color hintColor =
                currentLevel
                    .Pieces[
                        pieceIndex
                    ]
                    .Color;


            hintColor.a =
                pieceHintAlpha;


            foreach (
                Vector2Int coordinate
                in cells
            )
            {
                GridCell cell =
                    gridManager.GetCell(
                        coordinate.x,
                        coordinate.y
                    );


                if (cell == null)
                {
                    continue;
                }


                if (filled)
                {
                    cell.HideSolutionHint();
                }
                else
                {
                    cell.ShowSolutionHint(
                        hintColor
                    );
                }
            }
        }
    }


    // =====================================================
    // HINT 2
    // REVEAL EMPTY CELLS
    // =====================================================

    public void RevealRandomEmptyCells()
{
    if (currentLevel == null)
    {
        return;
    }


    List<Vector2Int> candidates =
        GetEmptyCellCandidates();


    if (candidates.Count == 0)
    {
        RefreshUI();

        return;
    }


    if (
        !PlayerResources.TrySpendHint(
            HintType.RevealEmptyCells
        )
    )
    {
        RefreshUI();

        return;
    }


    PlayerStats.RecordHintUsed();


    int revealCount =
        Mathf.Min(
            emptyCellsPerUse,
            candidates.Count
        );


    for (
        int i = 0;
        i < revealCount;
        i++
    )
    {
        int randomIndex =
            Random.Range(
                0,
                candidates.Count
            );


        Vector2Int coordinate =
            candidates[randomIndex];


        candidates.RemoveAt(
            randomIndex
        );


        revealedEmptyCells.Add(
            coordinate
        );


        GridCell cell =
            gridManager.GetCell(
                coordinate.x,
                coordinate.y
            );


        if (cell != null)
        {
            cell.ShowEmptyHint();
        }
    }


    RefreshUI();

    CloseHintPopup();
}


    // =====================================================
    // GRID CHANGE
    // =====================================================

    private void HandleGridChanged()
    {
        // Раскрытая фигура:
        //
        // Пока её область не заполнена —
        // показываем подсветку.
        //
        // Когда область заполнена —
        // скрываем.
        //
        // Если игрок потом снова освободит
        // эту область, подсветка появится
        // обратно.
        RefreshRevealedPieceVisuals();


        RefreshUI();
    }


    // =====================================================
    // HELPERS
    // =====================================================

    private bool AreAllCellsOccupied(
        IReadOnlyList<Vector2Int> cells
    )
    {
        if (
            cells == null ||
            cells.Count == 0
        )
        {
            return false;
        }


        foreach (
            Vector2Int coordinate
            in cells
        )
        {
            GridCell cell =
                gridManager.GetCell(
                    coordinate.x,
                    coordinate.y
                );


            if (
                cell == null ||
                !cell.IsOccupied()
            )
            {
                return false;
            }
        }


        return true;
    }


    // =====================================================
    // UI
    // =====================================================

    private void RefreshUI()
{
    int pieceHints =
        PlayerResources.GetHintCount(
            HintType.RevealPiece
        );


    int emptyHints =
        PlayerResources.GetHintCount(
            HintType.RevealEmptyCells
        );


    // Пока Level01 всё ещё использует
    // старый общий текстовый счётчик.
    if (hintCountText != null)
    {
        hintCountText.text =
            (
                pieceHints +
                emptyHints
            )
            .ToString();
    }


    if (revealPieceButton != null)
    {
        revealPieceButton.interactable =
            pieceHints > 0
            &&
            GetPieceHintCandidates()
                .Count > 0;
    }


    if (revealEmptyCellsButton != null)
    {
        revealEmptyCellsButton.interactable =
            emptyHints > 0
            &&
            GetEmptyCellCandidates()
                .Count > 0;
    }
}


    // =====================================================
    // VISUAL RESET
    // =====================================================

    private void ClearAllCellHintVisuals()
    {
        if (gridManager == null)
        {
            return;
        }


        for (
            int y = 0;
            y < gridManager.Rows;
            y++
        )
        {
            for (
                int x = 0;
                x < gridManager.Columns;
                x++
            )
            {
                GridCell cell =
                    gridManager.GetCell(
                        x,
                        y
                    );


                if (cell != null)
                {
                    cell.ClearGameplayHints();
                }
            }
        }
    }


    // =====================================================
    // EVENTS
    // =====================================================

    private void SubscribeToGrid()
    {
        if (
            subscribedToGrid ||
            gridManager == null
        )
        {
            return;
        }


        gridManager.GridChanged +=
            HandleGridChanged;


        subscribedToGrid =
            true;
    }


    private void UnsubscribeFromGrid()
    {
        if (
            !subscribedToGrid ||
            gridManager == null
        )
        {
            return;
        }


        gridManager.GridChanged -=
            HandleGridChanged;


        subscribedToGrid =
            false;
    }


    private void OnDestroy()
    {
        UnsubscribeFromGrid();
    }
}