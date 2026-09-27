public static class LevelSelectionState
{
    public static LevelData SelectedLevel
    {
        get;
        private set;
    }


    public static void SelectLevel(
        LevelData level
    )
    {
        SelectedLevel =
            level;
    }


    public static void ClearSelection()
    {
        SelectedLevel =
            null;
    }
}