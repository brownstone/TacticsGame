using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }

    public PieceMan[,] PieceMans { set; get; }
    private PieceMan _selectedPiece;

    private const int MAX_BOARD_X = 12;
    private const int MAX_BOARD_Y = 9;
    private int _selectionX;
    private int _selectionY;

    // s1, s2, s3, s4, v1, v2, v3, d1, d2, d3, o1, o2, o3,
    // g1, g2, g3, g4,
    // killer, cleaner, spy, gas, meta, center

    public List<GameObject> _redPiecePrefabs;
    public List<GameObject> _bluePiecePrefabs;

    public GameObject _piecePrefab;

    private List<GameObject> _activePieces = new List<GameObject>();

    private int _turnIndex = 0;

    struct UserInfo
    {
        public string userName;
        public int userIndex;
    }

    private UserInfo _player;
    private UserInfo _opponent;

    public struct MoveResult
    {
        public int color;
        public bool move;
        public int fromX;
        public int fromY;
        public int toX;
        public int toY;
        public bool battle;
        public bool win;

        public void Clear()
        {
            color = 0;
            move = false;
            fromX = 0;
            fromY = 0;
            toX = 0;
            toY = 0;
            battle = false;
            win = false;
        }
    }

    private MoveResult _moveResult = new MoveResult();

    public struct MetaAbility
    {
        public int x;
        public int y;
        public int mainType;
        public int subType;

        public void Clear()
        {
            x = 0;
            y = 0;
            mainType = 0;
            subType = 0;
        }
    }

    private MetaAbility _metaAbility = new MetaAbility();


    private void Start()
    {
        Instance = this;
        Invoke("GamePrepare", 0.5f);
        _player.userName = "brown";
        PieceMans = new PieceMan[12, 9];
    }

    private void GamePrepare()
    {
        string packet;
        packet = _player.userName + "|";
        //                 x|y|mt|st|
        packet = packet + "0|1|1|0|" + "1|1|1|1|";
        packet = packet + "2|1|1|2|" + "3|1|1|3|";
        packet = packet + "4|1|1|4|" + "5|1|1|5|";
        packet = packet + "6|1|1|6|" + "7|1|1|7|";
        packet = packet + "8|1|1|8|" + "9|1|1|9|";
        packet = packet + "10|1|1|10|" + "11|1|1|11|";
        packet = packet + "0|0|1|12|" + "1|0|1|13|";
        packet = packet + "2|0|1|14|" + "3|0|1|15|";
        packet = packet + "4|0|1|16|" + "5|0|2|0|";
        packet = packet + "6|0|2|1|" + "7|0|2|2|";
        packet = packet + "8|0|2|2|" + "9|0|2|3|";
        packet = packet + "10|0|2|3|" + "11|0|2|4|";
        packet = packet + "4|2|2|4|" + "5|2|2|5|";
        NetworkManager.Instance.SendPieceInfo(packet);
    }

    private void SpawnAllPieces()
    {
        SpawnRedPieces();
        SpawnBluePieces();
    }

    private void SpawnRedPieces()
    {
        SpawnPiece(0, GetPiecePos(0, 0), true);
        SpawnPiece(1, GetPiecePos(1, 0), true);
        SpawnPiece(2, GetPiecePos(2, 0), true);
        SpawnPiece(3, GetPiecePos(3, 0), true);
    }

    private void SpawnBluePieces()
    {
        SpawnPiece(0, GetPiecePos(0, 8), false);
        SpawnPiece(1, GetPiecePos(1, 8), false);
        SpawnPiece(2, GetPiecePos(2, 8), false);
        SpawnPiece(3, GetPiecePos(3, 8), false);
    }

    private void SpawnPiece(int index, Vector3 position, bool isRed)
    {
        GameObject go = null;
        if (isRed)
        {
            go = Instantiate(_redPiecePrefabs[index], position, Quaternion.identity) as GameObject;
        }
        else
        {
            go = Instantiate(_bluePiecePrefabs[index], position, Quaternion.identity) as GameObject;
        }

        if (go)
        {
            _activePieces.Add(go);
        }
    }
    public void SpawnPiece(int x, int y, int mainType, int subType, int color)
    {
        GameObject go = Instantiate(_piecePrefab, GetPiecePos(x, y), Quaternion.identity) as GameObject;
        if (go)
        {
            //go.transform.SetParent(transform);
            _activePieces.Add(go);
            PieceMan pm = go.GetComponent<PieceMan>();
            if (pm)
            {
                pm.SetProperty(x, y, color, mainType, subType);
                PieceMans[x, y] = pm;
            }
        }
    }

    private Vector3 GetPiecePos(int x, int y)
    {
        const float TILE_OFFSET = 0.5f;
        Vector3 origin = Vector3.right * x + Vector3.forward * y;

        origin.x += TILE_OFFSET;
        origin.y = 0.3f;
        origin.z += TILE_OFFSET;

        return origin;
    }

    public void OnReceiveSpawnPiece(int x, int y, int mainType, int subType, int color)
    {
        SpawnPiece(x, y, mainType, subType, color);
    }

    // Update is called once per frame
    private void Update()
    {
        UpdateSelection();
        DrawBoardLine();

        if (Input.GetMouseButtonDown(0))
        {
            if (_selectionX >= 0 && _selectionY >= 0)
            {
                if (_selectedPiece == null)
                {
                    // select
                    TrySelectPiece(_selectionX, _selectionY);
                }
                else
                {
                    // move
                    TryMovePiece(_selectionX, _selectionY);
                }
            }
        }
    }

    private void TrySelectPiece(int x, int y)
    {
        // my turn?
        if (_player.userIndex != _turnIndex)
            return;

        if (PieceMans[x, y] == null)
            return;

        PieceMan pm = PieceMans[x, y];
        if (pm._color != _player.userIndex)
            return;

        if (pm.CanMove() == false)
        {
            Debug.Log("Can not move piece");
            return;
        }

        bool[,] allowedMoves = new bool[12, 9];
        if (x - 1 >= 0)
        {
            PieceMan p = PieceMans[x - 1, y];
            if (p == null)
            {
                allowedMoves[x - 1, y] = true;
            }
            else
            {
                if (p._color != _turnIndex)
                {
                    allowedMoves[x - 1, y] = true;
                }
            }
        }
        if (x + 1 < 12)
        {
            PieceMan p = PieceMans[x + 1, y];
            if (p == null)
            {
                allowedMoves[x + 1, y] = true;
            }
            else
            {
                if (p._color != _turnIndex)
                {
                    allowedMoves[x + 1, y] = true;
                }
            }
        }
        if (y - 1 >= 0)
        {
            PieceMan p = PieceMans[x, y - 1];
            if (p == null)
            {
                allowedMoves[x, y - 1] = true;
            }
            else
            {
                if (p._color != _turnIndex)
                {
                    allowedMoves[x, y - 1] = true;
                }
            }
        }
        if (y + 1 < 9)
        {
            PieceMan p = PieceMans[x, y + 1];
            if (p == null)
            {
                allowedMoves[x, y + 1] = true;
            }
            else
            {
                if (p._color != _turnIndex)
                {
                    allowedMoves[x, y + 1] = true;
                }
            }
        }

        BoardHighlights.Instance.HighlightAllowedMoves(allowedMoves);


        _selectedPiece = pm;
    }

    private void TryMovePiece(int toX, int toY)
    {
        int fromX = _selectedPiece._currentX;
        int fromY = _selectedPiece._currentY;

        if (fromX == toX && fromY == toY)
        {
            BoardHighlights.Instance.HideHighlights();
            _selectedPiece = null;
            return;
        }

        if (IsMovePossible(fromX, fromY, toX, toY) == false)
        {
            return;
        }

        BoardHighlights.Instance.HideHighlights();
        _selectedPiece = null;

        NetworkManager.Instance.SendMovePiece(_turnIndex, fromX, fromY, toX, toY);
        OnMyTurn(false);
    }

    private bool IsMovePossible(int fromX, int fromY, int toX, int toY)
    {
        int moveDistant = Mathf.Abs(toX - fromX) + Mathf.Abs(toY - fromY);
        if (moveDistant != 1)
            return false;

        PieceMan pm = PieceMans[toX, toY];
        if (pm != null)
        {
            if (pm._color == _player.userIndex)
                return false;
        }

        // is moving piece?

        return true;
    }

    private void UpdateSelection()
    {
        if (!Camera.current)
            return;
        RaycastHit hit;
        if (Physics.Raycast(Camera.current.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane")))
        {
            _selectionX = (int)hit.point.x;
            _selectionY = (int)hit.point.z;
        }
        else
        {
            _selectionX = -1;
            _selectionY = -1;
        }
    }

    private void DrawBoardLine()
    {
        Vector3 boardWidth = Vector3.right * MAX_BOARD_X;
        Vector3 boardHeight = Vector3.forward * MAX_BOARD_Y;

        for (int i = 0; i < MAX_BOARD_X + 1; ++i)
        {
            Vector3 start = Vector3.right * i;
            Debug.DrawLine(start, start + boardHeight);
        }
        for (int i = 0; i < MAX_BOARD_Y + 1; ++i)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start + boardWidth);
        }

        //Draw the selection
        if (_selectionX > -1 && _selectionY > -1)
        {
            Debug.DrawLine(
                Vector3.right * _selectionX + Vector3.forward * _selectionY,
                Vector3.right * (_selectionX + 1) + Vector3.forward * (_selectionY + 1));
            Debug.DrawLine(
                Vector3.right * _selectionX + Vector3.forward * (_selectionY + 1),
                Vector3.right * (_selectionX + 1) + Vector3.forward * _selectionY);
        }

    }

    public void SendSpawnPiece(int x, int y, int mainType, int subType, int color)
    {
        SpawnPiece(x, y, mainType, subType, color);
    }

    private void BattleStart()
    {
        Debug.Log("Battle Start");
    }
    private void RemovePiece()
    {
        int x = -1;
        int y = -1;
        if (_moveResult.battle)
        {
            if (_moveResult.win)
            {
                x = _moveResult.toX;
                y = _moveResult.toY;
            }
            else
            {
                x = _moveResult.fromX;
                y = _moveResult.fromY;
            }
        }

        if (x < 0 || y < 0)
            return;

        PieceMan pm = PieceMans[x, y];

        if (pm == null)
        {
            Debug.Log("Invalid Piece");
            return;
        }

        _activePieces.Remove(pm.gameObject);
        Destroy(pm.gameObject);
        PieceMans[x, y] = null;
    }

    private void MovePiece()
    {
        PieceMan pm = PieceMans[_moveResult.fromX, _moveResult.fromY];
        if (pm == null)
        {
            Debug.Log("Invalid Piece");
            return;
        }

        if (PieceMans[_moveResult.toX, _moveResult.toY] != null)
        {
            Debug.Log("Target cell must be empty");
            return;
        }

        PieceMans[_moveResult.fromX, _moveResult.fromY] = null;
        pm.transform.position = GetPiecePos(_moveResult.toX, _moveResult.toY);
        pm._currentX = _moveResult.toX;
        pm._currentY = _moveResult.toY;
        PieceMans[_moveResult.toX, _moveResult.toY] = pm;
    }

    private void ChangePiece()
    {
        Debug.Log("Change Piece");

        PieceMan pm = PieceMans[_metaAbility.x, _metaAbility.y];
        if (pm == null)
        {
            Debug.Log("Invalid Piece");
            return;
        }

        if (pm._color != _player.userIndex)
        {
            Debug.Log("Invalid Piece user index");
            Debug.Log(pm._color);
            Debug.Log(_player.userIndex);
            return;
        }

        //pm.SetProperty(x, y, color, mainType, subType);
        pm.SetProperty(pm._currentX, pm._currentY, pm._color, _metaAbility.mainType, _metaAbility.subType);
    }

    public void OnReceiveTurnOver(int turnIndex)
    {
        _turnIndex = turnIndex;

        bool turn = (_player.userIndex == _turnIndex);
        OnMyTurn(turn);
        turn = (_opponent.userIndex == _turnIndex);
        OnOpponentTurn(turn);

        _moveResult.Clear();
        _metaAbility.Clear();
    }

    private void OnMyTurn(bool on)
    {
        if (on)
            Debug.Log("My turn");
    }

    private void OnOpponentTurn(bool on)
    {
        if (on)
            Debug.Log("Opponent turn");
    }

    public void OnReceiveUserInfo(string userInfos)
    {
        Debug.Log("OnReceiveUserInfo");
        string[] aData = userInfos.Split('|');

        if (aData.Length != 5)
        {
            Debug.Log(aData.Length);
            return;
        }

        if (aData[0] == _player.userName)
        {
            _player.userIndex = int.Parse(aData[1]);
            _opponent.userName = aData[2];
            _opponent.userIndex = int.Parse(aData[3]);
        }
        else
        {
            _opponent.userName = aData[0];
            _opponent.userIndex = int.Parse(aData[1]);
            _player.userIndex = int.Parse(aData[3]);
        }
    }

    public void OnReceiveGameResult(int winnerIndex)
    {
        if (_player.userIndex == winnerIndex)
        {
            Debug.Log("You win");
        }

        if (_opponent.userIndex == winnerIndex)
        {
            Debug.Log("You Lose");
        }

        if (winnerIndex == 0)
        {
            Debug.Log("Draw");
        }
    }

    public void OnReceiveSpecialAbility(int special, int x, int y, int mainType, int subType)
    {
        if (special == 1)
        {
            Debug.Log("Spy");
        }
        if (special == 2)
        {
            Debug.Log("Meta");
            _metaAbility.x = x;
            _metaAbility.y = y;
            _metaAbility.mainType = mainType;
            _metaAbility.subType = subType;
            if (_moveResult.win)
                Invoke("ChangePiece", 0.6f);
            else
                Invoke("ChangePiece", 0.4f);
        }
    }

    public void OnReceiveMoveResult(int color, bool move, int fromX, int fromY, int toX, int toY, bool battle, bool win)
    {
        PieceMan pm = PieceMans[fromX, fromY];
        if (pm != null)
        {
            if (pm._color != color)
            {
                Debug.Log("Invalid color");
                Debug.Log(pm._color);
                Debug.Log(color);
                return;
            }
        }

        _moveResult.color = color;
        _moveResult.move = move;
        _moveResult.fromX = fromX;
        _moveResult.fromY = fromY;
        _moveResult.toX = toX;
        _moveResult.toY = toY;
        _moveResult.battle = battle;
        _moveResult.win = win;

        if (battle)
        {
            Invoke("BattleStart", 0);
            if (win)
            {
                Invoke("RemovePiece", 0.3f);
                Invoke("MovePiece", 0.5f);
            }
            else
            {
                // nothing
                Invoke("RemovePiece", 0.2f);
            }
        }
        else
        {
            if (move)
            {
                Invoke("MovePiece", 0.1f);
            }
        }
    }
}