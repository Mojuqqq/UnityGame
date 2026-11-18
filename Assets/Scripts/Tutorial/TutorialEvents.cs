using System;


public static class TutorialEvents
{
    // =====================================================
    // EVENTS
    // =====================================================

    public static event Action
        PieceDragStarted;


    public static event Action
        PieceDragCancelled;


    public static event Action
        PiecePlaced;


    public static event Action
        PieceRotated;


    // =====================================================
    // REPORT
    // =====================================================

    public static void ReportPieceDragStarted()
    {
        PieceDragStarted?.Invoke();
    }


    public static void ReportPieceDragCancelled()
    {
        PieceDragCancelled?.Invoke();
    }


    public static void ReportPiecePlaced()
    {
        PiecePlaced?.Invoke();
    }


    public static void ReportPieceRotated()
    {
        PieceRotated?.Invoke();
    }
}