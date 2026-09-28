using System.Collections.Generic;
using UnityEngine;

public enum HintGroupVisualType
{
    Column,
    Row
}

public class HintGroupView : MonoBehaviour
{
    [Header("References")]

    [SerializeField]
    private RectTransform rowA;

    [SerializeField]
    private RectTransform rowB;

    [SerializeField]
    private HintIndicatorView
        indicatorPrefab;


    [Header("Indicator Sizes")]

    [SerializeField]
    private Vector2 columnIndicatorSize =
        new Vector2(
            28f,
            12f
        );

    [SerializeField]
    private Vector2 rowIndicatorSize =
        new Vector2(
            12f,
            28f
        );


    private readonly List<HintIndicatorView>
        indicators =
            new List<HintIndicatorView>();


    private int targetCount;
    private HintGroupVisualType
        visualType;


    public int TargetCount =>
        targetCount;


    public void Initialize(
        int newTargetCount,
        HintGroupVisualType
            newVisualType
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
        UpdateProgress(0);
    }


    public void UpdateProgress(
        int currentCount
    )
    {
        if (
            indicators == null ||
            indicators.Count == 0
        )
        {
            return;
        }


        if (currentCount > targetCount)
        {
            foreach (
                HintIndicatorView indicator
                in indicators
            )
            {
                if (indicator != null)
                {
                    indicator.SetState(
                        HintIndicatorState.Overflow
                    );
                }
            }

            return;
        }


        for (
            int i = 0;
            i < indicators.Count;
            i++
        )
        {
            HintIndicatorView indicator =
                indicators[i];

            if (indicator == null)
            {
                continue;
            }

            if (i < currentCount)
            {
                indicator.SetState(
                    HintIndicatorState.Filled
                );
            }
            else
            {
                indicator.SetState(
                    HintIndicatorState.Empty
                );
            }
        }
    }


    private void BuildIndicators()
    {
        ClearRows();

        if (
            indicatorPrefab == null ||
            rowA == null ||
            rowB == null
        )
        {
            Debug.LogError(
                $"HintGroupView '{gameObject.name}': " +
                "references are missing."
            );

            return;
        }


        int firstRowCount;
        int secondRowCount;

        GetRowDistribution(
            targetCount,
            out firstRowCount,
            out secondRowCount
        );


        Vector2 targetSize =
            visualType ==
            HintGroupVisualType.Column
                ? columnIndicatorSize
                : rowIndicatorSize;


        for (
            int i = 0;
            i < firstRowCount;
            i++
        )
        {
            HintIndicatorView
                indicator =
                    Instantiate(
                        indicatorPrefab,
                        rowA
                    );

            indicator.SetSize(
                targetSize
            );

            indicators.Add(
                indicator
            );
        }


        rowB.gameObject.SetActive(
            secondRowCount > 0
        );


        for (
            int i = 0;
            i < secondRowCount;
            i++
        )
        {
            HintIndicatorView
                indicator =
                    Instantiate(
                        indicatorPrefab,
                        rowB
                    );

            indicator.SetSize(
                targetSize
            );

            indicators.Add(
                indicator
            );
        }
    }


    private void GetRowDistribution(
        int totalCount,
        out int firstRowCount,
        out int secondRowCount
    )
    {
        if (totalCount <= 0)
        {
            firstRowCount =
                0;

            secondRowCount =
                0;

            return;
        }


        if (totalCount <= 5)
        {
            firstRowCount =
                totalCount;

            secondRowCount =
                0;

            return;
        }


        firstRowCount =
            totalCount / 2;

        secondRowCount =
            totalCount -
            firstRowCount;
    }


    private void ClearRows()
    {
        indicators.Clear();

        ClearChildren(rowA);
        ClearChildren(rowB);

        if (rowB != null)
        {
            rowB.gameObject.SetActive(
                false
            );
        }
    }


    private void ClearChildren(
        RectTransform container
    )
    {
        if (container == null)
        {
            return;
        }

        for (
            int i =
                container.childCount - 1;
            i >= 0;
            i--
        )
        {
            GameObject child =
                container
                    .GetChild(i)
                    .gameObject;

            child.SetActive(false);
            Destroy(child);
        }
    }
}