using UnityEngine;
using System.Collections;

public class PieceMan : MonoBehaviour {
    public GameObject _wall;
    public GameObject _bottom;
    public GameObject _idText;

    public int _currentX { get; set; }
    public int _currentY { get; set; }

    public int _mainType { set; get; } // soldier or special
    public int _subType { set; get; }  // class
	
	public int _color {set; get; }
	

    public void SetProperty(int x, int y, int color, int mainType, int subType)
    {
        _currentX = x;
        _currentY = y;
        _mainType = mainType;
        _subType = subType;
        _color = color;

        Color c = Color.yellow;
        if (color == 1)
            c = Color.red;
        if (color == 2)
            c = Color.blue;

        MeshRenderer mr = _wall.GetComponent<MeshRenderer>();
        if (mr != null)
            mr.material.color = c;

        mr = _bottom.GetComponent<MeshRenderer>();
        if (mr != null)
            mr.material.color = c;

        TextMesh tm = _idText.GetComponent<TextMesh>();
        if (tm != null)
            tm.text = GetClassText(mainType, subType);
    }

    private string GetClassText(int mainType, int subType)
    {
        string r = "";
        if (mainType == 1)
        {
            switch (subType)
            {
                case 0: r = "S1"; break;
                case 1: r = "S2"; break;
                case 2: r = "S3"; break;
                case 3: r = "S4"; break;
                case 4: r = "V1"; break;
                case 5: r = "V2"; break;
                case 6: r = "V3"; break;
                case 7: r = "D1"; break;
                case 8: r = "D2"; break;
                case 9: r = "D3"; break;
                case 10: r = "O1"; break;
                case 11: r = "O2"; break;
                case 12: r = "O3"; break;
                case 13: r = "G1"; break;
                case 14: r = "G2"; break;
                case 15: r = "G3"; break;
                case 16: r = "G4"; break;
            }
        }
        else if (mainType == 2)
        {
            switch (subType)
            {
                case 0: r = "Killer"; break;
                case 1: r = "Cleaner"; break;
                case 2: r = "Spy"; break;
                case 3: r = "Gas"; break;
                case 4: r = "Meta"; break;
                case 5: r = "Center"; break;
            }
        }

        return r;
    }
    public bool CanMove()
    {
        if (_mainType == 2)
        {
            // Gas or Center can not move
            if (_subType == 3 || _subType == 5)
            {
                return false;
            }
        }
        return true;
    }

}
