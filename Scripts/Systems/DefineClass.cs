using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInfo
{
    public int playerId;
    public string playerName;
    public int rate;
    public int coins;
    private PossessingItems items;
    private float alignInfo;
    private List<TopTable> topTableList;

    public PlayerInfo(int id, string name, int rate, int coins, PossessingItems items, float alignInfo)
    {
        this.playerId = id;
        this.playerName = name;
        this.rate = rate;
        this.coins = coins;
        this.items = items;
        this.alignInfo = alignInfo;

        GameManager.Instance.RefTopTableList(ref topTableList);
    }

    public void RefPossessingItems(ref PossessingItems items)
    {
        items = this.items;
    }

    /// <summary>
    /// 指定したコマを持ち物に加える
    /// </summary>
    /// <param name="id"></param>
    public void AddTop(int id)
    {
        items.GetTop(id);
    }

    /// <summary>
    /// 所持コマ情報を参照する
    /// </summary>
    /// <param name="val1"></param>
    /// <param name="val2"></param>
    public void RefPossessingTopInfo(ref float val1, ref float val2)
    {
        items.RefTopNum(ref val1, ref val2);
    }

    /// <summary>
    /// 配置情報を出力する
    /// </summary>
    /// <returns></returns>
    public int[] GetAlignInfo()
    {
        int[] info = new int[6];
        float tmpAlignInfo = this.alignInfo;

        //バイナリデータから配置情報を復元する
        for (int i = 1; i < info.Length; i++)
        {
            int divis = 0;
            divis = (int)(tmpAlignInfo % 0x10);
            //ID = 0のコマはないので除外
            if (divis != 0)
            {
                info[i] = topTableList.Find(top => top.Id == divis).Id;
            }
            tmpAlignInfo -= divis;
            tmpAlignInfo /= 0x10;
        }

        return info;
    }

    /// <summary>
    /// 配置情報を設定する
    /// </summary>
    /// <param name="info"></param>
    /// <param name="binData"></param>
    /// <returns></returns>
    public bool SetAlignInfo(int[] info, ref float binData)
    {
        float alignInfo = 0;
        bool retVal = false;

        for (int i = 1; i < info.Length; i++)
        {
            alignInfo += info[i] * Mathf.Pow(0x10, i - 1);
        }

        if (alignInfo != this.alignInfo)
        {
            //更新あり
            retVal = true;
            this.alignInfo = alignInfo;
            binData = alignInfo;
        }

        return retVal;
    }


}

public class ShogiBoard
{
    private class BoardInf
    {
        public int topId;
        public bool isMine;
        public int colorInf;
        public GameObject obj;

        public BoardInf()
        {
            this.topId = 0;
            this.isMine = true;
            this.colorInf = 0;
            this.obj = null;
        }
    }

    //public class BoardIndex
    //{
    //    public int xIndex;
    //    public int yIndex;

    //    public BoardIndex(int x, int y)
    //    {
    //        xIndex = x;
    //        yIndex = y;
    //    }
    //}

    private int boardSize;                                   /* 盤面サイズ   */
    private BoardInf[,] boardInf;                            /* 盤面情報     */
    private Vector2 boardPos;                                /* 将棋盤位置   */
    private Dictionary<int, List<BoardIndex>> topMovableDic; /* コマ移動定義 */

    /// <summary>
    /// クラスのインスタンス化
    /// </summary>
    /// <param name="size"></param>
    public ShogiBoard(int size, Vector2 boardPos)
    {
        boardSize = size;
        this.boardPos = boardPos;
        boardInf = new BoardInf[boardSize + 1, boardSize + 1];

        for (int i = 0; i <= boardSize; i++)
        {
            for (int j = 0; j <= boardSize; j++)
            {
                boardInf[i, j] = new BoardInf();
            }
        }

        //すべてのコマ移動定義を格納する
        topMovableDic = new Dictionary<int, List<BoardIndex>>();
        for (int i = 1; i <= 10; i++)
        {
            List<BoardIndex> indexesList = GetMovableIndexDefEachTop(i);
            topMovableDic.Add(i, indexesList);
        }
    }

    /// <summary>
    /// 盤面の中から自ゴマをすべてインデックスで取得する
    /// </summary>
    /// <returns></returns>
    public List<BoardIndex> GetMyTopBoardIndex()
    {
        List<BoardIndex> indexList = new List<BoardIndex>();

        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
            {
                if (boardInf[i,j].topId != 0 &&
                    boardInf[i, j].isMine)
                {
                    BoardIndex index = new BoardIndex(i, j);
                    indexList.Add(index);
                }
            }
        }
        return indexList;
    }

    /// <summary>
    /// 空のインデックスを取得する
    /// </summary>
    /// <returns></returns>
    public List<BoardIndex> GetEmptyBoardIndex()
    {
        List<BoardIndex> indexList = new List<BoardIndex>();

        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
            {
                if (boardInf[i, j].topId == 0)
                {
                    BoardIndex index = new BoardIndex(i, j);
                    indexList.Add(index);
                }
            }
        }
        return indexList;
    }

    /// <summary>
    /// 指定インデックスの自コマ情報を書き換える
    /// </summary>
    /// <param name="val"></param>
    /// <param name="index"></param>
    public void SetIsMine(bool val, BoardIndex index)
    {
        boardInf[index.xIndex, index.yIndex].isMine = val;
    }

    /// <summary>
    /// 指定インデックスのオブジェクトを取得する
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public GameObject GetObjByIndex(BoardIndex index)
    {
        return boardInf[index.xIndex, index.yIndex].obj;
    }

    /// <summary>
    /// 将棋盤インデックスから座標を出力する
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    public Vector2 GetBoardPosByIndex(int xIndex, int yIndex)
    {
        Vector2 cellPos = new Vector2(0, 0);

        //将棋盤の右上を原点にする
        cellPos = boardPos + new Vector2(1.0f, 1.0f);

        cellPos = new Vector2(cellPos.x - (float)(2 * xIndex - 1) / (boardSize),
                              cellPos.y - (float)(2 * yIndex - 1) / (boardSize));

        return cellPos;
    }

    public Vector2 GetBoardPosByIndexChild(BoardIndex index)
    {
        Vector2 cellPos = new Vector2(0, 0);

        //将棋盤の右上を原点にする
        cellPos = boardPos + new Vector2(0.5f, 0.5f);

        cellPos = new Vector2(cellPos.x - (float)(2 * index.xIndex - 1) / (2 * boardSize),
                              cellPos.y - (float)(2 * index.yIndex - 1) / (2 * boardSize));

        return cellPos;
    }

    /// <summary>
    /// 将棋盤インデックスからコマIDを出力する
    /// </summary>
    /// <returns></returns>
    public int GetTopIdByIndex(int xIndex, int yIndex)
    {
        return boardInf[xIndex, yIndex].topId;
    }

    /// <summary>
    /// 将棋盤にコマ情報を登録する
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    public void SetBoardInf(int topId, GameObject obj, int xIndex, int yIndex)
    {
        if (xIndex > boardSize || yIndex > boardSize)
        {
            Debug.LogError("GetBoardPosByIndex : 盤面サイズ以上の値が入力されました。");
            Debug.LogError(boardSize+ " -> " + xIndex + ", " + yIndex);
            return;
        }

        boardInf[xIndex, yIndex].topId = topId;
        boardInf[xIndex, yIndex].obj = obj;
    }

    /// <summary>
    /// 将棋盤にコマ情報を登録する
    /// </summary>
    /// <param name="topId"></param>
    /// <param name="obj"></param>
    /// <param name="isMine"></param>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    public void SetBoardInf(int topId, GameObject obj, bool isMine, int xIndex, int yIndex)
    {
        if (xIndex > boardSize || yIndex > boardSize)
        {
            Debug.LogError("GetBoardPosByIndex : 盤面サイズ以上の値が入力されました。");
            Debug.LogError(boardSize + " -> " + xIndex + ", " + yIndex);
            return;
        }

        boardInf[xIndex, yIndex].topId = topId;
        boardInf[xIndex, yIndex].obj = obj;
        boardInf[xIndex, yIndex].isMine = isMine;
    }

    /// <summary>
    /// 将棋盤情報を削除する
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    public GameObject DelBoardInf(int xIndex, int yIndex)
    {
        if (xIndex > boardSize || yIndex > boardSize)
        {
            Debug.LogError("DelBoardInf : 盤面サイズ以上の値が入力されました。");
            Debug.LogError(boardSize + " -> " + xIndex + ", " + yIndex);
            return null;
        }

        GameObject obj = boardInf[xIndex, yIndex].obj;

        boardInf[xIndex, yIndex].topId = 0;
        boardInf[xIndex, yIndex].obj = null;
        boardInf[xIndex, yIndex].isMine = false;

        Debug.Log("盤面情報削除");

        return obj;
    }

    /// <summary>
    /// ベクトル情報から最も近い盤面位置を出力する
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public Vector2 GetCellPosByPos(Vector2 vec, ref int xIndex, ref int yIndex)
    {
        float tmpVal = 0;

        for (int i = 1; i <= boardSize; i++)
        {
            if (i != 1 &&
                tmpVal < Mathf.Abs(vec.x - GetBoardPosByIndex(i, 1).x))
            {
                break;
            }

            xIndex = i;
            tmpVal = Mathf.Abs(vec.x - GetBoardPosByIndex(i, 1).x);
        }

        tmpVal = 0;
        for (int i = 1; i <= boardSize; i++)
        {
            if (i != 1 &&
                tmpVal < Mathf.Abs(vec.y - GetBoardPosByIndex(1, i).y))
            {
                break;
            }

            yIndex = i;
            tmpVal = Mathf.Abs(vec.y - GetBoardPosByIndex(1, i).y);
        }

        //算出された盤面上にコマがある場合は入力ベクトルを返す
        //if (boardInf[xIndex, yIndex] != 0)
        //{
        //    xIndex = 0;
        //    yIndex = 0;
        //    return vec;
        //}

        return GetBoardPosByIndex(xIndex, yIndex);
    }

    /// <summary>
    /// 入力された値に対応する将棋盤インデックスを出力する
    /// </summary>
    /// <param name="vec"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public bool GetIndexByPos(Vector2 vec, ref int x, ref int y)
    {
        int xIndex = 0;
        int yIndex = 0;
        float tmpVal = 0;

        for (int i = 1; i <= boardSize; i++)
        {
            if (i != 1 &&
                tmpVal < Mathf.Abs(vec.x - GetBoardPosByIndex(i, 1).x))
            {
                break;
            }

            xIndex = i;
            tmpVal = Mathf.Abs(vec.x - GetBoardPosByIndex(i, 1).x);
        }

        tmpVal = 0;
        for (int i = 1; i <= boardSize; i++)
        {
            if (i != 1 &&
                tmpVal < Mathf.Abs(vec.y - GetBoardPosByIndex(1, i).y))
            {
                break;
            }

            yIndex = i;
            tmpVal = Mathf.Abs(vec.y - GetBoardPosByIndex(1, i).y);
        }

        x = xIndex;
        y = yIndex;

        //算出された盤面上にコマがある場合はfalseを返す
        if (boardInf[xIndex, yIndex].topId != 0)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// 指定インデックスにコマがあるかチェックする
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    /// <returns></returns>
    public bool ChkBoardTop(int xIndex, int yIndex)
    {
        /* 禁止入力 */
        if (xIndex == 0 || yIndex == 0)
        {
            return false;
        }

        //指定位置にコマはない
        if (boardInf[xIndex, yIndex].topId == 0)
        {
            return false;
        }
        //指定位置にコマがある
        else
        {
            return true;
        }
    }

    /// <summary>
    /// コマIDと現在地から移動可能領域のリストを出力する
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    public List<BoardIndex> GetMovableIndex(int id, int x, int y)
    {
        List<BoardIndex> indexList = new List<BoardIndex>();
        BoardIndex originIndex = new BoardIndex(x, y);

        //浪人の場合はそのまま渡す
        if (id == 10)
        {
            return topMovableDic[10];
        }

        foreach (BoardIndex tmpIndex in topMovableDic[id])
        {
            BoardIndex index = new BoardIndex(tmpIndex.xIndex + x, tmpIndex.yIndex + y);

            //範囲外であれば移動不可
            if (index.xIndex > boardSize ||
                index.yIndex > boardSize ||
                index.xIndex <= 0        ||
                index.yIndex <= 0)
            {
                continue;
            }

            //味方のコマがあれば移動不可
            if (boardInf[index.xIndex, index.yIndex].topId != 0 &&
                boardInf[index.xIndex, index.yIndex].isMine == true)
            {
                Debug.Log("[" + index.xIndex + index.yIndex + index.xIndex + index.yIndex + "]");
                continue;
            }

            //元の位置とを繋ぐ縦1本道上に敵、味方のコマがあれば移動不可
            //id = 9(狙撃手は縦一本道上に敵味方がいても関係なく移動可)
            if (ChkAnotherTopColumn(index, originIndex) &&
                id != 9)
            {
                continue;
            }

            //元の位置とを繋ぐ横1本道上に敵、味方のコマがあれば移動不可
            if (ChkAnotherTopRow(index, originIndex))
            {
                continue;
            }

            //元の位置とを繋ぐ斜め1本道上に敵、味方のコマがあれば移動不可
            if (ChkAnotherTopDiago(index, originIndex))
            {
                continue;
            }

            //移動可能なのでリストに追加する
            indexList.Add(index);
        }

        return indexList;
    }

    /// <summary>
    /// 指定コマの移動可能定義を取得する
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public List<BoardIndex> GetMovableIndexDefEachTop(int id)
    {
        List<BoardIndex> inf = new List<BoardIndex>();

        switch (id)
        {
            case 1:
                {
                    BoardIndex indexies = new BoardIndex(0,-1);
                    inf.Add(indexies);
                    break;
                }
            case 2:
                { 
                    BoardIndex indexies1 = new BoardIndex(1, -2);
                    BoardIndex indexies2 = new BoardIndex(-1, -2);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    break;
                }
            case 3:
                {
                    BoardIndex indexies1 = new BoardIndex(0, -1);
                    BoardIndex indexies2 = new BoardIndex(0, -2);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    break;
                }
            case 4:
                {
                    BoardIndex indexies1 = new BoardIndex(1, 0);
                    BoardIndex indexies2 = new BoardIndex(1, -1);
                    BoardIndex indexies3 = new BoardIndex(0, 1);
                    BoardIndex indexies4 = new BoardIndex(0, -1);
                    BoardIndex indexies5 = new BoardIndex(-1, 0);
                    BoardIndex indexies6 = new BoardIndex(-1, -1);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    inf.Add(indexies3);
                    inf.Add(indexies4);
                    inf.Add(indexies5);
                    inf.Add(indexies6);
                    break;
                }
            case 5:
                {
                    BoardIndex indexies1 = new BoardIndex(1, 1);
                    BoardIndex indexies2 = new BoardIndex(1, -1);
                    BoardIndex indexies3 = new BoardIndex(0, -1);
                    BoardIndex indexies4 = new BoardIndex(-1, 1);
                    BoardIndex indexies5 = new BoardIndex(-1, -1);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    inf.Add(indexies3);
                    inf.Add(indexies4);
                    inf.Add(indexies5);
                    break;
                }
            case 6:
                {
                    BoardIndex indexies1 = new BoardIndex(1, 0);
                    BoardIndex indexies2 = new BoardIndex(2, 0);
                    BoardIndex indexies3 = new BoardIndex(0, 2);
                    BoardIndex indexies4 = new BoardIndex(0, 1);
                    BoardIndex indexies5 = new BoardIndex(0, -1);
                    BoardIndex indexies6 = new BoardIndex(0, -2);
                    BoardIndex indexies7 = new BoardIndex(-1, 0);
                    BoardIndex indexies8 = new BoardIndex(-2, 0);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    inf.Add(indexies3);
                    inf.Add(indexies4);
                    inf.Add(indexies5);
                    inf.Add(indexies6);
                    inf.Add(indexies7);
                    inf.Add(indexies8);
                    break;
                }
            case 7:
                {
                    BoardIndex indexies1 = new BoardIndex(1, 1);
                    BoardIndex indexies2 = new BoardIndex(2, 2);
                    BoardIndex indexies3 = new BoardIndex(-1, 1);
                    BoardIndex indexies4 = new BoardIndex(-2, 2);
                    BoardIndex indexies5 = new BoardIndex(1, -1);
                    BoardIndex indexies6 = new BoardIndex(2, -2);
                    BoardIndex indexies7 = new BoardIndex(-1, -1);
                    BoardIndex indexies8 = new BoardIndex(-2, -2);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    inf.Add(indexies3);
                    inf.Add(indexies4);
                    inf.Add(indexies5);
                    inf.Add(indexies6);
                    inf.Add(indexies7);
                    inf.Add(indexies8);
                    break;
                }
            case 8:
                {
                    BoardIndex indexies1 = new BoardIndex(1, 1);
                    BoardIndex indexies2 = new BoardIndex(1, 0);
                    BoardIndex indexies3 = new BoardIndex(1, -1);
                    BoardIndex indexies4 = new BoardIndex(0, 1);
                    BoardIndex indexies5 = new BoardIndex(0, -1);
                    BoardIndex indexies6 = new BoardIndex(-1, 1);
                    BoardIndex indexies7 = new BoardIndex(-1, 0);
                    BoardIndex indexies8 = new BoardIndex(-1, -1);
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    inf.Add(indexies3);
                    inf.Add(indexies4);
                    inf.Add(indexies5);
                    inf.Add(indexies6);
                    inf.Add(indexies7);
                    inf.Add(indexies8);
                    break;
                }
            case 9:
                {
                    BoardIndex indexies = new BoardIndex(0, -4);
                    inf.Add(indexies);
                    break;
                }
            case 10:
                {
                    /* ターン開始時に移動可能マスがランダムで3つ確定する */
                    BoardIndex indexies1 = new BoardIndex(Random.Range(1, 6), Random.Range(1, 6));
                    BoardIndex indexies2 = new BoardIndex(Random.Range(1, 6), Random.Range(1, 6));
                    BoardIndex indexies3 = new BoardIndex(Random.Range(1, 6), Random.Range(1, 6));
                    inf.Add(indexies1);
                    inf.Add(indexies2);
                    inf.Add(indexies3);
                    break;
                }
        }

        return inf;
    }

    /// <summary>
    /// 浪人の移動定義を変更する
    /// </summary>
    /// <param name="index"></param>
    public void SetRoninMovableIndexDef(BoardIndex index)
    {
        for (int i = 0; i < 3; i++)
        {
            int xIndex = Random.Range(1,6);
            int yIndex = Random.Range(1,6);
            topMovableDic[10][i].xIndex = xIndex;
            topMovableDic[10][i].xIndex = yIndex;
        }
    }

    /// <summary>
    /// 元の位置とを繋ぐ縦1本道上に敵、味方のコマがあるか
    /// </summary>
    /// <param name="chkIndex"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public bool ChkAnotherTopColumn(BoardIndex chkIndex, BoardIndex origin)
    {
        bool retVal = false;
        int chkVec = 0;
        int chkCnt = 0;

        //縦一本道ではない
        if (chkIndex.xIndex != origin.xIndex)
        {
            retVal = false;
            return retVal;
        }

        //下方向に調べる
        if (chkIndex.yIndex - origin.yIndex > 0)
        {
            chkVec = 1;
            chkCnt = chkIndex.yIndex - origin.yIndex - 1;
        }
        //上方向に調べる
        else
        {
            chkVec = -1;
            chkCnt = origin.yIndex - chkIndex.yIndex - 1;
        }

        for (int i = origin.yIndex + chkVec; chkCnt > 0; i += chkVec)
        {
            //コマを発見
            if (boardInf[origin.xIndex, i].topId != 0)
            {
                retVal = true;
            }

            chkCnt--;
        }

        return retVal;
    }

    /// <summary>
    /// 元の位置とを繋ぐ横1本道上に敵、味方のコマがあるか
    /// </summary>
    /// <param name="chkIndex"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public bool ChkAnotherTopRow(BoardIndex chkIndex, BoardIndex origin)
    {
        bool retVal = false;
        int chkVec = 0;
        int chkCnt = 0;

        //横一本道ではない
        if (chkIndex.yIndex != origin.yIndex)
        {
            retVal = false;
            return retVal;
        }

        //左方向に調べる
        if (chkIndex.xIndex - origin.xIndex > 0)
        {
            chkVec = 1;
            chkCnt = chkIndex.xIndex - origin.xIndex - 1;
        }
        //右方向に調べる
        else
        {
            chkVec = -1;
            chkCnt = origin.xIndex - chkIndex.xIndex - 1;
        }

        for (int i = origin.xIndex + chkVec; chkCnt > 0; i += chkVec)
        {
            //コマを発見
            if (boardInf[i, origin.yIndex].topId != 0)
            {
                retVal = true;
            }

            chkCnt--;
        }

        return retVal;
    }

    /// <summary>
    /// 元の位置とを繋ぐ斜め1本道上に敵、味方のコマがあるか
    /// </summary>
    /// <param name="chkIndex"></param>
    /// <param name="origin"></param>
    /// <returns></returns>
    public bool ChkAnotherTopDiago(BoardIndex chkIndex, BoardIndex origin)
    {
        bool retVal = false;
        int chkVecX = 0;
        int chkVecY = 0;
        int chkCnt = 0;

        int tmpXDiff = chkIndex.xIndex - origin.xIndex;
        int tmpYDiff = chkIndex.yIndex - origin.yIndex;

        //斜め一本道ではない
        if (Mathf.Abs(tmpXDiff) != Mathf.Abs(tmpYDiff))
        {
            retVal = false;
            return retVal;
        }

        //左下方向に調べる
        if (chkIndex.xIndex - origin.xIndex > 0 &&
            chkIndex.yIndex - origin.yIndex > 0)
        {
            chkVecX = 1;
            chkVecY = 1;
            chkCnt = chkIndex.xIndex - origin.xIndex - 1;
        }
        //右下方向に調べる
        else if (chkIndex.xIndex - origin.xIndex < 0 &&
                 chkIndex.yIndex - origin.yIndex > 0)
        {
            chkVecX = -1;
            chkVecY = 1;
            chkCnt = origin.xIndex - chkIndex.xIndex - 1;
        }
        //右上方向に調べる
        else if (chkIndex.xIndex - origin.xIndex < 0 &&
                 chkIndex.yIndex - origin.yIndex < 0)
        {
            chkVecX = -1;
            chkVecY = -1;
            chkCnt = origin.xIndex - chkIndex.xIndex - 1;
        }
        //左上方向に調べる
        else if (chkIndex.xIndex - origin.xIndex > 0 &&
                 chkIndex.yIndex - origin.yIndex < 0)
        {
            chkVecX = 1;
            chkVecY = -1;
            chkCnt = chkIndex.xIndex - origin.xIndex - 1;
        }

        for (int i = 1; chkCnt > 0; i++)
        {
            //コマを発見
            if (boardInf[origin.xIndex + chkVecX * i, origin.yIndex + chkVecY * i].topId != 0)
            {
                retVal = true;
            }

            chkCnt--;
        }

        return retVal;
    }

    /// <summary>
    /// 指定インデックス間の間にあるインデックスリストを出力する
    /// </summary>
    /// <param name="index1"></param>
    /// <param name="index2"></param>
    /// <returns></returns>
    public List<BoardIndex> GetCellIndexBitweenCells(BoardIndex index1, BoardIndex index2)
    {
        List<BoardIndex> indexList = new List<BoardIndex>();

        BoardIndex vec = new BoardIndex(index1.xIndex - index2.xIndex, index1.yIndex - index2.yIndex);
        BoardIndex vec2 = new BoardIndex(0, 0);
        BoardIndex tmp = new BoardIndex(index2.xIndex, index2.yIndex);

        //不正な値
        if (vec.xIndex == 0 && vec.yIndex == 0)
        {
            return indexList;
        }

        int roopCnt = 0;

        //縦
        if (vec.xIndex == 0)
        {
            roopCnt = Mathf.Abs(vec.yIndex);
            vec2 = new BoardIndex(0, vec.yIndex / Mathf.Abs(vec.yIndex));
        }
        //横
        else if (vec.yIndex == 0)
        {
            roopCnt = Mathf.Abs(vec.xIndex);
            vec2 = new BoardIndex(vec.xIndex / Mathf.Abs(vec.xIndex), 0);
        }
        //斜め
        else if (Mathf.Abs(vec.xIndex) == Mathf.Abs(vec.yIndex))
        {
            roopCnt = Mathf.Abs(vec.xIndex);
            vec2 = new BoardIndex(vec.xIndex / Mathf.Abs(vec.xIndex), vec.yIndex / Mathf.Abs(vec.yIndex));
        }

        for (int i = 1; i < roopCnt; i++)
        {
            tmp.xIndex += i * vec2.xIndex;
            tmp.yIndex += i * vec2.yIndex;
            BoardIndex index = new BoardIndex(tmp.xIndex, tmp.yIndex);
            indexList.Add(index);
        }

        return indexList;
    }

    /// <summary>
    /// ボードに色をつける
    /// </summary>
    /// <param name="index"></param>
    /// <param name="IsMine"></param>
    public void SetBoardColor(BoardIndex index, bool IsMine)
    {
        if (IsMine)
        {
            boardInf[index.xIndex, index.yIndex].colorInf = 1;
        }
        else
        {
            boardInf[index.xIndex, index.yIndex].colorInf = 2;
        }
    }

    /// <summary>
    /// すべてのセルが塗られているか
    /// </summary>
    /// <returns></returns>
    public bool ChkBoardColorAll()
    {
        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
            {
                if (boardInf[i, j].colorInf == 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// 自分が勝ったか
    /// </summary>
    /// <returns></returns>
    public bool ChkWinner()
    {
        int colorCnt = 0;

        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
            {
                if (boardInf[i, j].colorInf == 1)
                {
                    colorCnt++;
                }
            }
        }

        if (colorCnt > (boardSize * boardSize) / 2.0f)
        {
            return true;
        }
        else
        {
            return false;
        }

    }

    /// <summary>
    /// 所持しているすべてのゲームオブジェクトを出力する
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetGameObjectAll()
    {
        List<GameObject> objList = new List<GameObject>();

        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
            {
                if (boardInf[i, j].topId != 0)
                {
                    objList.Add(boardInf[i, j].obj);
                }
            }
        }
        return objList;

    }
}

public class BoardIndex
{
    public int xIndex;
    public int yIndex;

    public BoardIndex(int x, int y)
    {
        xIndex = x;
        yIndex = y;
    }
}

public class PossessingItems
{
    float possessingItemsVal1 = 0x00000000;
    float possessingItemsVal2 = 0x00000000;
    List<TopTable> topTableList;
    Dictionary<int, int> topDic = new Dictionary<int, int>(); /* 所持しているコマのid, 個数 */

    public PossessingItems(float val1, float val2)
    {
        topTableList = new List<TopTable>();
        GameManager.Instance.RefTopTableList(ref topTableList);
        
        possessingItemsVal1 = val1;
        possessingItemsVal2 = val2;

        foreach(TopTable top in topTableList)
        {
            int divis = 0;

            switch (top.Page)
            {
                case 1:
                    divis = (int)(val1 % 0x10);
                    val1 -= divis;
                    val1 /= 0x10;
                    break;
                case 2:
                    divis = (int)(val2 % 0x10);
                    val2 -= divis;
                    val2 /= 0x10;
                    break;
                default:
                    break;
            }

            topDic.Add(top.Id, divis);
        }
    }

    /// <summary>
    /// 所持コマリストを出力する
    /// </summary>
    public Dictionary<int, int> RefTopDic()
    {
        Dictionary<int, int> tmpDic = new Dictionary<int, int>();
        tmpDic = this.topDic;
        return tmpDic;
    }

    /// <summary>
    /// 数値型所持コマリストを参照する
    /// </summary>
    /// <param name="val1"></param>
    /// <param name="val2"></param>
    public void RefTopNum(ref float val1, ref float val2)
    {
        val1 = possessingItemsVal1;
        val2 = possessingItemsVal2;
    }

    /// <summary>
    /// 指定されたコマの所持数を1増やす
    /// </summary>
    /// <param name="topId"></param>
    public void GetTop(int topId)
    {
        TopTable topTable = topTableList.Find(top => top.Id == topId);

        /* コマは5つ以上持てないようにする */
        if (topDic[topId] >= 5)
        {
            Debug.Log("5つ以上保持しています");
            return;
        }

        //float値を登録
        SetPossessingItemsIntVal(topTable.Page, topTable.Digit);
        //リスト値を登録
        topDic[topId]++;
    }

    /// <summary>
    /// pageとdegitから整数値として登録する
    /// </summary>
    /// <param name="page"></param>
    /// <param name="digit"></param>
    private void SetPossessingItemsIntVal(int page, int digit)
    {
        float tmpVal = Mathf.Pow(0x10, digit - 1);

        switch (page)
        {
            case 1:
                possessingItemsVal1 += tmpVal;
                break;
            case 2:
                possessingItemsVal2 += tmpVal;
                break;
            default:
                break;
        }
    }
}


public enum MovePattern
{
    PATTERN_01,
    PATTERN_02,
    PATTERN_03,
    PATTERN_04,
    PATTERN_05
}
