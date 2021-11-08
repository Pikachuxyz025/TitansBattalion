using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MetricGameManager : MonoBehaviour
{
    public static UnityEvent M_vp;
    public SID_Chessman_Mirror chessmanTesting;
    public bool highlightOn;
    private void OnEnable()
    {
        Debug.Log("ready");
        if (M_vp == null)
            M_vp = new UnityEvent();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            M_vp.Invoke();

        if (Input.GetKeyDown(KeyCode.H) && !highlightOn)
        {
            Debug.Log("light on");
            SID_BoardHighlight_Mirror.Instance.HighLightAllowedMove(chessmanTesting.futureMoves);
            highlightOn = !highlightOn;
        }

        if (Input.GetKeyDown(KeyCode.G) && !highlightOn)
        {
            Debug.Log("light on");
            SID_BoardHighlight_Mirror.Instance.HighLightAllowedMove(chessmanTesting.confirmedMoves);
            highlightOn = !highlightOn;
        }
        else if (Input.GetKeyDown(KeyCode.J) && highlightOn)
        {
            Debug.Log("light off");
            SID_BoardHighlight_Mirror.Instance.HideHighlights();
            highlightOn = !highlightOn;
        }
    }
}
