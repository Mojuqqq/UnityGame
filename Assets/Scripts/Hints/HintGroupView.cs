using System.Collections.Generic;
using UnityEngine;

public enum HintGroupVisualType
{
    Column,
    Row
}

public class HintGroupView : MonoBehaviour
{
    [Header("Prefab")]

    [SerializeField]
    private HintIndicatorView indicatorPrefab;


    [Header("Sizes")]

    [SerializeField]
    private Vector2 columnIndicatorSize =
        new Vector2(
            32f,
            12f
        );

    [SerializeField]
    private Vector2 rowIndicatorSize =
        new Vector2(
            12f,
            32f
        );


    [Header("Spacing")]

    [SerializeField]
    private float indicatorSpacing = 7f;

    [SerializeField]
    private float laneSpacing = 8f;


    [Header("Edge Alignment")]

    [Tooltip(
        "Отступ индикаторов от нижней границы TopHints " +
        "или левой границы RowHints."
    )]
    [SerializeField]
    private float edgePadding = 0f;


    private readonly List<HintIndicatorView>
        indicators =
            new List<HintIndicatorView>();


    private int targetCount;

    private HintGroupVisualType visualType;


    // =====================================================
    // INITIALIZE
    // =====================================================

    public void Initialize(
        int newTargetCount,
        HintGroupVisualType newVisualType
    )
    {
        targetCount =
            Mathf.Max(
                0,
                newTargetCount
            );


        visualType =
            newVisualType;


        BuildIndicators();


        UpdateProgress(
            0
        );
    }


    // =====================================================
    // PROGRESS
    // =====================================================

    public void UpdateProgress(
        int currentCount
    )
    {
        if (indicators.Count == 0)
        {
            return;
        }


        // Перебор:
        // все индикаторы группы красные.
        if (currentCount > targetCount)
        {
            foreach (
                HintIndicatorView indicator
                in indicators
            )
            {
                indicator.SetState(
                    HintIndicatorState.Overflow
                );
            }


            return;
        }


        for (
            int i = 0;
            i < indicators.Count;
            i++
        )
        {
            if (i < currentCount)
            {
                indicators[i]
                    .SetState(
                        HintIndicatorState.Filled
                    );
            }
            else
            {
                indicators[i]
                    .SetState(
                        HintIndicatorState.Empty
                    );
            }
        }
    }


    // =====================================================
    // BUILD
    // =====================================================

    private void BuildIndicators()
    {
        ClearIndicators();


        if (
            indicatorPrefab == null ||
            targetCount <= 0
        )
        {
            return;
        }


        if (
            visualType ==
            HintGroupVisualType.Column
        )
        {
            BuildColumnIndicators();
        }
        else
        {
            BuildRowIndicators();
        }
    }


    // =====================================================
    // TOP / COLUMN HINTS
    //
    // Индикаторы прижаты к BOTTOM.
    // =====================================================

    private void BuildColumnIndicators()
    {
        if (targetCount <= 5)
        {
            BuildVerticalLane(
                targetCount,
                0f
            );


            return;
        }


        int leftCount =
            targetCount / 2;


        int rightCount =
            targetCount -
            leftCount;


        float xOffset =
            (
                columnIndicatorSize.x +
                laneSpacing
            ) * 0.5f;


        BuildVerticalLane(
            leftCount,
            -xOffset
        );


        BuildVerticalLane(
            rightCount,
            xOffset
        );
    }


    private void BuildVerticalLane(
        int count,
        float x
    )
    {
        if (count <= 0)
        {
            return;
        }


        // Anchor находится на BOTTOM CENTER группы.
        //
        // Первая полоска касается нижней границы,
        // следующие строятся вверх.

        float firstY =
            edgePadding +
            columnIndicatorSize.y *
            0.5f;


        for (
            int i = 0;
            i < count;
            i++
        )
        {
            float y =
                firstY +
                i *
                (
                    columnIndicatorSize.y +
                    indicatorSpacing
                );


            CreateIndicator(
                columnIndicatorSize,

                new Vector2(
                    0.5f,
                    0f
                ),

                new Vector2(
                    x,
                    y
                )
            );
        }
    }


    // =====================================================
    // RIGHT / ROW HINTS
    //
    // Индикаторы прижаты к LEFT.
    // =====================================================

    private void BuildRowIndicators()
    {
        if (targetCount <= 5)
        {
            BuildHorizontalLane(
                targetCount,
                0f
            );


            return;
        }


        int topCount =
            targetCount / 2;


        int bottomCount =
            targetCount -
            topCount;


        float yOffset =
            (
                rowIndicatorSize.y +
                laneSpacing
            ) * 0.5f;


        BuildHorizontalLane(
            topCount,
            yOffset
        );


        BuildHorizontalLane(
            bottomCount,
            -yOffset
        );
    }


    private void BuildHorizontalLane(
        int count,
        float y
    )
    {
        if (count <= 0)
        {
            return;
        }


        // Anchor находится на LEFT CENTER группы.
        //
        // Первая полоска начинается прямо от левого края,
        // остальные строятся вправо.

        float firstX =
            edgePadding +
            rowIndicatorSize.x *
            0.5f;


        for (
            int i = 0;
            i < count;
            i++
        )
        {
            float x =
                firstX +
                i *
                (
                    rowIndicatorSize.x +
                    indicatorSpacing
                );


            CreateIndicator(
                rowIndicatorSize,

                new Vector2(
                    0f,
                    0.5f
                ),

                new Vector2(
                    x,
                    y
                )
            );
        }
    }


    // =====================================================
    // CREATE
    // =====================================================

    private void CreateIndicator(
        Vector2 size,
        Vector2 anchor,
        Vector2 position
    )
    {
        HintIndicatorView indicator =
            Instantiate(
                indicatorPrefab,
                transform
            );


        indicator.name =
            $"HintIndicator_{indicators.Count}";


        RectTransform indicatorRect =
            indicator.GetComponent<
                RectTransform
            >();


        indicatorRect.anchorMin =
            anchor;

        indicatorRect.anchorMax =
            anchor;

        indicatorRect.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        indicator.SetSize(
            size
        );


        indicatorRect.anchoredPosition =
            position;


        indicator.SetState(
            HintIndicatorState.Empty
        );


        indicators.Add(
            indicator
        );
    }


    // =====================================================
    // CLEAR
    // =====================================================

    private void ClearIndicators()
    {
        indicators.Clear();


        for (
            int i =
                transform.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                transform
                    .GetChild(i)
                    .gameObject;


            child.SetActive(
                false
            );


            Destroy(
                child
            );
        }
    }
}