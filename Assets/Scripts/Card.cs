using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public int cardId;
    public CardData data;
    public GameObject front;
    public GameObject back;

    public bool isFlipped = false;
    public bool isMatched = false;

    public Animator animator;

void Start()
{
    if (data != null && front != null)
    {
        Image frontImage = front.GetComponent<Image>();
        if (frontImage != null && data.image != null)
        {
            frontImage.sprite = data.image;
        }
    }
}


    public void OnClick()
    {
        if (isFlipped || isMatched) return;
        GameManager.Instance.CardFlipped(this);

        Flip();

    }

    public void Flip()
    {
        AudioManager.Instance.PlayCardFlip();
        isFlipped = true;
        ShowFront();
    }

    public void Unflip()
    {
        isFlipped = false;
        ShowBack();
    }

    public void ShowFront()
    {
        animator.SetTrigger("FlipToFront");
    }

    public void ShowBack()
    {
        animator.SetTrigger("FlipToBack");
    }
    public void Match()
    {
        animator.SetTrigger("Match");
        AudioManager.Instance.PlayMatch();

    }
    public void MisMatch()
    {
        animator.SetTrigger("MisMatch");
        AudioManager.Instance.PlayMismatch();
        ShowBack();
    }
    
}
