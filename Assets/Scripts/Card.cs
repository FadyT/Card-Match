using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public GameObject front;
    public GameObject back;
    public CardData data;
    public bool isFlipped = false;
    public bool isMatched = false;

    private Image frontImage;

    private void Awake()
    {
        frontImage = front?.GetComponent<Image>();
        front.SetActive(false);
        back.SetActive(true);
        transform.localScale = Vector3.one;
    }

    public void Setup(CardData newData)
    {
        data = newData;
        isFlipped = false;
        isMatched = false;

        if (frontImage != null && data?.image != null)
            frontImage.sprite = data.image;

        front.SetActive(false);
        back.SetActive(true);
        transform.localScale = Vector3.one;
    }

    public void OnClick()
    {
        if (isFlipped || isMatched || GameManager.Instance == null) return;
        GameManager.Instance.CardFlipped(this);
    }

    public void Flip()
    {
        if (isFlipped || isMatched) return;
        isFlipped = true;

        ShowCardFront(true);

        AudioManager.Instance?.PlayCardFlip();
    }


    public void ShowCardFront(bool showCard)
    {
        if (showCard)
        {

            print("Flipping !");
            Sequence flipSequence = DOTween.Sequence();

            flipSequence.Append(back.transform.DOScaleX(0f, 0.2f).SetEase(Ease.InBack))
                .AppendCallback(() =>
                {
                    back.SetActive(false);
                    front.SetActive(true); // activate first
                    front.transform.localScale = new Vector3(0f, 1f, 1f); // reset scale X
                })
                .Append(front.transform.DOScaleX(1f, 0.2f).SetEase(Ease.OutBack));

        }
        else

        {

            Sequence unflipSequence = DOTween.Sequence();

            unflipSequence.Append(front.transform.DOScaleX(0f, 0.2f).SetEase(Ease.InBack))
                .AppendCallback(() =>
                {
                    front.SetActive(false);
                    back.SetActive(true); // activate back
                    back.transform.localScale = new Vector3(0f, 1f, 1f); // reset scale X
                })
                .Append(back.transform.DOScaleX(1f, 0.2f).SetEase(Ease.OutBack));


        }
    }

    public void Unflip()
    {
        isFlipped = false;
        ShowCardFront(false);
    }

    public void Match()
    {
        isMatched = true;
        transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 5, 0.5f);
        AudioManager.Instance?.PlayMatch();
    }

    public void MisMatch()
    {
        // Shake first, then unflip after
        Sequence mismatchSequence = DOTween.Sequence();
        transform.DOShakeRotation(0.4f, new Vector3(0, 0, 25), 10, 90, true);
        isMatched = false;
        isFlipped = false;
        AudioManager.Instance?.PlayMismatch();
    }

    public void SetMatched(bool value)
    {
        isMatched = value;
    }
}
