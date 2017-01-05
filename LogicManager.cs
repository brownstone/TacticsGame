using UnityEngine;
using System.Collections;

public class PieceInfo
{
    public int _x;
    public int _y;
    public int _mainType;
    public int _subType;
    public int _color;

    public PieceInfo(int x, int y, int mainType, int subType, int color)
    {
        _x = x;
        _y = y;
        _mainType = mainType;
        _subType = subType;
        _color = color;
    }
}

public struct UserInfo
{
    public string userName;
    public int userId;
    public int userIndex;
}


public struct MoveResult
{
    public int _color;
    public bool _move;
    public int _fromX;
    public int _fromY;
    public int _toX;
    public int _toY;
    public bool _battle;
    public bool _win;
    public int _special; // 0 : noting, 1 : spy, 2 : meta

	
	public void Clear()
	{
        _color = 0;
		_move = false;
		_fromX = 0;
		_fromY = 0;
		_toX = 0;
		_toY = 0;
		_battle = false;
		_win = false;
        _special = 0;
	}
}

public class LogicManager : MonoBehaviour {
    public static LogicManager Instance { set; get; }

    private float _readyElapsed = 0;
    private int _readyCount = 0;
    private float _startElapsed = 0;
    private float _resultElapsed = 0;
	private float _battleElapsed = 0;
    private float _simulationElapsed = 0;
    private bool _battleAction = false;
	
	private PieceInfo[,] _pieceInfos;

    private UserInfo[] _userInfos;
	
    private int _turnIndex = 0; // 0:Noting, 1:red, 2:blue

    enum GameState
    {
        GS_NONE,
        GS_READY,
        GS_START,
        GS_BATTLE,
        GS_BATTLE_SIMULATION,
        GS_RESULT,
        GS_END
    }
    private GameState _gs;


	private void Start ()
    {
        Instance = this;
        _gs = GameState.GS_READY;
        _pieceInfos = new PieceInfo[12, 9];
        _userInfos = new UserInfo[2];
	}

    //private void FillRedColor()
    //{
    //    PieceInfo[] infos = new PieceInfo[]
    //    {
    //        // x, y, mainType, subType, color
    //        new PieceInfo(  0, 1, 1,  0, 1 ), new PieceInfo( 1, 1, 1,  1, 1 ),
    //        new PieceInfo(  2, 1, 1,  2, 1 ), new PieceInfo( 3, 1, 1,  3, 1 ),
    //        new PieceInfo(  4, 1, 1,  4, 1 ), new PieceInfo( 5, 1, 1,  5, 1 ),
    //        new PieceInfo(  6, 1, 1,  6, 1 ), new PieceInfo( 7, 1, 1,  7, 1 ),
    //        new PieceInfo(  8, 1, 1,  8, 1 ), new PieceInfo( 9, 1, 1,  9, 1 ), // 10
    //        new PieceInfo( 10, 1, 1, 10, 1 ), new PieceInfo(11, 1, 1, 11, 1 ),
    //        new PieceInfo(  0, 0, 1, 12, 1 ), new PieceInfo( 1, 0, 1, 13, 1 ),
    //        new PieceInfo(  2, 0, 1, 14, 1 ), new PieceInfo( 3, 0, 1, 15, 1 ),
    //        new PieceInfo(  4, 0, 1, 16, 1 ), new PieceInfo( 5, 0, 2,  0, 1 ),
    //        new PieceInfo(  6, 0, 2,  1, 1 ), new PieceInfo( 7, 0, 2,  2, 1 ), // 20
    //        new PieceInfo(  8, 0, 2,  2, 1 ), new PieceInfo( 9, 0, 2,  3, 1 ),
    //        new PieceInfo( 10, 0, 2,  3, 1 ), new PieceInfo(11, 0, 2,  4, 1 ),
    //        new PieceInfo(  4, 2, 2,  4, 1 ), new PieceInfo( 5, 2, 2,  5, 1 )
    //    };

    //    for (int i = 0; i < infos.Length; i++)
    //    {
    //        if (BoardManager.Instance != null)
    //        {
    //            BoardManager.Instance.SendSpawnPiece(infos[i]._x, infos[i]._y, infos[i]._mainType, infos[i]._subType, infos[i]._color);
    //            _pieceInfos[infos[i]._x, infos[i]._y] = infos[i];
    //        }
    //    }

    //    _readyCount++;
    //}

    //private void FillBlueColor()
    //{
    //    PieceInfo[] infos = new PieceInfo[]
    //    {
    //        new PieceInfo(  0, 7, 1,  0, 2 ), new PieceInfo( 1, 7, 1,  1, 2 ),
    //        new PieceInfo(  2, 7, 1,  2, 2 ), new PieceInfo( 3, 7, 1,  3, 2 ),
    //        new PieceInfo(  4, 7, 1,  4, 2 ), new PieceInfo( 5, 7, 1,  5, 2 ),
    //        new PieceInfo(  6, 7, 1,  6, 2 ), new PieceInfo( 7, 7, 1,  7, 2 ),
    //        new PieceInfo(  8, 7, 1,  8, 2 ), new PieceInfo( 9, 7, 1,  9, 2 ), // 10
    //        new PieceInfo( 10, 7, 1, 10, 2 ), new PieceInfo(11, 7, 1, 11, 2 ),
    //        new PieceInfo(  0, 8, 1, 12, 2 ), new PieceInfo( 1, 8, 1, 13, 2 ),
    //        new PieceInfo(  2, 8, 1, 14, 2 ), new PieceInfo( 3, 8, 1, 15, 2 ),
    //        new PieceInfo(  4, 8, 1, 16, 2 ), new PieceInfo( 5, 8, 2,  0, 2 ),
    //        new PieceInfo(  6, 8, 2,  1, 2 ), new PieceInfo( 7, 8, 2,  2, 2 ), // 20
    //        new PieceInfo(  8, 8, 2,  2, 2 ), new PieceInfo( 9, 8, 2,  3, 2 ),
    //        new PieceInfo( 10, 8, 2,  3, 2 ), new PieceInfo(11, 8, 2,  4, 2 ),
    //        new PieceInfo(  4, 6, 2,  4, 2 ), new PieceInfo( 5, 6, 2,  5, 2 )
    //    };

    //    for (int i = 0; i < infos.Length; i++)
    //    {
    //        if (BoardManager.Instance != null)
    //        {
    //            BoardManager.Instance.SendSpawnPiece(infos[i]._x, infos[i]._y, infos[i]._mainType, infos[i]._subType, infos[i]._color);
    //            _pieceInfos[infos[i]._x, infos[i]._y] = infos[i];
    //        }
    //    }

    //    _readyCount++;
    //}


    // Update is called once per frame
    private void Update () {

        switch(_gs)
        {
            case GameState.GS_READY:  ProcessReady();  break;
            case GameState.GS_START:  ProcessStart();  break;
            case GameState.GS_BATTLE: ProcessBattle(); break;
            case GameState.GS_BATTLE_SIMULATION: ProcessBattleSimulation(); break;
            case GameState.GS_RESULT: ProcessResult(); break;
        }
    }

    private void ProcessReady()
    {
        _readyElapsed += Time.deltaTime;
    }

    private void ProcessStart()
    {
        _startElapsed += Time.deltaTime;

        if (_startElapsed > 1.0f)
        {
            _gs = GameState.GS_BATTLE;
            TurnOver(true);
            Debug.Log(_gs);
        }
    }

    private void ProcessBattle()
    {
		_battleElapsed += Time.deltaTime;

        if (_turnIndex == 1)
        {
            // wait
        }
        else if (_turnIndex == 2)
        {
            // think AI
			if (_battleElapsed < 3.0f)
				return;

			MoveResult mi = ProcessThink();

            SendMoveResult(mi);

            if (mi._battle)
            {
                if (mi._win)
                {
                    _pieceInfos[mi._toX, mi._toY] = _pieceInfos[mi._fromX, mi._fromY];
                    _pieceInfos[mi._fromX, mi._fromY] = null;
                    _pieceInfos[mi._toX, mi._toY]._x = mi._toX;
                    _pieceInfos[mi._toX, mi._toY]._y = mi._toY;
                }
                else
                {
                    _pieceInfos[mi._fromX, mi._fromY] = null;
                }
            } else
            {
                if (mi._move)
                {
                    _pieceInfos[mi._toX, mi._toY] = _pieceInfos[mi._fromX, mi._fromY];
                    _pieceInfos[mi._fromX, mi._fromY] = null;
                    _pieceInfos[mi._toX, mi._toY]._x = mi._toX;
                    _pieceInfos[mi._toX, mi._toY]._y = mi._toY;
                }
            }

            _gs = GameState.GS_BATTLE_SIMULATION;
        }
    }

    private void ProcessBattleSimulation()
    {
        _simulationElapsed += Time.deltaTime;

        if (_simulationElapsed > 3.0f)
        {
            bool isGameOver = GameOver();
            if (isGameOver)
            {
                TurnOver(false);
                _gs = GameState.GS_RESULT;
            }
            else
            {
                TurnOver(true);
                _gs = GameState.GS_BATTLE;
                _battleElapsed = 0;
				_simulationElapsed = 0;
            }
        }
    }
    
    private int GetSpecialAbility(int pid, int fromX, int fromY, int toX, int toY)
    {
        int special = 0;
        int p1special = 0;
        int p2special = 0;

        //struct SpecialAbility
        //{
        //    public int p1special;
        //    public int p1maintype;
        //    public int p1subtype;
        //    public int p2special;
        //    public int p2maintype;
        //    public int p2subtype;
        //}

        //SpecialAbility sa;




		PieceInfo p = _pieceInfos[fromX, fromY];
		if (p == null)
			return special;
		
		if (p._color != pid)
			return special;
		
		PieceInfo enemy = _pieceInfos[toX, toY];
        if (enemy == null)
            return special;

        if (p._color == enemy._color)
            return special;


        if (p._mainType == 2)
        {
            if (p._subType == 2) // spy
                p1special = 1;
            if (p._subType == 4) // meta
                p2special = 2;
        }

        if (enemy._mainType == 2)
        {
            if (enemy._subType == 2)
                p2special = 1;
            if (enemy._subType == 4)
                p2special = 2;
        }

        return p1special + p2special;
    }

	private MoveResult GetMoveResult(int pid, int fromX, int fromY, int toX, int toY)
    {
        MoveResult mi = new MoveResult();
		mi.Clear();
        mi._move = false;
        mi._fromX = 0;
        mi._fromY = 0;
        mi._toX = 0;
        mi._toY = 0;
		
		PieceInfo p = _pieceInfos[fromX, fromY];
		if (p == null)
		{
			return mi;
		}
		
		if (p._color != pid)
		{
			return mi;
		}
		
		PieceInfo enemy = _pieceInfos[toX, toY];
		if (enemy != null)
		{
            bool win = WinOrLose(p._mainType, p._subType, enemy._mainType, enemy._subType);

            mi._move = true;
            mi._fromX = p._x;
            mi._fromY = p._y;
            mi._toX = enemy._x;
            mi._toY = enemy._y;
            mi._battle = true;
            mi._win = win;
		}
		else
		{
			mi._move = true;
            mi._fromX = fromX;
            mi._fromY = fromY;
            mi._toX = toX;
            mi._toY = toY;
            mi._battle = false;
            mi._win = false;
		}
		return mi;
	}

    private MoveResult ProcessThink()
    {
		Debug.Log("ProcessThink");
		
        MoveResult mi = new MoveResult();
		mi.Clear();
        mi._move = false;
        mi._fromX = 0;
        mi._fromY = 0;
        mi._toX = 0;
        mi._toY = 0;

        PieceInfo p = GetPieceNearEnemy(_turnIndex);
        if (p != null)
        {
            PieceInfo enemy = FindEnemy(p._x, p._y, p._color);
            if (enemy == null)
            {
                Debug.Log("What the fuck!!!!");
            }

            bool win = WinOrLose(p._mainType, p._subType, enemy._mainType, enemy._subType);

            mi._move = true;
            mi._fromX = p._x;
            mi._fromY = p._y;
            mi._toX = enemy._x;
            mi._toY = enemy._y;
            mi._battle = true;
            mi._win = win;
        }
        if (p == null)
        {
            p = GetPieceMoveable(_turnIndex);
            if (p != null)
            {
				Debug.Log("GetPieceMoveable");
                int xx; int yy;
                bool moveable = GetMovePos(p._x, p._y, out xx, out yy);
				if (moveable) {
					mi._move = true;
					mi._fromX = p._x;
					mi._fromY = p._y;
					mi._toX = xx;
					mi._toY = yy;
					mi._battle = false;
					mi._win = false;
				}
            }
			else
			{
				Debug.Log("not move");
			}
			
        }

        return mi;
    }

    private bool WinOrLose(int heroMainType, int heroSubType, int enemyMainType, int enemySubType)
    {
        // killer, cleaner, spy, gas, meta, center
        if (heroMainType == 1 && enemyMainType == 1)
        {
            if (heroSubType >= enemySubType)
                return true;
            else
                return false;
        }
        if (heroMainType == 1 && enemyMainType == 2)
        {
            switch (enemySubType)
            {
                case 0: // killer
				{
                    if (heroSubType > 12)
                        return false;
                    else
                        return true;
                    // break;
				}
                case 1: // cleaner
                    return true;
                    // break;
                case 2: // spy
                    // special notify
                    return true;
                    // break;
                case 3: // gas
                    return false;
                    // break;
                case 4: // meta
                    // special change this meta
                    return false;
                    // break;
                case 5: // center
                    return true;
                    // break;
            }
        }
        if (heroMainType == 2 && enemyMainType == 1)
        {
            switch (heroSubType)
            {
                case 0: // killer
                    if (enemySubType > 12)
                        return true;
                    else
                        return false;
                    // break;
                case 1: // cleaner
                    return false;
                    // break;
                case 2: // spy
                    // special notify
                    return false;
                    // break;
                case 3: // gas  // attacker should not be gas
                    return true;
                    // break;
                case 4: // meta
                    // special change this meta
                    return true;
                    // break;
                case 5: // center  // attacker should not be center
                    return false;
                    // break;
            }
        }
        if (heroMainType == 2 && enemyMainType == 2)
        {
            if (heroSubType != 0)
            {
                if (heroSubType == enemySubType)
				{
                    return true;
				}
            }
			
			return true;

            // switch (heroSubType)
            // {
                // case 0: // killer  // 오로지 별을 잡기 위한 유닛이어서 모두에게 진다.
                    // if (enemySubType == 5)
                        // return false;
                    // // break;
                // case 1: // cleaner
                    // if (enemySubType == 0 || enemySubType == 2 || enemySubType == 3 || enemySubType == 5)
                        // return true;
                    // return false;
                    // // break;
                // case 2: // spy
                    // // special notify
                    // if (enemySubType == 0 || enemySubType == 5)
                        // return true;
                    // return false;
                    // // break;
                // case 3: // gas  // attacker should not be gas
                    // Debug.Log("attacker should not be gas");
                    // return true;
                    // // break;
                // case 4: // meta
                    // // special change this meta
                    // return true;
                    // // break;
                // case 5: // center  // attacker should not be center
                    // return false;
                    // // break;
            // }
        }

        Debug.Log("Invalid attack battle");
        return false;
    }

    private PieceInfo GetPieceMoveable(int color)
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 12; x++)
            {
                PieceInfo p = _pieceInfos[x, y];

                if (p == null)
                    continue;

                if (p._color != 2)
                    continue;

                bool moveable = CanPieceMove(p._mainType, p._subType);
                if (moveable == false)
                    continue;

                int xx, yy;
                bool findEmptyPos = GetMovePos(p._x, p._y, out xx, out yy);
                if (findEmptyPos)
                    return p;
            }
        }
        return null;
    }

    private bool GetMovePos(int x, int y, out int xx, out int yy)
    {
        xx = 0;
        yy = 0;

        if (y > 0)
        {
            if (_pieceInfos[x, y - 1] == null)
            {
                xx = x;
                yy = y - 1;
                return true;
            }
        }
        if (y < 8)
        {
            if (_pieceInfos[x, y + 1] == null)
            {
                xx = x;
                yy = y + 1;
                return true;
            }
        }

        if (x > 0)
        {
            if (_pieceInfos[x - 1, y] == null)
            {
                xx = x - 1;
                yy = y;
                return true;
            }
        }
        if (x < 11)
        {
            if (_pieceInfos[x + 1, y] == null)
            {
                xx = x + 1;
                yy = y;
                return true;
            }
        }
        return false;
    }

    private PieceInfo GetPieceNearEnemy(int color)
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 12; x++)
            {
                PieceInfo p = _pieceInfos[x, y];

                if (p == null)
                    continue;

                if (p._color != 2)
                    continue;

                bool moveable = CanPieceMove(p._mainType, p._subType);
                if (moveable == false)
                    continue;
                PieceInfo enemy = FindEnemy(x, y, color);
                if (enemy != null)
                    return p;
            }
        }
        return null;
    }

    private PieceInfo FindEnemy(int x, int y, int color)
    {
        PieceInfo f = null;
		PieceInfo r = null;
		PieceInfo l = null;
		PieceInfo b = null;
		
        if (y > 0)
            f = _pieceInfos[x, y - 1];
        if (y < 8)
            b = _pieceInfos[x, y + 1];
        if (x > 0)
            l = _pieceInfos[x - 1, y];
        if (x < 11)
            r = _pieceInfos[x + 1, y];

        int enemyColor = 1;
        if (color == 1)
            enemyColor = 2;

        if (f != null && f._color == enemyColor)
        {
            return f;
        }
        if (b != null && b._color == enemyColor)
        {
            return b;
        }
        if (l != null && l._color == enemyColor)
        {
            return l;
        }
        if (r != null && r._color == enemyColor)
        {
            return r;
        }

        return null;
    }

    private bool CanPieceMove(int mainType, int subType)
    {
        if (mainType == 1)
            return true;

        if (mainType == 2)
        {
            if (subType == 0 || subType == 1 || subType == 2 || subType == 5)
                return true;
        }

        return false;
    }

    private void TurnOver(bool on)
    {
        if (on)
        {
            if (_turnIndex == 0)
                _turnIndex = 1;
            else if (_turnIndex == 1)
                _turnIndex = 2;
            else if (_turnIndex == 2)
                _turnIndex = 1;
        }
        else
        {
            _turnIndex = 0;
        }
		
		NetworkManager.Instance.SendTurnOver(_turnIndex);
    }

    private void ProcessResult()
    {
        _resultElapsed += Time.deltaTime;

        if (_resultElapsed > 1.0f)
        {
            int winnerIndex = GetWinnerIndex();
            NetworkManager.Instance.SendGameResult(winnerIndex);
            GameDataClear();
            _gs = GameState.GS_END;
        }
    }

    private bool GameOver()
    {
		bool p1center = false;
		bool p2center = false;
		for (int x = 0; x < 12; x++)
		{
			for (int y = 0; y < 9; y++)
			{
				PieceInfo p = _pieceInfos[x, y];
				if (p != null) {
					if (p._mainType == 2 && p._subType == 5) {
						if (p._color == 1)
							p1center = true;
						if (p._color == 2)
							p2center = true;
							
					}
				}
			}
		}
		
		if (p1center == false || p2center == false)
			return true;
		
        return false;
    }

    private int GetWinnerIndex()
    {
        bool p1center = false;
        bool p2center = false;
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                PieceInfo p = _pieceInfos[x, y];
                if (p != null)
                {
                    if (p._mainType == 2 && p._subType == 5)
                    {
                        if (p._color == 1)
                            p1center = true;
                        if (p._color == 2)
                            p2center = true;
                    }
                }
            }
        }

        if (p1center == false && p2center == true)
            return 2;
        if (p1center == true && p2center == false)
            return 1;

        return 0;
    }

    private void GameDataClear()
    {
        _readyElapsed = 0;
        _readyCount = 0;
        _startElapsed = 0;
        _resultElapsed = 0;
        _battleElapsed = 0;
        _simulationElapsed = 0;
        _battleAction = false;
        _turnIndex = 0;

        for (int i = 0; i < 12; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                _pieceInfos[i, j] = null;
            }
        }
    }
	
	public void OnReceiveMovePiece(int pid, int fromX, int fromY, int toX, int toY)
	{
		Debug.Log("OnReceiveMovePiece");
		if (_gs != GameState.GS_BATTLE) 
		{
			Debug.Log("Invalid state!!  OnReceiveMovePiece");
			return;
		}
		
		if (_turnIndex != pid) 
		{
			Debug.Log("Invalid pid!! OnReceiveMovePiece");
			return;
		}

		MoveResult mi = GetMoveResult(pid, fromX, fromY, toX, toY);
        SendMoveResult(mi);

        int special = GetSpecialAbility(pid, fromX, fromY, toX, toY);
		
		special++;
			
		if (mi._battle)
		{
			if (mi._win)
			{
				_pieceInfos[mi._toX, mi._toY] = _pieceInfos[mi._fromX, mi._fromY];
				_pieceInfos[mi._fromX, mi._fromY] = null;
				_pieceInfos[mi._toX, mi._toY]._x = mi._toX;
				_pieceInfos[mi._toX, mi._toY]._y = mi._toY;
			}
			else
			{
				_pieceInfos[mi._fromX, mi._fromY] = null;
			}
		} 
		else
		{
			if (mi._move)
			{
				_pieceInfos[mi._toX, mi._toY] = _pieceInfos[mi._fromX, mi._fromY];
				_pieceInfos[mi._fromX, mi._fromY] = null;
				_pieceInfos[mi._toX, mi._toY]._x = mi._toX;
				_pieceInfos[mi._toX, mi._toY]._y = mi._toY;
			}
		}

        _gs = GameState.GS_BATTLE_SIMULATION;
	}
	
	public void SendMovePiece(int pid, int fromX, int fromY, int toX, int toY)
	{
		OnReceiveMovePiece(pid, fromX, fromY, toX, toY);
	}

    public void SendMoveResult(MoveResult mr)
    {
        NetworkManager.Instance.SendMoveResult(mr._color, mr._move, mr._fromX, mr._fromY, mr._toX, mr._toY, mr._battle, mr._win);
    }

    public void OnReceivePieceInfo(string packet)
    {
        _readyCount++;
        CreatePiece(packet);

        _readyCount++;
        string randomPieceInfo = GetRandomPieceInfo();
        CreatePiece(randomPieceInfo);

        Invoke("SendUserInfo", 1.0f);
        Invoke("SendPieceInfo", 2.0f);
    }

    public void SendUserInfo()
    {
        string userInfos = "";
        for (int i = 0; i < _userInfos.Length; i++)
        {
            userInfos = userInfos + _userInfos[i].userName + "|" + _userInfos[i].userIndex.ToString() + "|";
        }

        NetworkManager.Instance.SendUserInfo(userInfos);
    }

    public void SendPieceInfo()
    {
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 9; y++)
            {
                PieceInfo pi = _pieceInfos[x, y];
                if (pi != null)
                {
                    NetworkManager.Instance.SendSpawnPiece(pi._x, pi._y, pi._mainType, pi._subType, pi._color);
                }
            }
        }

        _gs = GameState.GS_START;
    }

    private string GetRandomPieceInfo()
    {
        string pieceInfo;
        pieceInfo = "guest|";
        //                 x|y|t|st|
        pieceInfo = pieceInfo + "0|1|1|0|" + "1|1|1|1|";
        pieceInfo = pieceInfo + "2|1|1|2|" + "3|1|1|3|";
        pieceInfo = pieceInfo + "4|1|1|4|" + "5|1|1|5|";
        pieceInfo = pieceInfo + "6|1|1|6|" + "7|1|1|7|";
        pieceInfo = pieceInfo + "8|1|1|8|" + "9|1|1|9|";
        pieceInfo = pieceInfo + "10|1|1|10|" + "11|1|1|11|";
        pieceInfo = pieceInfo + "0|0|1|12|" + "1|0|1|13|";
        pieceInfo = pieceInfo + "2|0|1|14|" + "3|0|1|15|";
        pieceInfo = pieceInfo + "4|0|1|16|" + "5|0|2|0|";
        pieceInfo = pieceInfo + "6|0|2|1|" + "7|0|2|2|";
        pieceInfo = pieceInfo + "8|0|2|2|" + "9|0|2|3|";
        pieceInfo = pieceInfo + "10|0|2|3|" + "11|0|2|4|";
        pieceInfo = pieceInfo + "4|2|2|4|" + "5|2|2|5|";

        return pieceInfo;
    }

    private void CreatePiece(string pieceInfo)
    {
        string[] aData = pieceInfo.Split('|');
		
        if (aData.Length != 106)
		{
			Debug.Log(aData.Length);
            return;
		}

        UserInfo userInfo = new UserInfo();
        userInfo.userName = aData[0];
        userInfo.userId = _readyCount;
        userInfo.userIndex = _readyCount;

        _userInfos[_readyCount - 1] = userInfo;

        int pieceCount = (int)aData.Length / 4;
        for (int i = 0; i < pieceCount; i++)
        {
            int index = i * 4 + 1;
            int x = int.Parse(aData[index]);
            int y = int.Parse(aData[index + 1]);
            if (_readyCount == 2)
            {
                x = 11 - x;
                y = 8 - y;
            }
            int mainType = int.Parse(aData[index + 2]);
            int subType = int.Parse(aData[index + 3]);
            PieceInfo pi = new PieceInfo(x, y, mainType, subType, _readyCount);
            _pieceInfos[x, y] = pi;
        }
    }
}
