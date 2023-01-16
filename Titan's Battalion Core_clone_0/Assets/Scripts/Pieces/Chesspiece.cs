using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum SetType
{
    Open,
    Manual,
    ScriptedCustom
}

public enum SpecialMove
{
    None,
    EnPassant,
    Castling,
    Promotion
}

public class Chesspiece : NetworkBehaviour
{
    public int team;
    public bool isKing, hasMoved,isPawn;

    [Header("Location")]
    public int currentX = 0;
    public int currentY = 0;

    NetworkVariable<int> netCurrentX = new NetworkVariable<int>(0);
    NetworkVariable<int> netCurrentY = new NetworkVariable<int>(0);

    private Vector3 desiredPosition;
    private Vector3 desiredScale = Vector3.one;

   [SerializeField] protected ChessPieceManager chessManager;

    [HideInInspector] public SetType type;

    [HideInInspector] public bool allDiagonal;
    [HideInInspector] public bool allStraight;

    //[SerializeField] protected bool isFirstMove;
    [HideInInspector] public List<Points> addedPoints;
    [SerializeField] protected List<Points> specialPoints = new List<Points>();
    [SerializeField] protected List<Points> firstMovePoints = new List<Points>();
    public SpecialMove specialMove = SpecialMove.None;

    private void Awake()
    {
        netCurrentX.OnValueChanged += ChangeX;
        netCurrentY.OnValueChanged += ChangeY;
        chessManager = ChessPieceManager.instance;
    }

    private void ChangeY(int previousValue, int newValue)
    {
        currentY = newValue;
    }

    private void ChangeX(int previousValue, int newValue)
    {
        currentX = newValue;
    }

    [ClientRpc]
    public void SetupPieceClientRpc(int teamNum)
    {
        team = teamNum;
    } 

    public void AddActivePiece()
    {
        ChessPieceManager.instance.activeChesspieces.Add(this);
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * 10);
        transform.localScale = Vector3.Lerp(transform.localScale, desiredScale, Time.deltaTime * 10);
    }

    public virtual List<Points> GetSpecialMoves()
    {
       List<Points> result = new List<Points>();
        return result;
    }
    [ServerRpc]
    public void SetPositionServerRpc(int x,int y ,Vector3 position, bool force = false)
    {
        netCurrentX.Value= x;
        netCurrentY.Value= y;
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }

    [ServerRpc(RequireOwnership =false)]
    public void ReturnPositionServerRpc(Vector3 position, bool force = false)
    {
        desiredPosition = position;
        if (force)
            transform.position = desiredPosition;
    }


    public override void OnDestroy()
    {
        ChessPieceManager che=ChessPieceManager.instance;
        if(che.activeChesspieces.Contains(this))
         che.activeChesspieces.Remove(this); 
    }

    protected void AddInCheck(Points p)
    {
        if (!chessManager.GetChesspieceConnection(p).inCheck.Contains(team))
            chessManager.GetChesspieceConnection(p).inCheck.Add(team);
    }


    public virtual List<Points> GetAvailableMoves()
    {
        List<Points> newMoves = new List<Points>();
;
        switch (type)
        { 
            case SetType.Manual:

                for (int i = 0; i < addedPoints.Count; i++)
                {
                    Points c = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);

                    if (!ChessPieceManager.instance.IsCoordinateInList(c))
                        continue;
                    if (chessManager.IsOccupied(c))
                        if (chessManager.GetOccupiedPiece(c).team == team)
                            continue;

                    AddInCheck(c);
                    newMoves.Add(c);
                }
                break;
            case SetType.Open:
                if (allStraight)
                {
                    // UP
                    for (int i = currentY + 1; i > currentY; i++)
                    {
                        Points p = new Points(currentX, i);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    //Down
                    for (int i = currentY - 1; i < currentY; i--)
                    {
                        Points p = new Points(currentX, i);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    //Left
                    for (int i = currentX - 1; i < currentX; i--)
                    {
                        Points p = new Points(i, currentY);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    //Right
                    for (int i = currentX + 1; i > currentX; i++)
                    {
                        Points p = new Points(i, currentY);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }
                }
                if (allDiagonal)
                {

                    for (int x = currentX + 1, y = currentY + 1; x > currentX && y > currentY; x++, y++)
                    {
                        Points p = new Points(x, y);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                                                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    for (int x = currentX - 1, y = currentY + 1; x < currentX && y > currentY; x--, y++)
                    {
                        Points p = new Points(x, y);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                                                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    for (int x = currentX - 1, y = currentY - 1; x < currentX && y < currentY; x--, y--)
                    {
                        Points p = new Points(x, y);
                        //Set(ref newMoves, p);
                        //LocationStatus pieceStatus;
                        //CheckLocationStatus(p, out pieceStatus);

                        //AddPoint(ref newMoves, pieceStatus, p);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);
                    }

                    for (int x = currentX + 1, y = currentY - 1; x > currentX && y < currentY; x++, y--)
                    {
                        Points p = new Points(x, y);
                        //Set(ref newMoves, p);
                        //LocationStatus pieceStatus;
                        //CheckLocationStatus(p, out pieceStatus);

                        //AddPoint(ref newMoves, pieceStatus, p);
                        if (!chessManager.IsCoordinateInList(p))
                            break;
                        if (chessManager.IsOccupied(p))
                        {
                            if (chessManager.GetOccupiedPiece(p).team != team)
                            {
                                AddInCheck(p);
                                newMoves.Add(p);
                            }
                            break;
                        }
                        AddInCheck(p);
                        newMoves.Add(p);

                    }
                }        
        break;
        }

        #region If Statement SetType
        /*if (type == SetType.Manual)
        {
            for (int i = 0; i < addedPoints.Count; i++)
            {
                Points c = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);
                if (ChessPieceManager.instance.IsCoordinateInList(c))
                    newMoves.Add(c);
            }
        }
        else {
            if (allStraight)
            {
                // UP
                for (int i = currentY + 1; i > currentY; i++)
                {
                    Points p = new Points(currentX, i);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(new Points(currentX, i));
                        break;
                    }
                    newMoves.Add(p);
                }

                //Down
                for (int i = currentY - 1; i < currentY; i--)
                {
                    Points p = new Points(currentX, i);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(new Points(currentX, i));
                        break;
                    }
                    newMoves.Add(p);
                }

                //Left
                for (int i = currentX - 1; i < currentX; i--)
                {
                    Points p = new Points(i,currentY);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(new Points(currentX, i));
                        break;
                    }
                    newMoves.Add(p);
                }

                //Right
                for (int i = currentX + 1; i > currentX; i++)
                {
                    Points p = new Points(i, currentY);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(new Points(i, currentY));
                        break;
                    }
                    newMoves.Add(p);
                }
            }
            if (allDiagonal)
            {

                for (int x = currentX + 1, y = currentY + 1; x > currentX && y > currentY; x++, y++)
                {
                    Points p = new Points(x,y);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(p);
                        break;
                    }
                    newMoves.Add(p);
                }

                for (int x = currentX - 1, y = currentY + 1; x < currentX && y > currentY; x--, y++)
                {
                    Points p = new Points(x, y);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(p);
                        break;
                    }
                    newMoves.Add(p);
                }

                for (int x = currentX - 1, y = currentY - 1; x < currentX && y < currentY; x--, y--)
                {
                    Points p = new Points(x, y);
                    //Set(ref newMoves, p);
                    //LocationStatus pieceStatus;
                    //CheckLocationStatus(p, out pieceStatus);

                    //AddPoint(ref newMoves, pieceStatus, p);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(p);
                        break;
                    }
                    newMoves.Add(p);
                }

                for (int x = currentX + 1, y = currentY - 1; x > currentX && y < currentY; x++, y--)
                {
                    Points p = new Points(x, y);
                    //Set(ref newMoves, p);
                    //LocationStatus pieceStatus;
                    //CheckLocationStatus(p, out pieceStatus);

                    //AddPoint(ref newMoves, pieceStatus, p);
                    if (!chessManager.IsCoordinateInList(p))
                        break;
                    if (chessManager.IsOccupied(p))
                    {
                        if (chessManager.GetOccupiedPiece(p).team != team)
                            newMoves.Add(p);
                        break;
                    }
                    newMoves.Add(p);

                }
            }
        }*/
        #endregion
        return newMoves;
    }

    /*protected void Set(ref List<Points> moves,Points p)
    {
        ChessPieceManager chessManager = ChessPieceManager.instance;
        if (!chessManager.IsCoordinateInList(p))
            return;
        if (chessManager.IsOccupied(p))
        {
            if (chessManager.GetOccupiedPiece(p).team != team)
               moves.Add(p);
            return;
        }
       moves.Add(p);
    }

    protected void AddPoint(ref List<Points> moves,LocationStatus status,Points p)
    {
        if (status == LocationStatus.IsInvalid || status == LocationStatus.HasAlly)
            return;
        if (status == LocationStatus.HasEnemy)
        {
            moves.Add(p);
            return;
        }
        moves.Add(p);
    }

    protected void CheckLocationStatus(Points p, out LocationStatus status)
    {
        ChessPieceManager chessManager = ChessPieceManager.instance;

        if (!chessManager.IsCoordinateInList(p))
        {
            status = LocationStatus.IsInvalid;
            return;
        }

        if (chessManager.IsOccupied(p))
        {
            if (chessManager.GetOccupiedPiece(p).team != team)
            {
                status = LocationStatus.HasEnemy;
                return;
            }
            status = LocationStatus.HasAlly;
            return;
        }
        status = LocationStatus.IsEmpty;
    }*/

    public virtual void SetScale(Vector3 scale, bool force = false)
    {
        desiredScale = scale;
        if (force)
        {
            transform.localScale = desiredScale;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Chesspiece), true)]
public class Chesspiece_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Chesspiece piece = (Chesspiece)target;

        //
        piece.type = (SetType)EditorGUILayout.EnumPopup("Selectable Range", piece.type);
        List<Points> setPoints = piece.addedPoints;
        switch (piece.type)
        {
            case SetType.Manual:
                int size = Mathf.Max(0, EditorGUILayout.DelayedIntField("Size", setPoints.Count));
                while (size > setPoints.Count)
                    setPoints.Add(new Points(0, 0));
                while (size < setPoints.Count)
                    setPoints.RemoveAt(setPoints.Count - 1);


                for (int i = 0; i < setPoints.Count; i++)
                {
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField("Piece " + i + " Out X", GUILayout.MaxWidth(150));
                    setPoints[i].X = EditorGUILayout.IntField(setPoints[i].X);

                    EditorGUILayout.LabelField("Piece " + i + " Out Y", GUILayout.MaxWidth(150));
                    setPoints[i].Y = EditorGUILayout.IntField(setPoints[i].Y);
                    EditorGUILayout.EndHorizontal();
                }

                break;
            case SetType.Open:

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Can Extend Diagonally?", GUILayout.MaxWidth(150));
                piece.allDiagonal = EditorGUILayout.Toggle(piece.allDiagonal);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Can Extend On X and Y?", GUILayout.MaxWidth(150));
                piece.allStraight = EditorGUILayout.Toggle(piece.allStraight);
                EditorGUILayout.EndHorizontal();

                break;
            case SetType.ScriptedCustom:
                break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(piece);
            //EditorSceneManager.MarkSceneDirty(piece.gameObject.scene);
        }
    }
}
#endif
