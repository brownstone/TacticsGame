using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour {
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
    private int _myTurnIndex = 0;

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
    };

    private MoveResult _moveResult = new MoveResult();


	private void Start () {
        Instance = this;
        Invoke("GamePrepare", 1.0f);
        _player.userName = "brown";
		PieceMans = new PieceMan[12,9];
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
        } else
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
	private void Update () {
        UpdateSelection();
        DrawBoardLine();

        if (Input.GetMouseButtonDown(0))
        {
			Debug.Log("mouse");
            if (_selectionX >= 0 && _selectionY >= 0)
            {
                if (_selectedPiece == null)
                {
                    // select
                    SelectPiece(_selectionX, _selectionY);
                }
                else
                {
					Debug.Log("move piece");
                    // move
                    MovePiece(_selectionX, _selectionY);
                }
            }
        }
	}

    private void SelectPiece(int x, int y)
    {
        // my turn?
        if (_player.userIndex != _turnIndex)
            return;

        if (PieceMans[x, y] == null)
            return;

        PieceMan pm = PieceMans[x, y];
        // if (pm._color != _player.userIndex)
            // return;
		Debug.Log("3");

        if (pm.CanMove() == false)
        {
            Debug.Log("Can not move piece");
            return;
        }

		Debug.Log("Select success");
        _selectedPiece = pm;
    }

    private void MovePiece(int toX, int toY)
    {
        int fromX = _selectedPiece._currentX;
        int fromY = _selectedPiece._currentY;

        if (IsMovePossible(fromX, fromY, toX, toY) == false)
        {
            return;
        }

        _selectedPiece = null;
		
		Debug.Log("Move to");
		Debug.Log(toX);
		Debug.Log(toY);

        NetworkManager.Instance.SendMovePiece(fromX, fromY, toX, toY);
        OnMyTurn(false);
    }

    private bool IsMovePossible(int fromX, int fromY, int toX, int toY)
    {
        int moveDistant = Mathf.Abs(toX - fromX + toY - fromY);
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
        if (Physics.Raycast(Camera.current.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("BoardPlane"))) {
            _selectionX = (int)hit.point.x;
            _selectionY = (int)hit.point.z;
        } else
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
		Debug.Log("MovePiece");
        PieceMan pm = PieceMans[_moveResult.fromX, _moveResult.fromY];
        if (pm == null)
        {
            Debug.Log("Invalid Piece");
            return;
        }

        if (PieceMans[_moveResult.toX, _moveResult.toY] != null)
        {
			Debug.Log(_moveResult.toX);
			Debug.Log(_moveResult.toY);
			
            Debug.Log("Target cell must be empty");
            return;
        }

        PieceMans[_moveResult.fromX, _moveResult.fromY] = null;
        pm.transform.position = GetPiecePos(_moveResult.toX, _moveResult.toY);
        pm._currentX = _moveResult.toX;
        pm._currentY = _moveResult.toY;
        PieceMans[_moveResult.toX, _moveResult.toY] = pm;
    }

    public void OnReceiveTurnOver(int turnIndex)
    {
        _turnIndex = turnIndex;

        bool turn = (_player.userIndex == _turnIndex);
        OnMyTurn(turn);
        turn = (_opponent.userIndex == _turnIndex);
        OnOpponentTurn(turn);
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

    public void OnReceiveMoveResult(int color, bool move, int fromX, int fromY, int toX, int toY, bool battle, bool win)
    {
		Debug.Log("OnReceiveMoveResult");
		
        PieceMan pm = PieceMans[fromX, fromY];
        if (pm != null)
        {
            if (pm._color != color)
            {
                Debug.Log("Invalid color");
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
                Invoke("RemovePiece", 0.5f);
                Invoke("MovePiece", 0.7f);
            }
            else
            {
                // nothing
                Invoke("RemovePiece", 0.5f);
            }
        }
        else
        {
            if (move)
            {
                Invoke("MovePiece", 0.5f);
            }
        }
    }
}
