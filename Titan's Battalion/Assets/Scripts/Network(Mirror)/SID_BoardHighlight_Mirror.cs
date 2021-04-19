using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class SID_BoardHighlight_Mirror : NetworkBehaviour
{
    public static SID_BoardHighlight_Mirror Instance;
    public SID_BoardManager_Mirror Sid;
    public Transform originPieceTrans;
    public GameObject highlightPrefab;
    public List<GameObject> highlights;
    private const float TileSize = 1.0f, TileOffset = 0.5f;
    Quaternion nine = Quaternion.Euler(0, 90, 0);
    private void Awake()
    {
        Instance = this;
        highlights = new List<GameObject>();
    }

    private GameObject GetHighlighObject()
    {
        GameObject go = highlights.Find(g => !g.activeSelf);
        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            go.GetComponent<NetworkMatchChecker>().matchId = GetComponent<NetworkMatchChecker>().matchId;
            NetworkServer.Spawn(go);
            RpcSetupnAdd(go);
        }
        return go;
    }

    private GameObject GetHighlighObjects()
    {
        GameObject go = highlights.Find(g => !g.activeSelf);
        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            highlights.Add(go);
        }       
        return go;
    }
    void SetnSpawn(Vector3 varin)
    {
        GameObject go = GetHighlighObjects();
        //Debug.Log(go == null);
        go.SetActive(true);
        go.transform.rotation = nine;
        go.transform.position = varin;
    }

    #region Command

    [Command]
    void CmdSetnGo(GameObject gg, Vector3 voi)
    {
        gg.SetActive(true);
        gg.transform.rotation = nine;
        gg.transform.position = voi;
    }

    [Command]
    void CmdSetnSpawn(Vector3 varin)
    {
        GameObject go = GetHighlighObject();
        //Debug.Log(go == null);
        go.SetActive(true);
        go.transform.rotation = nine;
        go.transform.position = varin;
        if (isServer)
            RpcSetnGo(go);
    }

    [Command]
    void CmdSetupnAdd(GameObject g)
    {
        Debug.Log(g == null);
        RpcSetupnAdd(g);
    } 

    #endregion

    #region ClientRpc

    [ClientRpc]
    void RpcSetnGo(GameObject gg)
    {
        gg.SetActive(true);
    }

    [ClientRpc]
    void RpcSetupnAdd(GameObject g)
    {
        //NetworkServer.Spawn(g);
        highlights.Add(g);
    }

    #endregion

    public void HighLightAllowedMoves(Dictionary<Points, bool> moves)
    {
        foreach (Points point in moves.Keys.ToList<Points>())
        {
            if (moves[point])
            {
                CmdSetnSpawn(GetTileCenter(point.X, point.Y));
            }
        }
    }

    public void HighLightAllowedMove(Dictionary<Points, bool> moves)
    {
        foreach (Points point in moves.Keys.ToList<Points>())
        {
            SetnSpawn(GetTileTestCenter(point.X, point.Y));
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

    private Vector3 GetTileTestCenter(int x, int y)
    {
        Vector3 origin = originPieceTrans.position;
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
