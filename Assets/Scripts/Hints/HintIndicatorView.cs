using UnityEngine;
using UnityEngine.UI;

public enum HintIndicatorState
{
    Empty,
    Filled,
    Overflow
}

[RequireComponent(typeof(Image))]
[RequireComponent(typeof(LayoutElement))]
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
    private LayoutElement layoutElement;


    private void Awake()
    {
        targetImage =
            GetComponent<Image>();

        layoutElement =
            GetComponent<LayoutElement>();
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

        layoutElement.minWidth =
            size.x;

        layoutElement.preferredWidth =
            size.x;

        layoutElement.minHeight =
            size.y;

        layoutElement.preferredHeight =
            size.y;
    }


    private void EnsureReferences()
    {
        if (targetImage == null)
        {
            targetImage =
                GetComponent<Image>();
        }

        if (layoutElement == null)
        {
            layoutElement =
                GetComponent<LayoutElement>();
        }
    }
}