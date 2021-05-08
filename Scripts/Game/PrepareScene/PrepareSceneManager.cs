using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrepareSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject shogiBoard = default; 
    [SerializeField]
    private Transform content = default;

    Dictionary<int, int> topDic;         /* 所持コマ情報     */
    private int[] topArignInfo;          /* コマ配置情報     */
    private ShogiBoard board;            /* 将棋盤情報       */
    private Text[] topCntText;           /* 持ち駒数テキスト */
    private GameObject[,] redCellObj;    /* 赤いセル         */
    private GameObject[,] yellowCellObj; /* 黄色いセル       */

    void Start()
    {
        /* 変数初期化 */
        List<TopTable> topTableList = new List<TopTable>();
        GameManager.Instance.RefTopTableList(ref topTableList);

        /* 初期情報登録 */
        topDic = GameManager.Instance.GetPossessingTops();
        GameManager.Instance.RefTopAlignInfo(ref topArignInfo);
        board = new ShogiBoard(GameDef.BOARD_CELLS, shogiBoard.transform.position);
        topCntText = new Text[topTableList.Count + 1];
        redCellObj = new GameObject[GameDef.BOARD_CELLS + 1, GameDef.BOARD_CELLS + 1];
        yellowCellObj = new GameObject[GameDef.BOARD_CELLS + 1, GameDef.BOARD_CELLS + 1];

        /* ロードリソース */
        GameObject image = Resources.Load<GameObject>("GameObjects/Image");
        GameObject itemPref = Resources.Load<GameObject>("GameObjects/PrepareScene/Tops/Top");
        GameObject topCntTextObj = Resources.Load<GameObject>("UI/Text");
        GameObject redCellPref = Resources.Load<GameObject>("GameObjects/PrepareScene/RedCellObject");
        GameObject yellowCellPref = Resources.Load<GameObject>("GameObjects/PrepareScene/YellowCellObject");

        /* 所持コマ情報をもとにScrollViewにコマを格納する */
        foreach (TopTable top in topTableList)
        {
            //所持コマ0個の場合はimageを生成しない
            if (topDic[top.Id] == 0)
            {
                continue;
            }

            //imageを生成する
            GameObject tmp = Instantiate(image);
            Sprite itemSprite = Resources.Load<Sprite>("Images/" + top.AssetName);
            tmp.GetComponent<Image>().sprite = itemSprite;
            tmp.transform.SetParent(content);
            tmp.transform.localPosition = new Vector3(tmp.transform.position.x,
                                                      tmp.transform.position.y,
                                                      0);
            tmp.transform.localScale = new Vector3(1, 1, 1);
            tmp.AddComponent<ContentClickListener>().topId = top.Id;

            //textを生成する
            GameObject tmpTextObj = Instantiate(topCntTextObj);
            topCntText[top.Id] = tmpTextObj.GetComponent<Text>();
            tmpTextObj.transform.SetParent(tmp.transform);
            tmpTextObj.transform.localScale = new Vector3(1,1,1);
            tmpTextObj.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(0, 7.6f, 0);
            topCntText[top.Id].text = "×  " + topDic[top.Id];
        }

        /* コマ配置情報をもとにコマを再配置する */
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            if (topArignInfo[i] == 0)
            {
                continue;
            }

            GameObject top = Instantiate(itemPref);

            //マテリアル設定
            Material[] mats = top.GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == topArignInfo[i]).AssetName);
            top.GetComponent<MeshRenderer>().materials = mats;

            BoardIndex index = new BoardIndex(i, GameDef.BOARD_CELLS);
            Vector2 tmpVec = board.GetBoardPosByIndex(index);
            top.transform.position = new Vector3(tmpVec.x, tmpVec.y, -6.85f);
            PrepareTopCtrl prepareTopCtrl = top.GetComponent<PrepareTopCtrl>();
            prepareTopCtrl.topId = topArignInfo[i];
            prepareTopCtrl.movePermitFlg = false;
            prepareTopCtrl.SetTopIndex(index);
            board.SetBoardInf(topArignInfo[i], top, true, index);
        }

        /* カラーセルを配置する */
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            for (int j = 1; j <= GameDef.BOARD_CELLS; j++)
            {
                BoardIndex index = new BoardIndex(i, j);
                Vector2 tmpVal = board.GetBoardPosByIndex(index);
                Vector3 tmpVal2 = new Vector3(tmpVal.x, tmpVal.y, -6.85f);

                //色付きセル(黄色、赤)を配置する
                GameObject redCell = Instantiate(redCellPref, tmpVal2, Quaternion.identity);
                redCellObj[i, j] = redCell;
                GameObject yellowCell = Instantiate(yellowCellPref, tmpVal2, Quaternion.identity);
                yellowCellObj[i, j] = yellowCell;

                //赤セルのみを少し下げておく
                redCellObj[i, j].transform.position = new Vector3(redCellObj[i, j].transform.position.x,
                                                                  redCellObj[i, j].transform.position.y,
                                                                  -6.0f);
            }
        }
    }

    /// <summary>
    /// 将棋盤情報を参照する
    /// </summary>
    /// <param name="board"></param>
    public void RefShogiBoard(ref ShogiBoard board)
    {
        board = this.board;
    }

    /// <summary>
    /// 盤面にコマを置く
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
    /// 持ち駒情報からコマを引き出し可能かチェック
    /// </summary>
    /// <param name="topId"></param>
    /// <returns></returns>
    public bool ChkPullOut(int topId)
    {
        if (topDic[topId] == 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 持ち駒情報からコマを引き出す
    /// </summary>
    /// <param name="topId"></param>
    public void PullOutATop(int topId)
    {
        topDic[topId]--;
        topCntText[topId].text = "×  " + topDic[topId];
    }

    /// <summary>
    /// 持ち駒情報にコマをしまう
    /// </summary>
    /// <param name="topId"></param>
    public void EndOutATop(int topId)
    {
        topDic[topId]++;
        topCntText[topId].text = "×  " + topDic[topId];
    }

    /// <summary>
    /// 盤面のハイライトを行う
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    public void CellHighLight(List<BoardIndex> indexies)
    {
        //盤面初期化
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            for (int j = 1; j <= GameDef.BOARD_CELLS; j++)
            {
                redCellObj[i, j].transform.position =
                    new Vector3(redCellObj[i, j].transform.position.x,
                                redCellObj[i, j].transform.position.y,
                                -6f);

                yellowCellObj[i, j].transform.position =
                    new Vector3(yellowCellObj[i, j].transform.position.x,
                                yellowCellObj[i, j].transform.position.y,
                                -6.85f);
            }
        }

        //指定インデックスの箇所のみを浮かせる
        foreach (BoardIndex index in indexies)
        {
            //赤いセルを浮上させる
            redCellObj[index.xIndex, index.yIndex].transform.position =
                new Vector3(redCellObj[index.xIndex, index.yIndex].transform.position.x,
                            redCellObj[index.xIndex, index.yIndex].transform.position.y,
                            -6.86f);
        }
    }

    /// <summary>
    /// メニューボタンを押したときの処理
    /// </summary>
    public void PushMenuButton()
    {
        //コマ配置情報に登録
        for (int i = 1; i <= GameDef.BOARD_CELLS; i++)
        {
            BoardIndex index = new BoardIndex(i, GameDef.BOARD_CELLS);
            int topId = board.GetTopIdByIndex(index);
            topArignInfo[i] = topId;
        }

        GameManager.Instance.SetAlignInfo(topArignInfo);

        GameManager.Instance.LoadScene("MenuScene");
    }
}
