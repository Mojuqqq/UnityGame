using UnityEngine;


public enum TutorialStep
{
    DragPiece,
    ReadHints,
    RotatePiece,
    BlockedCell,
    HintButtons
}


public static class TutorialProgress
{
    // =====================================================
    // KEYS
    // =====================================================

    private const string CompletedKey =
        "Puzzle_Tutorial_Completed";


    private const string DragPieceKey =
        "Puzzle_Tutorial_DragPiece";


    private const string ReadHintsKey =
        "Puzzle_Tutorial_ReadHints";


    private const string RotatePieceKey =
        "Puzzle_Tutorial_RotatePiece";


    private const string BlockedCellKey =
        "Puzzle_Tutorial_BlockedCell";


    private const string HintButtonsKey =
        "Puzzle_Tutorial_HintButtons";


    // =====================================================
    // COMPLETE
    // =====================================================

    public static bool IsTutorialCompleted()
    {
        return
            PlayerPrefs.GetInt(
                CompletedKey,
                0
            ) == 1;
    }


    // =====================================================
    // STEP STATE
    // =====================================================

    public static bool IsStepCompleted(
        TutorialStep step
    )
    {
        return
            PlayerPrefs.GetInt(
                GetStepKey(step),
                0
            ) == 1;
    }


    public static void MarkStepCompleted(
        TutorialStep step
    )
    {
        PlayerPrefs.SetInt(
            GetStepKey(step),
            1
        );


        if (AreAllStepsCompleted())
        {
            PlayerPrefs.SetInt(
                CompletedKey,
                1
            );
        }


        PlayerPrefs.Save();
    }


    // =====================================================
    // ALL STEPS
    // =====================================================

    private static bool AreAllStepsCompleted()
    {
        return
            IsStepCompleted(
                TutorialStep.DragPiece
            )
            &&
            IsStepCompleted(
                TutorialStep.ReadHints
            )
            &&
            IsStepCompleted(
                TutorialStep.RotatePiece
            )
            &&
            IsStepCompleted(
                TutorialStep.BlockedCell
            )
            &&
            IsStepCompleted(
                TutorialStep.HintButtons
            );
    }


    // =====================================================
    // KEY
    // =====================================================

    private static string GetStepKey(
        TutorialStep step
    )
    {
        switch (step)
        {
            case TutorialStep.DragPiece:

                return
                    DragPieceKey;


            case TutorialStep.ReadHints:

                return
                    ReadHintsKey;


            case TutorialStep.RotatePiece:

                return
                    RotatePieceKey;


            case TutorialStep.BlockedCell:

                return
                    BlockedCellKey;


            case TutorialStep.HintButtons:

                return
                    HintButtonsKey;
        }


        return
            "Puzzle_Tutorial_Unknown";
    }


    // =====================================================
    // RESET
    // =====================================================

    public static void ResetForTesting()
    {
        PlayerPrefs.DeleteKey(
            CompletedKey
        );


        PlayerPrefs.DeleteKey(
            DragPieceKey
        );


        PlayerPrefs.DeleteKey(
            ReadHintsKey
        );


        PlayerPrefs.DeleteKey(
            RotatePieceKey
        );


        PlayerPrefs.DeleteKey(
            BlockedCellKey
        );


        PlayerPrefs.DeleteKey(
            HintButtonsKey
        );


        PlayerPrefs.Save();
    }
}