using UnityEngine;
using UnityEngine.UI;

public enum HintIndicatorState
{
    Empty,
    Filled,
    Overflow
}

[RequireComponent(typeof(Image))]
public class HintIndicatorView : MonoBehaviour
{
    [Header("Colors")]

    [SerializeField]
    private Color emptyColor =
        new Color(
            1f,
            1f,
            1f,
            1f
        );

    [SerializeField]
    private Color filledColor =
        new Color(
            0.45f,
            1f,
            0.2f,
            1f
        );

    [SerializeField]
    private Color overflowColor =
        new Color(
            1f,
            0.2f,
            0.2f,
            1f
        );


    private Image targetImage;
    private RectTransform rectTransform;


    private void Awake()
    {
        targetImage =
            GetComponent<Image>();

        rectTransform =
            GetComponent<RectTransform>();
    }


    public void SetState(
        HintIndicatorState state
    )
    {
        EnsureReferences();


        switch (state)
        {
            case HintIndicatorState.Empty:

                targetImage.color =
                    emptyColor;

                break;


            case HintIndicatorState.Filled:

                targetImage.color =
                    filledColor;

                break;


            case HintIndicatorState.Overflow:

                targetImage.color =
                    overflowColor;

                break;
        }
    }


    public void SetSize(
        Vector2 size
    )
    {
        EnsureReferences();


        rectTransform.sizeDelta =
            size;
    }


    public void SetPosition(
        Vector2 anchoredPosition
    )
    {
        EnsureReferences();


        rectTransform.anchorMin =
            new Vector2(
                0.5f,
                0.5f
            );

        rectTransform.anchorMax =
            new Vector2(
                0.5f,
                0.5f
            );

        rectTransform.pivot =
            new Vector2(
                0.5f,
                0.5f
            );


        rectTransform.anchoredPosition =
            anchoredPosition;
    }


    private void EnsureReferences()
    {
        if (targetImage == null)
        {
            targetImage =
                GetComponent<Image>();
        }


        if (rectTransform == null)
        {
            rectTransform =
                GetComponent<RectTransform>();
        }
    }
}