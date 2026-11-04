using System;


public static class TutorialEvents
{
    // =====================================================
    // EVENTS
    // =====================================================

    public static event Action
        PiecePlaced;


    public static event Action
        PieceRotated;


    // =====================================================
    // REPORT
    // =====================================================

    public static void ReportPiecePlaced()
    {
        PiecePlaced?.Invoke();
    }


    public static void ReportPieceRotated()
    {
        PieceRotated?.Invoke();
    }
}