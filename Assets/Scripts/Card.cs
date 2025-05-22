using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [Header("References")]
    public GameObject front;
    public GameObject back;
    public Animator animator;

    public CardData data;
    public bool isFlipped = false;
    public bool isMatched = false;

    private Image frontImage;

    private void Awake()
    {
        frontImage = front != null ? front.GetComponent<Image>() : null;
    }

    public void Setup(CardData newData)
    {
        data = newData;
        isFlipped = false;
        isMatched = false;

        if (frontImage != null && data != null && data.image != null)
            frontImage.sprite = data.image;

        ShowBackInstant();
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
        animator.SetTrigger("FlipToFront");
        AudioManager.Instance?.PlayCardFlip();
    }

    public void Unflip()
    {
        isFlipped = false;
        animator.SetTrigger("FlipToBack");
    }

    public void Match()
    {
        isMatched = true;
        animator.SetTrigger("Match");
        AudioManager.Instance?.PlayMatch();
    }

    public void MisMatch()
    {
        isMatched = false;
        isFlipped = false;
        animator.SetTrigger("MisMatch");
        AudioManager.Instance?.PlayMismatch();
    }
    public void SetMatched(bool value)
    {
        isMatched = value;  
    }

    private void ShowBackInstant()
    {
        animator.Play("Back", 0, 1f); // Ensure card starts back-side
    }
}
