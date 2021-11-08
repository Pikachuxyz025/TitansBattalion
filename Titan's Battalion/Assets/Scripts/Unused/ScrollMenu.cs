using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollMenu : MonoBehaviour
{
    public GameObject horizontalscrollbar, verticalscrollbar;
    public Transform verticalContent, horizontalContent;
    float scroll_pos = 0, Vscroll_pos = 0;
    float[] pos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HorizontalScrollBar();
    }

    void HorizontalScrollBar()
    {
        pos = new float[horizontalContent.childCount];
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
        if (Input.GetMouseButton(0))
        {
            scroll_pos = horizontalscrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
                {
                    horizontalscrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(horizontalscrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }
        for (int i = 0; i < pos.Length; i++)
        {
            if (scroll_pos < pos[i] + (distance / 2) && scroll_pos > pos[i] - (distance / 2))
            {
                horizontalContent.GetChild(i).localScale = Vector2.Lerp(horizontalContent.GetChild(i).localScale, new Vector2(1f, 1f), .1f);
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        horizontalContent.GetChild(a).localScale = Vector2.Lerp(horizontalContent.GetChild(a).localScale, new Vector2(.8f, .8f), .1f);
                    }
                }
            }
        }
    }
    void VerticalScrollBar()
    {
        pos = new float[verticalContent.childCount];
        float distance = 1f / (pos.Length - 1f);
        for (int i = 0; i < pos.Length; i++)
        {
            pos[i] = distance * i;
        }
        if (Input.GetMouseButton(0))
        {
            Vscroll_pos = verticalscrollbar.GetComponent<Scrollbar>().value;
        }
        else
        {
            for (int i = 0; i < pos.Length; i++)
            {
                if (Vscroll_pos < pos[i] + (distance / 2) && Vscroll_pos > pos[i] - (distance / 2))
                {
                    verticalscrollbar.GetComponent<Scrollbar>().value = Mathf.Lerp(verticalscrollbar.GetComponent<Scrollbar>().value, pos[i], 0.1f);
                }
            }
        }
        for (int i = 0; i < pos.Length; i++)
        {
            if (Vscroll_pos < pos[i] + (distance / 2) && Vscroll_pos > pos[i] - (distance / 2))
            {
                verticalContent.GetChild(i).localScale = Vector2.Lerp(verticalContent.GetChild(i).localScale, new Vector2(1f, 1f), .1f);
                for (int a = 0; a < pos.Length; a++)
                {
                    if (a != i)
                    {
                        verticalContent.GetChild(a).localScale = Vector2.Lerp(verticalContent.GetChild(a).localScale, new Vector2(.8f, .8f), .1f);
                    }
                }
            }
        }
    }
}
