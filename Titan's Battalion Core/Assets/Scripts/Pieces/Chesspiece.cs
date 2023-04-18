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
    protected Player controllingPlayer;
    protected CheckableList playersCheckableList;
    public int team;
    public bool isKing, hasMoved, isPawn;
    public int currentX { get; private set; } = 0;
    public int currentY { get; private set; } = 0;

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

    [SerializeField] protected NetworkVariable<GameMode> currentGameMode = new NetworkVariable<GameMode>(GameMode.None);

    public SpecialMove specialMove = SpecialMove.None;

    private void Awake()
    {
        netCurrentX.OnValueChanged += ChangeX;
        netCurrentY.OnValueChanged += ChangeY;
        chessManager = ChessPieceManager.instance;
    }

    public Points GetCurrentPosition()
    {
        return new Points(currentX, currentY);
    }

    public void SetNewCurrentPoints(int x, int y)
    {
        currentX = x;
        currentY = y;
    }
    public void SetGameMode(GameMode newMode) => currentGameMode.Value = newMode;

   protected bool CurrentPieceCheck(Points p)
    {
        return (chessManager.GetChesspieceConnection(p).IsInCheck(team));
    }

    private void ChangeY(int previousValue, int newValue)
    {
        currentY = newValue;
    }

    private void ChangeX(int previousValue, int newValue)
    {
        currentX = newValue;
    }

    public void SetupPiece(int teamNum, Player player)
    {
        team = teamNum;
        controllingPlayer = player;
        playersCheckableList = player.playerCheckableList;
        ChessPieceManager.instance.activeChesspieces.Add(this);
    }

    public bool IsInCheck()
    {
        bool b = false;
        if (chessManager == null)
            return b;
        
        ChessPieceConnection conn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
        if (conn == null)
            return b;

        b = conn.IsInCheck(team);
        return b;
    }
    public bool IsInCheck(out List<Chesspiece> dangerousChesspieces)
    {
        dangerousChesspieces = new List<Chesspiece>();
        bool b = false;
        if (chessManager == null)
            return b;

        ChessPieceConnection conn = chessManager.GetChesspieceConnection(new Points(currentX, currentY));
        if (conn == null)
            return b;

        b = conn.IsInCheck(team);
        dangerousChesspieces.AddRange(conn.piecesThatHaveUsInCheck);
        return b;
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
    public void SetPositionServerRpc(int x, int y, Vector3 position, bool force = false)
    {
        netCurrentX.Value = x;
        netCurrentY.Value = y;
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
        ChessPieceManager che = ChessPieceManager.instance;
        if (che.activeChesspieces.Contains(this))
            che.activeChesspieces.Remove(this);
    }

    protected void AddInCheck(Points p)
    {
        if (!chessManager.GetChesspieceConnection(p).piecesThatHaveUsInCheck.Contains(this))
            chessManager.GetChesspieceConnection(p).piecesThatHaveUsInCheck.Add(this);
    }

    protected void AddInCheck(List<Points> newPoints)
    {
        foreach (Points newPoint in newPoints)
        {
            if (!chessManager.GetChesspieceConnection(newPoint).piecesThatHaveUsInCheck.Contains(this))
                chessManager.GetChesspieceConnection(newPoint).piecesThatHaveUsInCheck.Add(this);
        }
    }

    public bool IsThisTheControllingPlayer(Player player)
    {
        return controllingPlayer == player;
    }

    public virtual List<Points> GetAvailableMoves()
    {
        List<Points> newMoves = new List<Points>();

        switch (type)
        {
            case SetType.Manual:

                for (int i = 0; i < addedPoints.Count; i++)
                {
                    Points addedPoint = new Points(currentX + addedPoints[i].X, currentY + addedPoints[i].Y);

                    if (!ChessPieceManager.instance.IsCoordinateInList(addedPoint))
                        continue;
                    if (chessManager.IsOccupied(addedPoint))
                        if (chessManager.GetOccupiedPiece(addedPoint).team == team)
                            continue;

                    AddInCheck(addedPoint);
                    newMoves.Add(addedPoint);
                }
                break;
            case SetType.Open:
                if (allStraight)
                {
                    // UP
                    AddInCheck(NextPoint(false, 1));
                    newMoves.AddRange(NextPoint(false, 1));

                    //Down
                    AddInCheck(NextPoint(false, -1));
                    newMoves.AddRange(NextPoint(false, -1));

                    //Left
                    AddInCheck(NextPoint(true, -1));
                    newMoves.AddRange(NextPoint(true, -1));

                    //Right                  
                    AddInCheck(NextPoint(true, 1));
                    newMoves.AddRange(NextPoint(true, 1));
                }
                if (allDiagonal)
                {
                    AddInCheck(NextPoint(1, 1));
                    newMoves.AddRange(NextPoint(1, 1));
                    /*for (int x = currentX + 1, y = currentY + 1; x > currentX && y > currentY; x++, y++)
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
                    }*/
                    AddInCheck(NextPoint(-1, 1));
                    newMoves.AddRange(NextPoint(-1, 1));
                    /*for (int x = currentX - 1, y = currentY + 1; x < currentX && y > currentY; x--, y++)
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
                    }*/
                    AddInCheck(NextPoint(-1, -1));
                    newMoves.AddRange(NextPoint(-1, -1));
                    /*for (int x = currentX - 1, y = currentY - 1; x < currentX && y < currentY; x--, y--)
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
                    }*/
                    AddInCheck(NextPoint(1, -1));
                    newMoves.AddRange(NextPoint(1, -1));
                    /*for (int x = currentX + 1, y = currentY - 1; x > currentX && y < currentY; x++, y--)
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

                    }*/
                }
                break;
        }
        return newMoves;
    }

    protected List<Points> NextPoint(bool XIsTrueYIsFalse, int pointOffset)
    {
        List<Points> resultingPoints = new List<Points>();
        int combinedOffset = XIsTrueYIsFalse ? currentX + pointOffset : currentY + pointOffset;
        int currentlyXorY = XIsTrueYIsFalse ? currentX : currentY;
        bool isPositiveOrNegative = pointOffset > 0 ? true : false;
        for (int i = combinedOffset; isPositiveOrNegative ? i > currentlyXorY : i < currentlyXorY; i += pointOffset)
        {
            Points nextPoint = XIsTrueYIsFalse ? new Points(i, currentY) : new Points(currentX, i);
            if (!chessManager.IsCoordinateInList(nextPoint))
                break;

            if (chessManager.IsOccupied(nextPoint))
            {
                if (chessManager.GetOccupiedPiece(nextPoint).team != team)
                    resultingPoints.Add(nextPoint);
                break;
            }
            resultingPoints.Add(nextPoint);
        }
        return resultingPoints;
    }

    protected List<Points> NextPoint(int pointOffsetX, int pointOffsetY)
    {
        List<Points> resultingPoints = new List<Points>();
        int combinedOffsetX = currentX + pointOffsetX;
        int combinedOffsetY = currentY + pointOffsetX;
        bool isXPositiveOrNegative = pointOffsetX > 0 ? true : false;
        bool isYPositiveOrNegative = pointOffsetY > 0 ? true : false;
        for (int x = combinedOffsetX, y = combinedOffsetY; isXPositiveOrNegative ? x > currentX : x < currentX && isYPositiveOrNegative ? y > currentY : y < currentY; x += pointOffsetX, y += pointOffsetY)
        {
            Points nextPoint = new Points(x, y);
            if (!chessManager.IsCoordinateInList(nextPoint))
                break;

            if (chessManager.IsOccupied(nextPoint))
            {
                if (chessManager.GetOccupiedPiece(nextPoint).team != team)
                    resultingPoints.Add(nextPoint);
                break;
            }
            resultingPoints.Add(nextPoint);
        }
        return resultingPoints;
    }
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
