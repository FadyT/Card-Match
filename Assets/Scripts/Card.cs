using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public CardData data;
    public GameObject front;
    public GameObject back;

    public bool isFlipped = false;
    Animator animator;


    void Start()
    {
        animator = GetComponent<Animator>();
        front.GetComponent<Image>().sprite = data.image;
    }

    public void OnClick()
    {
        if (isFlipped ) return;

        Flip();
    }

    public void Flip()
    {
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

    public void ShowBack() {
        animator.SetTrigger("FlipToBack");
    }
}
