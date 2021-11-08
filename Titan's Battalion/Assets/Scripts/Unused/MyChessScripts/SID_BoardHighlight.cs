using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SID_BoardHighlight : MonoBehaviour
{
    public static SID_BoardHighlight Instance;
    SID_BoardManager Sid;
    public GameObject highlightPrefab;
    public List<GameObject> highlights;
    private const float TileSize = 1.0f, TileOffset = 0.5f;
    Quaternion nine = Quaternion.Euler(0, 90, 0);
    private void Start()
    {
        Instance = this;
        highlights = new List<GameObject>();
        Sid = FindObjectOfType<SID_BoardManager>();
    }

    private GameObject GetHighlighObject()
    {
        GameObject go = highlights.Find(g => !g.activeSelf);
        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            highlights.Add(go);
        }
        return go;
    }
    public void HighLightAllowedMoves(Dictionary<Points, bool> moves)
    {
        foreach (Points point in moves.Keys.ToList<Points>())
        {
            if (moves[point])
            {
                GameObject go = GetHighlighObject();
                go.SetActive(true);
                go.transform.rotation = nine;
                go.transform.position = GetTileCenter(point.X, point.Y);
            }
        }
    }
    private Vector3 GetTileCenter(int x, int y)
    {
        Vector3 origin = Sid.originBoardPiece[2].transform.position;
        origin.x += (TileSize * x);
        origin.y += 1.01f;
        origin.z += (TileSize * y);
        return origin;
    }
    public void HideHighlights()
    {
        foreach (GameObject go in highlights)
            go.SetActive(false);
    }
}
