using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMainBoardInfo
{
    void InsertMainBoardInfo(int x, int y);
    void CreatePieceList(ChessboardTile connection);
}
