using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { set; get; }

    // Use this for initialization
    public void Start()
    {
        Debug.Log("Init");
        Instance = this;

    }

    public void SetTest()
    {
        Debug.Log("asdfasdf");
    }

    public void SendMoveResult(int color, bool move, int fromX, int fromY, int toX, int toY, bool battle, bool win)
    {
        BoardManager.Instance.OnReceiveMoveResult(color, move, fromX, fromY, toX, toY, battle, win);
    }

    public void SendMovePiece(int userIndex, int fromX, int fromY, int toX, int toY)
    {
        LogicManager.Instance.OnReceiveMovePiece(userIndex, fromX, fromY, toX, toY);
    }

    public void SendPieceInfo(string packet)
    {
        LogicManager.Instance.OnReceivePieceInfo(packet);
    }

    public void SendSpawnPiece(int x, int y, int mainType, int subType, int color)
    {
        BoardManager.Instance.OnReceiveSpawnPiece(x, y, mainType, subType, color);
    }

    public void SendUserInfo(string packet)
    {
        BoardManager.Instance.OnReceiveUserInfo(packet);
    }

    public void SendTurnOver(int turnIndex)
    {
        BoardManager.Instance.OnReceiveTurnOver(turnIndex);
    }

    public void SendGameResult(int winnerIndex)
    {
        BoardManager.Instance.OnReceiveGameResult(winnerIndex);
    }
    public void SendSpecialAbility(int special, int x, int y, int mainType, int subType)
    {
        BoardManager.Instance.OnReceiveSpecialAbility(special, x, y, mainType, subType);
    }
}