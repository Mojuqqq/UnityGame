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
    private float indicatorSpacing =
        7f;

    [SerializeField]
    private float laneSpacing =
        8f;


    private readonly List<HintIndicatorView>
        indicators =
            new List<HintIndicatorView>();


    private int targetCount;

    private HintGroupVisualType visualType;


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


    public void UpdateProgress(
        int currentCount
    )
    {
        if (indicators.Count == 0)
        {
            return;
        }


        // Перебор:
        // вся подсказка становится красной.
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


        // Нормальный прогресс.
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
    // COLUMN HINTS
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


        float totalHeight =
            count *
            columnIndicatorSize.y +
            (count - 1) *
            indicatorSpacing;


        float firstY =
            totalHeight * 0.5f -
            columnIndicatorSize.y *
            0.5f;


        for (
            int i = 0;
            i < count;
            i++
        )
        {
            float y =
                firstY -
                i *
                (
                    columnIndicatorSize.y +
                    indicatorSpacing
                );


            CreateIndicator(
                columnIndicatorSize,
                new Vector2(
                    x,
                    y
                )
            );
        }
    }


    // =====================================================
    // ROW HINTS
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


        float totalWidth =
            count *
            rowIndicatorSize.x +
            (count - 1) *
            indicatorSpacing;


        float firstX =
            -totalWidth * 0.5f +
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


        indicator.SetSize(
            size
        );


        indicator.SetPosition(
            position
        );


        indicator.SetState(
            HintIndicatorState.Empty
        );


        indicators.Add(
            indicator
        );
    }


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