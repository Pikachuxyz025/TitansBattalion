using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class SID_BoardHighlight_Mirror : NetworkBehaviour
{
    public static SID_BoardHighlight_Mirror Instance;
    public SID_BoardManager_Mirror Sid;
    public GameObject highlightPrefab;
    public List<GameObject> highlights;
    public PlayerInfo player;
    private const float TileSize = 1.0f, TileOffset = 0.5f;
    Quaternion nine = Quaternion.Euler(0, 90, 0);
    private void Awake()
    {
        Instance = this;
        highlights = new List<GameObject>();
        Debug.Log("Voice");
    }

    private GameObject GetHighlighObject()
    {
        GameObject go = highlights.Find(g => !g.activeSelf);
        if (go == null)
        {
            go = Instantiate(highlightPrefab);
            NetworkServer.Spawn(go);
            RpcSetupnAdd(go);
        }
        Debug.Log(go == null);
        return go;
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
                //GameObject go = GetHighlighObject();
                ////Debug.Log(go == null);
                //go.SetActive(true);
                //go.transform.rotation = nine;
                //go.transform.position = GetTileCenter(point.X, point.Y);
                //if (isServer)
                //    RpcSetnGo(go);
                CmdSetnSpawn(GetTileCenter(point.X, point.Y));
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
