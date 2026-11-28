using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private PieceView pieceViewPrefab;

    [Header("Containers")]
    [SerializeField] private RectTransform piecesContainer;
    [SerializeField] private RectTransform piecePanel;
    [SerializeField] private RectTransform dragLayer;

    [Header("Gameplay")]
    [SerializeField] private GridManager gridManager;

    [Header("Adaptive Piece Size")]
    [SerializeField, Min(1f)] private float minPanelCellSize = 30f;
    [SerializeField, Min(1f)] private float maxPanelCellSize = 70f;
    [Tooltip("Максимальный размер клетки фигуры относительно клетки игрового поля.")]
    [SerializeField, Range(0.1f, 1f)] private float gridCellRatio = 0.38f;

    [Header("Cell Spacing")]
    [SerializeField, Min(0f)] private float internalCellSpacing = 4f;

    [Header("Piece Spacing")]
    [SerializeField, Min(0f)] private float horizontalPieceSpacing = 36f;
    [SerializeField, Min(0f)] private float verticalPieceSpacing = 30f;

    [Header("Panel Reflow")]
    [SerializeField, Min(0.01f)] private float panelReflowDuration = 0.18f;

    private readonly List<PieceView> spawnedPieces = new List<PieceView>();
    private List<PieceRow> cachedRows = new List<PieceRow>();
    private readonly Dictionary<PieceView, PanelSlot> panelSlots = new Dictionary<PieceView, PanelSlot>();
    private readonly Dictionary<PieceView, Vector2> slotCenters = new Dictionary<PieceView, Vector2>();

    private PieceView returningPiece;
    private float panelCellSize;
    private float panelSpacing;
    private bool slotsMoving;
    private bool panelWasRotating;

    public bool GameplayLocked { get; private set; }
    public bool IsPanelReflowing => panelWasRotating || HasPanelRotation() || slotsMoving || returningPiece != null;

    private class PieceRow
    {
        public readonly List<PieceView> pieces = new List<PieceView>();
        public float width;
        public float height;
    }

    private class PanelSlot
    {
        public Vector2 Current;
        public Vector2 From;
        public Vector2 Target;
        public float Elapsed;

        public bool Update(Vector2 target, float duration, bool immediate)
        {
            duration = Mathf.Max(0.01f, duration);
            if (immediate)
            {
                Current = From = Target = target;
                Elapsed = duration;
                return false;
            }

            if (Target != target)
            {
                From = Current;
                Target = target;
                Elapsed = 0f;
            }

            Elapsed = Mathf.Min(Elapsed + Time.unscaledDeltaTime, duration);
            float t = Mathf.Clamp01(Elapsed / duration);
            Current = Vector2.Lerp(From, Target, Mathf.SmoothStep(0f, 1f, t));
            if (t >= 1f)
                Current = Target;
            return t < 1f;
        }
    }

    public void LockGameplay()
    {
        GameplayLocked = true;
    }

    private bool HasPanelRotation()
    {
        foreach (PieceView piece in spawnedPieces)
        {
            if (piece != null && piece.transform.parent == piecesContainer && piece.IsRotating)
                return true;
        }
        return false;
    }

    public bool CanStartPanelRotation()
    {
        if (GameplayLocked || IsPanelReflowing)
            return false;

        foreach (PieceView piece in spawnedPieces)
        {
            if (piece == null)
                continue;
            PieceDragHandler handler = piece.GetComponent<PieceDragHandler>();
            if (handler != null && handler.IsBusyForPanelRotation)
                return false;
        }
        return true;
    }

    private void LateUpdate()
    {
        if (cachedRows.Count == 0)
            return;

        bool rotating = HasPanelRotation();
        bool wasReflowing = panelWasRotating || slotsMoving;
        slotsMoving = false;

        foreach (var entry in panelSlots)
        {
            PieceView piece = entry.Key;
            if (piece == null)
                continue;

            bool immediate = piece.transform.parent == piecesContainer &&
                             (piece.IsRotating || panelWasRotating);

            if (entry.Value.Update(GetTargetSlotSize(piece), panelReflowDuration, immediate))
                slotsMoving = true;
        }

        LayoutRows(cachedRows);
        panelWasRotating = rotating;

        if (wasReflowing && !rotating && !slotsMoving && returningPiece == null)
            RefreshPanelHomes();
    }

    // BUILD
    public void BuildPieces(PieceDefinition[] definitions)
    {
        ClearPieces();

        if (definitions == null || definitions.Length == 0)
        {
            Debug.LogError("PieceSpawner: No pieces supplied.");
            return;
        }
        if (pieceViewPrefab == null)
        {
            Debug.LogError("PieceSpawner: PieceView Prefab is not assigned.");
            return;
        }
        if (piecesContainer == null)
        {
            Debug.LogError("PieceSpawner: PiecesContainer is not assigned.");
            return;
        }

        foreach (PieceDefinition definition in definitions)
        {
            if (definition == null)
                continue;

            PieceView piece = Instantiate(pieceViewPrefab, piecesContainer);
            piece.name = $"Piece_{spawnedPieces.Count}_{definition.PieceId}";
            piece.Initialize(definition);

            PieceDragHandler handler = piece.GetComponent<PieceDragHandler>();
            if (handler != null)
            {
                handler.Configure(gridManager, dragLayer, piecePanel);
                handler.SetPieceSpawner(this);
            }
            else
            {
                Debug.LogError($"{piece.name} has no PieceDragHandler.");
            }
            spawnedPieces.Add(piece);
        }

        Canvas.ForceUpdateCanvases();
        RefreshLayout();
    }

    // REFRESH
    public void RefreshLayout()
    {
        if (spawnedPieces.Count == 0 || piecesContainer == null)
            return;

        Canvas.ForceUpdateCanvases();
        float availableWidth = piecesContainer.rect.width;
        float availableHeight = piecesContainer.rect.height;
        if (availableWidth <= 0f || availableHeight <= 0f)
            return;

        float effectiveMaxCellSize = maxPanelCellSize;
        if (gridManager != null && gridManager.CellSize > 0f)
            effectiveMaxCellSize = Mathf.Min(maxPanelCellSize, gridManager.CellSize * gridCellRatio);
        effectiveMaxCellSize = Mathf.Max(minPanelCellSize, effectiveMaxCellSize);

        panelCellSize = Mathf.Round(FindBestCellSize(availableWidth, availableHeight, effectiveMaxCellSize));
        panelSpacing = Mathf.Max(1f, Mathf.Round(internalCellSpacing));

        foreach (PieceView piece in spawnedPieces)
        {
            if (piece != null && piece.transform.parent == piecesContainer)
                piece.SetLayout(panelCellSize, panelSpacing);
        }

        Canvas.ForceUpdateCanvases();
        cachedRows = BuildRows(availableWidth, panelCellSize, panelSpacing);
        panelSlots.Clear();
        slotCenters.Clear();

        foreach (PieceRow row in cachedRows)
        {
            foreach (PieceView piece in row.pieces)
            {
                Vector2 size = GetTargetSlotSize(piece);
                panelSlots[piece] = new PanelSlot
                {
                    Current = size,
                    From = size,
                    Target = size,
                    Elapsed = panelReflowDuration
                };
            }
        }

        LayoutRows(cachedRows);
        RefreshPanelHomes();
    }

    private Vector2 GetTargetSlotSize(PieceView piece)
    {
        if (piece == null)
            return Vector2.zero;

        Vector2 size;
        if (piece.transform.parent == piecesContainer)
            size = piece.GetPanelVisualSize();
        else if (piece == returningPiece)
            size = CalculatePieceSize(piece, panelCellSize, panelSpacing);
        else
            return Vector2.zero;

        return size + new Vector2(horizontalPieceSpacing, verticalPieceSpacing);
    }

    private void RefreshPanelHomes()
    {
        foreach (PieceRow row in cachedRows)
        {
            foreach (PieceView piece in row.pieces)
            {
                if (piece == null || piece.transform.parent != piecesContainer)
                    continue;
                PieceDragHandler handler = piece.GetComponent<PieceDragHandler>();
                if (handler != null)
                    handler.RefreshPanelHome();
            }
        }
    }

    // FIT AND ROWS
    private float FindBestCellSize(float availableWidth, float availableHeight, float maxCellSize)
    {
        int minimum = Mathf.Max(1, Mathf.CeilToInt(minPanelCellSize));
        int candidate = Mathf.Max(minimum, Mathf.FloorToInt(maxCellSize));
        float spacing = Mathf.Max(1f, Mathf.Round(internalCellSpacing));

        while (candidate >= minimum)
        {
            List<PieceRow> rows = BuildRows(availableWidth, candidate, spacing);
            bool fits = GetRowsTotalHeight(rows) <= availableHeight;
            foreach (PieceRow row in rows)
            {
                if (row.width > availableWidth)
                {
                    fits = false;
                    break;
                }
            }
            if (fits)
                return candidate;
            candidate--;
        }
        Debug.LogWarning("PieceSpawner: Not enough space for safe rotation.");
        return minimum;
    }

    private List<PieceRow> BuildRows(float availableWidth, float cellSize, float spacing)
    {
        List<PieceRow> rows = new List<PieceRow>();
        PieceRow row = new PieceRow();

        // Reserve the same outer spacing used by animated slots.
        // Keep all pieces in their original rows, including pieces on Grid.
        foreach (PieceView piece in spawnedPieces)
        {
            if (piece == null)
                continue;

            Vector2 size = CalculateReservedPieceSize(piece, cellSize, spacing);
            size += new Vector2(horizontalPieceSpacing, verticalPieceSpacing);

            if (row.pieces.Count > 0 && row.width + size.x > availableWidth)
            {
                rows.Add(row);
                row = new PieceRow();
            }

            row.pieces.Add(piece);
            row.width += size.x;
            row.height = Mathf.Max(row.height, size.y);
        }
        if (row.pieces.Count > 0)
            rows.Add(row);
        return rows;
    }

    private Vector2 CalculateReservedPieceSize(PieceView piece, float cellSize, float spacing)
    {
        float diagonal = CalculatePieceSize(piece, cellSize, spacing).magnitude;
        return new Vector2(diagonal, diagonal);
    }

    private Vector2 CalculatePieceSize(PieceView piece, float cellSize, float spacing)
    {
        if (piece == null || piece.CurrentCells == null || piece.CurrentCells.Length == 0)
            return Vector2.zero;

        int maxX = 0;
        int maxY = 0;
        foreach (Vector2Int cell in piece.CurrentCells)
        {
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }
        return new Vector2((maxX + 1) * cellSize + maxX * spacing,
                           (maxY + 1) * cellSize + maxY * spacing);
    }

    private float GetRowsTotalHeight(List<PieceRow> rows)
    {
        float height = 0f;
        foreach (PieceRow row in rows)
            height += row.height;
        return height;
    }

    // FINAL POSITIONING
    private void LayoutRows(List<PieceRow> rows)
    {
        if (rows == null || rows.Count == 0)
            return;

        float totalHeight = 0f;
        foreach (PieceRow row in rows)
        {
            row.width = 0f;
            row.height = 0f;
            foreach (PieceView piece in row.pieces)
            {
                if (piece == null || !panelSlots.TryGetValue(piece, out PanelSlot slot))
                    continue;
                row.width += slot.Current.x;
                row.height = Mathf.Max(row.height, slot.Current.y);
            }
            totalHeight += row.height;
        }

        float currentY = totalHeight * 0.5f;
        foreach (PieceRow row in rows)
        {
            float currentX = -row.width * 0.5f;
            float rowCenterY = currentY - row.height * 0.5f;

            foreach (PieceView piece in row.pieces)
            {
                if (piece == null || !panelSlots.TryGetValue(piece, out PanelSlot slot))
                    continue;

                Vector2 position = new Vector2(currentX + slot.Current.x * 0.5f, rowCenterY);
                slotCenters[piece] = position;

                if (piece.transform.parent == piecesContainer)
                {
                    RectTransform rect = piece.GetComponent<RectTransform>();
                    rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
                    rect.anchoredPosition = position;
                }
                currentX += slot.Current.x;
            }
            currentY -= row.height;
        }
    }

    // RETURN TO PANEL
    public IEnumerator PrepareReturnSlot(PieceView piece)
    {
        if (piece == null || !panelSlots.TryGetValue(piece, out PanelSlot slot))
            yield break;

        returningPiece = piece;
        yield return null;

        while (piece != null && returningPiece == piece &&
               ((slot.Current - GetTargetSlotSize(piece)).sqrMagnitude > 0.0001f || slotsMoving))
        {
            yield return null;
        }

        if (piece != null && returningPiece == piece)
            LayoutRows(cachedRows);
    }

    public bool TryGetReturnTarget(PieceView piece, out Vector2 anchoredPosition,
        out Vector3 topLeftWorld, out float cellSize, out float spacing)
    {
        anchoredPosition = Vector2.zero;
        topLeftWorld = Vector3.zero;
        cellSize = panelCellSize;
        spacing = panelSpacing;

        if (piece == null || piecesContainer == null || returningPiece != piece ||
            !slotCenters.TryGetValue(piece, out anchoredPosition))
            return false;

        Vector2 size = CalculatePieceSize(piece, cellSize, spacing);
        Vector2 topLeftLocal = piecesContainer.rect.center + anchoredPosition +
                               new Vector2(-size.x * 0.5f, size.y * 0.5f);
        topLeftWorld = piecesContainer.TransformPoint(topLeftLocal);
        return true;
    }

    public void EndPanelReturn(PieceView piece)
    {
        if (returningPiece == piece)
            returningPiece = null;
        RefreshPanelHomes();
    }

    // CLEAR
    public void ClearPieces()
    {
        cachedRows.Clear();
        panelSlots.Clear();
        slotCenters.Clear();
        returningPiece = null;
        slotsMoving = false;
        panelWasRotating = false;
        GameplayLocked = false;

        // Pieces on Grid are also owned by this spawner.
        foreach (PieceView piece in spawnedPieces)
        {
            if (piece == null || piece.transform.parent == piecesContainer)
                continue;
            piece.gameObject.SetActive(false);
            Destroy(piece.gameObject);
        }
        spawnedPieces.Clear();

        if (piecesContainer == null)
            return;
        for (int i = piecesContainer.childCount - 1; i >= 0; i--)
        {
            GameObject child = piecesContainer.GetChild(i).gameObject;
            child.SetActive(false);
            Destroy(child);
        }
    }
}
