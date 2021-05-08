using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using UnityEngine.UI;

public class BattleSceneManager : MonoBehaviourPunCallbacks, IPunTurnManagerCallbacks
{
    [SerializeField]
    private GameObject machingPanel = default;

    [SerializeField]
    private GameObject BoardObj = default;

    [SerializeField]
    private GameObject myTopStageObj = default;

    [SerializeField]
    private GameObject oppenentsTopStageObj = default;

    [SerializeField]
    private GameObject PopupPanel = default;

    [SerializeField]
    private Text popupText = default;

    [SerializeField]
    private GameObject MyPanel = default;

    [SerializeField]
    private GameObject OpponentPanel = default;

    public class TopMoveInf
    {
        public BoardIndex source;
        public BoardIndex destination;
        public bool IsBring;

        public TopMoveInf(BoardIndex source, BoardIndex destination, bool IsBring)
        {
            this.source = source;
            this.destination = destination;
            this.IsBring = IsBring;
        }
    }

    private PunTurnManager turnManager;   /* ターンマネージャー */
    private PlayerInfo playerInfo;        /* プレイヤー情報     */
    List<TopTable> topTableList;          /* コマテーブルリスト */
    private int[] alignInfo;              /* 自身の盤面情報     */
    private int[] opponentAlignInfo;      /* 相手の盤面情報     */
    private int myTurnIndex = 1;          /* ターンインデックス */
    private GameObject[,] redCellObj;     /* 赤いセル           */
    private GameObject[,] blueCellObj;    /* 赤いセル           */
    private GameObject[,] yellowCellObj;  /* 黄色いセル         */
    private GameObject[,] whiteCellObj;   /* 白いセル           */
    private ShogiBoard board;             /* 盤面情報           */
    private ShogiBoard myTopStage;        /* 自分の持ち駒情報   */
    private ShogiBoard opponentTopStage;  /* 相手の持ち駒情報   */

    void Start()
    {
        turnManager = GetComponent<PunTurnManager>();
        turnManager.TurnManagerListener = this;
        turnManager.TurnDuration = 30f;

        PhotonNetwork.IsMessageQueueRunning = true;

        GameManager.Instance.RefTopTableList(ref topTableList);
        GameManager.Instance.RefTopAlignInfo(ref alignInfo);
        opponentAlignInfo = new int[GameDef.BOARD_CELLS + 1];
        //ボードのインスタンス化
        board = new ShogiBoard(GameDef.BOARD_CELLS, new Vector2(BoardObj.transform.position.x, BoardObj.transform.position.z));
        redCellObj = new GameObject[GameDef.BOARD_CELLS + 1, GameDef.BOARD_CELLS + 1];
        blueCellObj = new GameObject[GameDef.BOARD_CELLS + 1, GameDef.BOARD_CELLS + 1];
        yellowCellObj = new GameObject[GameDef.BOARD_CELLS + 1, GameDef.BOARD_CELLS + 1];
        whiteCellObj = new GameObject[GameDef.BOARD_CELLS + 1, GameDef.BOARD_CELLS + 1];
        GameObject redCellPref = Resources.Load<GameObject>("GameObjects/BattleScene/RedCellObject");
        GameObject blueCellPref = Resources.Load<GameObject>("GameObjects/BattleScene/BlueCellObject");
        GameObject yellowCellPref = Resources.Load<GameObject>("GameObjects/BattleScene/YellowCellObject");
        GameObject whiteCellPref = Resources.Load<GameObject>("GameObjects/BattleScene/WhiteCellObject");
        PopupPanel.SetActive(false);
        //配置情報をもとに盤面を初期化する
        //コマ配置情報をもとにコマを再配置する
        GameObject topPref = Resources.Load<GameObject>("GameObjects/BattleScene/Top");
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            if (alignInfo[i] == 0)
            {
                continue;
            }

            GameObject top = Instantiate(topPref);
            //マテリアル設定
            Material[] mats = top.GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == alignInfo[i]).AssetName);
            top.GetComponent<MeshRenderer>().materials = mats;

            BattleTopCtrl topCtrl = top.GetComponent<BattleTopCtrl>();
            BoardIndex index = new BoardIndex(i, GameDef.BOARD_CELLS);
            Vector2 tmpVec = board.GetBoardPosByIndex(index);
            top.transform.position = new Vector3(tmpVec.x, 1, tmpVec.y);
            topCtrl.topId = alignInfo[i];
            topCtrl.isMine = true;
            topCtrl.SetIsMine(true);
            topCtrl.SetIsMyTurn(false);
            board.SetBoardInf(alignInfo[i], top, true, index);
        }

        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            for (int j = 1; j <= GameDef.BOARD_CELLS; j++)
            {
                BoardIndex index = new BoardIndex(i, j);
                Vector2 tmpVal = board.GetBoardPosByIndex(index);
                Vector3 tmpVal2 = new Vector3(tmpVal.x, 1, tmpVal.y);

                //色付きセル(黄色、赤)を配置する
                GameObject redCell = Instantiate(redCellPref, tmpVal2, Quaternion.identity);
                redCellObj[i, j] = redCell;
                GameObject yellowCell = Instantiate(yellowCellPref, tmpVal2, Quaternion.identity);
                yellowCellObj[i, j] = yellowCell;
                GameObject blueCell = Instantiate(blueCellPref, tmpVal2, Quaternion.identity);
                blueCellObj[i, j] = blueCell;
                GameObject whiteCell = Instantiate(whiteCellPref, tmpVal2, Quaternion.identity);
                whiteCellObj[i, j] = whiteCell;

                //赤セルを少し下げておく
                redCellObj[i, j].transform.position = new Vector3(redCellObj[i, j].transform.position.x,
                                                                  0.5f,
                                                                  redCellObj[i, j].transform.position.z);
                //青セルを少し下げておく
                blueCellObj[i, j].transform.position = new Vector3(blueCellObj[i, j].transform.position.x,
                                                                   0.5f,
                                                                   blueCellObj[i, j].transform.position.z);
                //黄セルを少し下げておく
                yellowCellObj[i, j].transform.position = new Vector3(yellowCellObj[i, j].transform.position.x,
                                                                     0.5f,
                                                                     yellowCellObj[i, j].transform.position.z);
            }
        }

        //コマ置き場のインスタンス化
        myTopStage = new ShogiBoard(3, new Vector2(myTopStageObj.transform.position.x,
                                                   myTopStageObj.transform.position.z));
        opponentTopStage = new ShogiBoard(3, new Vector2(oppenentsTopStageObj.transform.position.x,
                                                         oppenentsTopStageObj.transform.position.z));
        //初期位置は塗っておく
        for (int i = 1; i <= 5; i++)
        {
            BoardIndex index = new BoardIndex(i, 5);
            PaintCell(index, true);
        }

        //すでに部屋に相手がいる場合はゲーム開始します
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            machingPanel.SetActive(false);
            StartCoroutine(StartGameSequence());
        }
        else
        {
            machingPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 自分のターンを開始する
    /// </summary>
    private void StartTurn()
    {
        /* 自分のコマをアクティブにする */
        List<BoardIndex> indexList = board.GetMyTopBoardIndex();

        foreach (BoardIndex index in indexList)
        {
            GameObject obj = board.GetObjByIndex(index);
            obj.GetComponent<BattleTopCtrl>().SetIsMyTurn(true);
        }

        //自分の持ち駒をすべてアクティブにする
        List<GameObject> objList = myTopStage.GetGameObjectAll();

        foreach (GameObject obj in objList)
        {
            obj.GetComponent<BattleTopCtrl>().SetIsMyTurn(true);
        }
    }

    /// <summary>
    /// 指定されたセルをハイライトする
    /// </summary>
    /// <param name="indexies"></param>
    public void CellHighLight(List<BoardIndex> indexies)
    {
        //盤面黄色のみ初期化
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            for (int j = 1; j <= GameDef.BOARD_CELLS; j++)
            {
                yellowCellObj[i, j].transform.position =
                    new Vector3(yellowCellObj[i, j].transform.position.x,
                                0.5f,
                                yellowCellObj[i, j].transform.position.z);
            }
        }

        //指定インデックスの箇所のみを浮かせる
        foreach (BoardIndex index in indexies)
        {
            //黄色いセルを浮上させる
            yellowCellObj[index.xIndex, index.yIndex].transform.position =
                new Vector3(yellowCellObj[index.xIndex, index.yIndex].transform.position.x,
                            1.01f,
                            yellowCellObj[index.xIndex, index.yIndex].transform.position.z);
        }
    }

    /// <summary>
    /// 自分のターンを終了する
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    /// <param name="IsBring"></param>
    public void TurnEnd(BoardIndex source, BoardIndex destination, bool IsBring)
    {
        /* 自分のコマを非アクティブにする */
        List<BoardIndex> indexList = board.GetMyTopBoardIndex();

        foreach (BoardIndex index in indexList)
        {
            GameObject obj = board.GetObjByIndex(index);
            obj.GetComponent<BattleTopCtrl>().SetIsMyTurn(false);
        }

        //自分の持ち駒をすべてアクティブにする
        List<GameObject> objList = myTopStage.GetGameObjectAll();

        foreach (GameObject obj in objList)
        {
            obj.GetComponent<BattleTopCtrl>().SetIsMyTurn(false);
        }

        /* ターン情報通知 */
        TopMoveInf inf = new TopMoveInf(source, destination, IsBring);
        float move = ConvMoveInf(inf);
        Debug.Log(move);
        //自分のターンはここで終了
        turnManager.SendMove(move, true);
    }

    /// <summary>
    /// 盤面アップデート
    /// </summary>
    /// <param name="inf"></param>
    private void BoardUpDate(TopMoveInf inf)
    {
        BoardIndex source;

        //持ち駒かどうかによってソースは変わる
        if (inf.IsBring)
        {
            source = ComvOppenentIndexChild(inf.source);
        }
        else
        {
            source = ComvOpponentIndex(inf.source);
        }
        BoardIndex distination = ComvOpponentIndex(inf.destination);


        //行先にコマがあれば削除して相手の持ち駒とする
        if (board.ChkBoardTop(distination))
        {
            GameObject obj = board.GetObjByIndex(distination);
            board.DelBoardInf(distination);

            int id = obj.GetComponent<BattleTopCtrl>().topId;
            LostATop(obj);
        }

        //盤面情報更新
        if (inf.IsBring)
        {
            GameObject obj = opponentTopStage.GetObjByIndex(source);
            //相手からの情報なので相手の持ち駒を削除
            opponentTopStage.DelBoardInf(source);

            int topId = obj.GetComponent<BattleTopCtrl>().topId;
            Vector2 tmpVec = board.GetBoardPosByIndex(distination);
            obj.transform.position = new Vector3(tmpVec.x, 1, tmpVec.y);
            board.SetBoardInf(topId, obj, false, distination);
            PaintCell(new BoardIndex(distination.xIndex, distination.yIndex), false);
        }
        else
        {
            GameObject obj = board.GetObjByIndex(source);

            board.DelBoardInf(source);

            int topId = obj.GetComponent<BattleTopCtrl>().topId;
            Vector2 tmpVec = board.GetBoardPosByIndex(distination);
            obj.transform.position = new Vector3(tmpVec.x, 1,tmpVec.y);
            board.SetBoardInf(topId, obj, false, distination);

            //とびがないかチェック(狙撃手はとび確認不要)
            if (topId != 9)
            {
                List<BoardIndex> indexList =
                board.GetCellIndexBitweenCells(source, distination);

                foreach (BoardIndex index in indexList)
                {
                    PaintCell(index, false);
                }
            }
            PaintCell(new BoardIndex(distination.xIndex, distination.yIndex), false);
        }
    }

    /// <summary>
    /// 指定の位置にコマを置く
    /// </summary>
    /// <param name="topId"></param>
    /// <param name="obj"></param>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    public void PutATop(int topId, GameObject obj, BoardIndex index)
    {
        board.SetBoardInf(topId, obj, true, index);
    }

    /// <summary>
    /// コマを獲得する
    /// </summary>
    public void GetATop(GameObject obj)
    {
        int id = obj.GetComponent<BattleTopCtrl>().topId;

        //持ち駒リストに追加する
        for (int i = 3; i >= 1; i--)
        {
            for (int j = 3; j >= 1; j--)
            {
                BoardIndex index = new BoardIndex(j, i);
                //コマがなければそこに移動
                if (!myTopStage.ChkBoardTop(index))
                {
                    myTopStage.SetBoardInf(id, obj, true, index);
                    Vector2 tmpPos = myTopStage.GetBoardPosByIndexChild(index);

                    //オブジェクトを移動
                    obj.transform.position = new Vector3(tmpPos.x, 1, tmpPos.y);
                    obj.GetComponent<BattleTopCtrl>().SetIsMine(true);
                    obj.GetComponent<BattleTopCtrl>().SetIndex(index);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// コマを失う
    /// </summary>
    /// <param name="obj"></param>
    public void LostATop(GameObject obj)
    {
        int id = obj.GetComponent<BattleTopCtrl>().topId;

        //持ち駒リストに追加する
        for (int i = 1; i <= 3; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                BoardIndex index = new BoardIndex(j, i);
                //コマがなければそこに移動
                if (!opponentTopStage.ChkBoardTop(index))
                {
                    opponentTopStage.SetBoardInf(id, obj, false, index);
                    Vector2 tmpPos = opponentTopStage.GetBoardPosByIndexChild(index);

                    //オブジェクトを移動
                    obj.transform.position = new Vector3(tmpPos.x, 1, tmpPos.y);
                    obj.GetComponent<BattleTopCtrl>().SetIsMine(false);
                    obj.GetComponent<BattleTopCtrl>().SetIndex(index);
                    return;
                }
            }
        }
    }

    /// <summary>
    /// コマを使う
    /// </summary>
    /// <param name="index"></param>
    public void UseATop(BoardIndex index)
    {
        myTopStage.DelBoardInf(index);
    }

    /// <summary>
    /// ゲーム開始要求
    /// </summary>
    public void RequireGameStart()
    {
        //マスタークライアントがターンを開始する
        if (PhotonNetwork.IsMasterClient)
        {
            //マスタークライアントが先攻後攻を振り分ける
            int prio = Random.Range(0, 2);
            //ルームのカスタムプロパティを書き換える
            var hashtable = new ExitGames.Client.Photon.Hashtable()
            {
                ["PrioIndex"] = prio
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);

            /* ルームのカスタムプロパティ変更のコールバックでゲームスタートします */
        }
    }

    /// <summary>
    /// 将棋盤クラスを参照する
    /// </summary>
    /// <param name="board"></param>
    public void RefShogiBoard(ref ShogiBoard board)
    {
        board = this.board;
    }

    /// <summary>
    /// ゲームスタートシーケンス
    /// </summary>
    /// <returns></returns>
    public IEnumerator StartGameSequence()
    {
        //マスタークライアントが部屋を締め切る
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }

        //対戦相手表示
        PopupPanel.SetActive(true);
        popupText.GetComponent<Text>().text = "対戦準備中..";
        yield return new WaitForSeconds(1);

        //プレイヤー名を交換する
        var hashtable1 = new ExitGames.Client.Photon.Hashtable()
        {
            ["Name"] = PhotonNetwork.LocalPlayer.NickName
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable1);

        popupText.GetComponent<Text>().text = "配置情報を交換しています..";
        yield return new WaitForSeconds(1);

        //盤面情報を交換する
        var hashtable2 = new ExitGames.Client.Photon.Hashtable()
        {
            ["AlignInfo"] = alignInfo
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable2);

        //ターン開始
        RequireGameStart();
        yield break;
    }

    /// <summary>
    /// ポップアップフェードアウト
    /// </summary>
    /// <returns></returns>
    public IEnumerator PopupFedOut()
    {
        //対戦相手表示
        yield return new WaitForSeconds(2);
        PopupPanel.SetActive(false);

        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            //マスタークライアントがターン開始
            turnManager.BeginTurn();
        }

        yield break;
    }

    /// <summary>
    /// エンドゲームシーケンス
    /// </summary>
    /// <returns></returns>
    public IEnumerator EndGameSequence()
    {
        //結果表示
        Debug.Log("3秒後にメニューへ戻ります");
        PopupPanel.SetActive(true);

        if (board.ChkWinner())
        {
            popupText.text = "あなたのかちです。";
        }
        else
        {
            popupText.text = "あなたのまけです";
        }
        yield return new WaitForSeconds(2);
        popupText.text = "1000コインが追加されます";
        GameManager.Instance.RefPlayerInfo(ref playerInfo);
        playerInfo.coins += 1000;
        yield return new WaitForSeconds(2);
        popupText.text = "メニューへ戻ります";
        yield return new WaitForSeconds(3);
        //ルームから出る
        PhotonNetwork.LeaveRoom();
        //メニューシーンへ
        GameManager.Instance.LoadScene("MenuScene");

        yield break;
    }

    /// <summary>
    /// 対局相手退出シーケンス
    /// </summary>
    /// <returns></returns>
    public IEnumerator OpponentOutSequence()
    {
        //結果表示
        PopupPanel.SetActive(true);

        popupText.text = "相手が退室しました";

        yield return new WaitForSeconds(3);
        popupText.text = "メニューへ戻ります";
        yield return new WaitForSeconds(3);
        //ルームから出る
        PhotonNetwork.LeaveRoom();
        //メニューシーンへ
        GameManager.Instance.LoadScene("MenuScene");

        yield break;
    }

    /// <summary>
    /// 盤面に相手の配置情報をマージする
    /// </summary>
    /// <param name="alignInfo"></param>
    private void MargeOpponentBoard(int[] alignInfo)
    {
        GameObject topPref = Resources.Load<GameObject>("GameObjects/BattleScene/Top");

        for (int i = 1; i < 6; i++)
        {
            if (alignInfo[i] == 0)
            {
                continue;
            }

            GameObject top = Instantiate(topPref);

            //マテリアル設定
            Material[] mats = top.GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == alignInfo[i]).AssetName);
            top.GetComponent<MeshRenderer>().materials = mats;

            BattleTopCtrl topCtrl = top.GetComponent<BattleTopCtrl>();
            BoardIndex index = new BoardIndex((GameDef.BOARD_CELLS + 1) - i, 1);
            Vector2 tmpVec = board.GetBoardPosByIndex(index);
            top.transform.position = new Vector3(tmpVec.x, 1, tmpVec.y);
            topCtrl.topId = alignInfo[i];
            topCtrl.SetIsMine(false);
            topCtrl.SetIsMyTurn(false);
            board.SetBoardInf(alignInfo[i], top, false, index);
        }

        //初期位置は塗っておく
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            BoardIndex index = new BoardIndex(i, 1);
            PaintCell(index, false);
        }
    }

    /// <summary>
    /// 相手の盤面インデックスを変換する
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private BoardIndex ComvOpponentIndex(BoardIndex index)
    {
        return new BoardIndex((GameDef.BOARD_CELLS + 1) - index.xIndex,
                              (GameDef.BOARD_CELLS + 1) - index.yIndex);
    }

    /// <summary>
    /// 相手の持ち駒インデックスを変換する
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    private BoardIndex ComvOppenentIndexChild(BoardIndex index)
    {
        return new BoardIndex((GameDef.BOARD_CHILD_CELLS + 1) - index.xIndex,
                              (GameDef.BOARD_CHILD_CELLS + 1) - index.yIndex);
    }

    /// <summary>
    /// 指定されたセルを塗る
    /// </summary>
    public void PaintCell(BoardIndex index, bool isMine)
    {
        if (isMine)
        {
            //赤いセルを浮上させる
            redCellObj[index.xIndex, index.yIndex].transform.position =
                new Vector3(redCellObj[index.xIndex, index.yIndex].transform.position.x,
                            1.005f,
                            redCellObj[index.xIndex, index.yIndex].transform.position.z);

            //青いセルを沈める
            blueCellObj[index.xIndex, index.yIndex].transform.position =
                new Vector3(blueCellObj[index.xIndex, index.yIndex].transform.position.x,
                            0.5f,
                            blueCellObj[index.xIndex, index.yIndex].transform.position.z);
        }
        else
        {
            //赤いセルを沈める
            redCellObj[index.xIndex, index.yIndex].transform.position =
                new Vector3(redCellObj[index.xIndex, index.yIndex].transform.position.x,
                            0.5f,
                            redCellObj[index.xIndex, index.yIndex].transform.position.z);

            //青いセルを浮上させる
            blueCellObj[index.xIndex, index.yIndex].transform.position =
                new Vector3(blueCellObj[index.xIndex, index.yIndex].transform.position.x,
                            1.005f,
                            blueCellObj[index.xIndex, index.yIndex].transform.position.z);
        }

        //ボード情報に登録する
        board.SetBoardColor(index, isMine);
    }

    /// <summary>
    /// 移動情報をfloat型に変換する
    /// </summary>
    /// <param name="inf"></param>
    /// <returns></returns>
    public float ConvMoveInf(TopMoveInf inf)
    {
        float i = 0x00000000;

        if (inf.IsBring)
        {
            i += 1 * Mathf.Pow(0x10, 4);
        }
        else
        {
            i += 2 * Mathf.Pow(0x10, 4);
        }

        i += inf.source.xIndex * Mathf.Pow(0x10, 3);      /* 4桁目 */
        i += inf.source.yIndex * Mathf.Pow(0x10, 2);      /* 3桁目 */
        i += inf.destination.xIndex * Mathf.Pow(0x10, 1); /* 2桁目 */
        i += inf.destination.yIndex * Mathf.Pow(0x10, 0); /* 1桁目 */

        return i;
    }

    /// <summary>
    /// float情報から移動情報に変換する
    /// </summary>
    /// <param name="inf"></param>
    /// <returns></returns>
    public TopMoveInf ConvSetMoveInf(float inf)
    {
        int destinationY = (int)(inf % 0x10);
        inf -= destinationY;
        inf /= 0x10;
        int destinationX = (int)(inf % 0x10);
        inf -= destinationX;
        inf /= 0x10;
        int sourceY = (int)(inf % 0x10);
        inf -= sourceY;
        inf /= 0x10;
        int sourceX = (int)(inf % 0x10);
        inf -= sourceX;
        inf /= 0x10;

        bool isBring;

        if (inf == 1)
        {
            isBring = true;
        }
        else
        {
            isBring = false;
        }
        BoardIndex source = new BoardIndex(sourceX, sourceY);
        BoardIndex destination = new BoardIndex(destinationX, destinationY);
        return new TopMoveInf(source, destination, isBring);
    }

    /// <summary>
    /// メニューボタンを押したときの処理
    /// </summary>
    public void PushMenuButton()
    {
        //ルームから出る
        PhotonNetwork.LeaveRoom();
        //メニューシーンへ
        GameManager.Instance.LoadScene("MenuScene");

    }

    /// <summary>
    /// ターンスタート可能か確認する
    /// </summary>
    /// <returns></returns>
    private bool IsAbleTurnStart()
    {
        int indexListCnt = 0;

        List<GameObject> myObjList = myTopStage.GetGameObjectAll();
        List<BoardIndex> boardObjList = board.GetMyTopBoardIndex();

        foreach (BoardIndex index in boardObjList)
        {
            int id = board.GetTopIdByIndex(index);
            List<BoardIndex> indexList = board.GetMovableIndex(id, index);

            foreach (BoardIndex index2 in indexList)
            {
                indexListCnt++;
            }
        }

        if (myObjList.Count == 0 && indexListCnt == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    /*★                         Pun Callback List                          ★*/
    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    /* Photonに接続したとき */
    public override void OnConnected()
    {
        Debug.Log("ネットワークに接続しました。");
    }

    /* Photonから切断されたとき */
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("ネットワークから切断されました。");
    }

    /* マスターサーバへ接続したとき */
    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバに接続しました。");
    }

    /* ロビーに入ったとき */
    public override void OnJoinedLobby()
    {
        Debug.Log("ロビーに入りました。");

    }

    /* ロビーから出たとき */
    public override void OnLeftLobby()
    {
        Debug.Log("ロビーから出ました。");
    }

    /* 部屋を作成したとき */
    public override void OnCreatedRoom()
    {
        Debug.Log("部屋を作成しました。");

        GameManager.Instance.LoadScene("BattleScene");
    }

    /* 部屋の作成に失敗したとき */
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("部屋の作成に失敗しました。");
    }

    /* 部屋に入室したとき */
    public override void OnJoinedRoom()
    {
        Debug.Log("部屋に入室しました。");
    }

    /* 特定の部屋への入室に失敗したとき */
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("部屋の入室に失敗しました。");
    }

    /* ランダムな部屋の入室に失敗したとき */
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("ランダムな部屋の入室に失敗しました。");
    }

    /* 部屋から退室したとき */
    public override void OnLeftRoom()
    {
        Debug.Log("部屋から退室しました。");
    }

    /* 他のプレイヤーが入室したとき */
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("他のプレイヤーが入室してきました。");

        //ルームのプレイヤー数が2人であればゲーム開始します
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("ゲーム開始");
            machingPanel.SetActive(false);
            OpponentPanel.transform.Find("NameText").GetComponent<Text>().text = newPlayer.NickName;
            StartCoroutine(StartGameSequence());
        }
    }

    /* 他のプレイヤーが退室したとき */
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("他のプレイヤーが退室しました。");
        StartCoroutine(OpponentOutSequence());
    }

    /* マスタークライアントが変わったとき */
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("マスタークライアントが変更されました。");
    }

    /* ロビーに更新があったとき */
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        Debug.Log("ロビーが更新されました。");
    }

    /* ルームリストに更新があったとき */
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("ルームリストに更新がありました。");
    }


    /* フレンドリストに更新があったとき */
    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log("フレンドリストに更新がありました。");
    }

    /* 地域リストを受け取ったとき */
    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("地域リストを取得しました。");
    }

    /* WebRpcのレスポンスがあったとき */
    //public override void OnWebRpcResponse(OperationResponse response)
    //{
    //    Debug.Log("WebRcpの応答を検出しました。");
    //}

    /* カスタム認証のレスポンスがあったとき */
    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log("カスタム認証の応答を検出しました。");
    }

    /* カスタム認証に失敗したとき */
    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("カスタム認証に失敗しました。");
    }

    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    /*★                     TurnManager Callback List                      ★*/
    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    //ターンが開始したとき
    public void OnTurnBegins(int turn)
    {

        //盤面がすべて塗られていればゲーム終了
        if (board.ChkBoardColorAll())
        {
            StartCoroutine(EndGameSequence());
        }

        Debug.Log(turn +"ターン目開始");
        //ターンは1から開始されるので-1しておく
        int activePlayerIndex = (turn - 1) % 2;

        if (myTurnIndex == activePlayerIndex)
        {
            //自分のターン処理
            if (IsAbleTurnStart())
            {
                StartTurn();
                MyPanel.transform.Find("EventText").GetComponent<Text>().text = "あなたのターンです";
                OpponentPanel.transform.Find("EventText").GetComponent<Text>().text = "";
            }
            else
            {
                turnManager.SendMove(0, true);
            }
        }
        else
        {
            //相手のターン中は何もしないのでそのままターンエンド
            turnManager.SendMove(0, true);
            MyPanel.transform.Find("EventText").GetComponent<Text>().text = "";
            OpponentPanel.transform.Find("EventText").GetComponent<Text>().text = "思考中...";
        }
    }

    //ターンのムーブが全プレイヤー完了したとき
    public void OnTurnCompleted(int turn)
    {
        Debug.Log("全プレイヤー完了した");
        if (PhotonNetwork.IsMasterClient)
        {
            turnManager.BeginTurn();
        }
    }

    //プレイヤーのムーブを開始したとき
    public void OnPlayerMove(Player player, int turn, object move)
    {

    }

    //プレイヤーのムーブが終了したとき
    public void OnPlayerFinished(Player player, int turn, object move)
    {
        //ターンは1から開始されるので-1しておく
        int activePlayerIndex = (turn - 1) % 2;

        /* 自分のターンでなければに相手からのメッセージを受信 */
        if (myTurnIndex != activePlayerIndex)
        {
            //送信されてきたデータをもとにオブジェクトを動かす
            if (player != PhotonNetwork.LocalPlayer)
            {
                float inf = (float)move;
                if (inf != 0)
                {
                    TopMoveInf moveInf = ConvSetMoveInf(inf);
                    BoardUpDate(moveInf);
                }
            }
        }
    }

    //タイマーがタイムアウトしたときに処理したい内容を書く
    public void OnTurnTimeEnds(int turn)
    {

    }

    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    /*★                         Pun Callback List                          ★*/
    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    //プレイヤーのカスタムプロパティの値が変更されたとき
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (changedProps.TryGetValue("Name", out object name))
        {
            if (targetPlayer != PhotonNetwork.LocalPlayer)
            {
                OpponentPanel.transform.Find("NameText").GetComponent<Text>().text = targetPlayer.NickName;
                MyPanel.transform.Find("NameText").GetComponent<Text>().text = PhotonNetwork.LocalPlayer.NickName;
            }
        }

        //盤面の初期配置情報が通知された場合
        if (changedProps.TryGetValue("AlignInfo", out object alignInfo))
        {
            //通知元が相手プレイヤーであれば盤面情報をマージする
            if (targetPlayer != PhotonNetwork.LocalPlayer)
            {
                opponentAlignInfo = (int[])alignInfo;
                MargeOpponentBoard(opponentAlignInfo);
            }
        }
    }

    /* ルームプロパティが更新されたとき */
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("ルームプロパティが更新されたとき");
        if (propertiesThatChanged.TryGetValue("PrioIndex", out object prioIndex))
        {
            int tmpVal = (int)prioIndex;
            if (PhotonNetwork.IsMasterClient)
            {
                if (tmpVal == 0)
                {
                    myTurnIndex = 0;
                }
                else
                {
                    myTurnIndex = 1;
                }
            }
            else
            {
                if (tmpVal == 0)
                {
                    myTurnIndex = 1;
                }
                else
                {
                    myTurnIndex = 0;
                }
            }

            if (myTurnIndex == 0)
            {
                popupText.GetComponent<Text>().text = "あなたが先手です..";
                MyPanel.transform.Find("PrioText").GetComponent<Text>().text = "先手";
                OpponentPanel.transform.Find("PrioText").GetComponent<Text>().text = "後手";
            }
            else
            {
                popupText.GetComponent<Text>().text = "あなたが後手です..";
                MyPanel.transform.Find("PrioText").GetComponent<Text>().text = "後手";
                OpponentPanel.transform.Find("PrioText").GetComponent<Text>().text = "先手";
            }

            StartCoroutine(PopupFedOut());
        }
    }
}
