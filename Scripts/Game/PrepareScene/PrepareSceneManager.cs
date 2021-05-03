using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrepareSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject shogiBoard;
    
    [SerializeField]
    private Transform content = default;

    private PossessingItems items;   /* 所持コマ情報 */
    Dictionary<int, int> topDic = new Dictionary<int, int>();
    private int[] topArignInfo;      /* コマ配置情報 */
    private ShogiBoard board;        /* 将棋盤情報   */
    private int boardSize;           /* 盤面サイズ   */
    private Text[] topCntText;       /* 持ち駒数テキスト */
    private GameObject[,] redCellObj; /* 赤いセル     */
    private GameObject[,] blueCellObj; /* 赤いセル     */
    private GameObject[,] yellowCellObj; /* 黄色いセル     */

    void Start()
    {
        List<TopTable> topTableList = new List<TopTable>();
        GameManager.Instance.RefPossessingItems(ref items);
        GameManager.Instance.RefTopTableList(ref topTableList);
        GameObject image = Resources.Load<GameObject>("GameObjects/Image");
        Sprite[] itemSprites = Resources.LoadAll<Sprite>("Images");
        GameObject topCntTextObj = Resources.Load<GameObject>("UI/Text");
        topCntText = new Text[11];//コマ種別数と一致させておく
        boardSize = GameManager.Instance.GetShogiBoard();
        GameManager.Instance.RefTopAlignInfo(ref topArignInfo);
        board = new ShogiBoard(boardSize, shogiBoard.transform.position);
        redCellObj = new GameObject[boardSize + 1, boardSize + 1];
        yellowCellObj = new GameObject[boardSize + 1, boardSize + 1];
        GameObject redCellPref = Resources.Load<GameObject>("GameObjects/PrepareScene/RedCellObject");
        GameObject yellowCellPref = Resources.Load<GameObject>("GameObjects/PrepareScene/YellowCellObject");

        GameObject itemPref = Resources.Load<GameObject>("GameObjects/PrepareScene/Tops/Top");

        //所持コマ情報をもとにScrollViewにコマを格納する
        Dictionary<int, int> tmpTopDic = items.RefTopDic();
        topDic = tmpTopDic;
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
            //imageの親にcontentを指定する
            tmp.transform.SetParent(content);
            tmp.transform.localPosition = new Vector3(tmp.transform.position.x,
                                                      tmp.transform.position.y,
                                                      0);
            tmp.transform.localScale = new Vector3(1, 1, 1);
            tmp.AddComponent<ContentClickListener>().topId = top.Id;

            //textを生成する
            GameObject tmpTextObj = Instantiate(topCntTextObj);
            topCntText[top.Id] = tmpTextObj.GetComponent<Text>();
            //textの親にimageを指定する
            tmpTextObj.transform.SetParent(tmp.transform);
            tmpTextObj.transform.localScale = new Vector3(1,1,1);
            /* ここは後で直したい */
            tmpTextObj.transform.localPosition = new Vector3(0, 60, 0);
            topCntText[top.Id].text = "×  " + topDic[top.Id];
        }

        //コマ配置情報をもとにコマを再配置する
        for (int i = 1; i <= boardSize; i++)
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

            Vector2 tmpVec = board.GetBoardPosByIndex(i, boardSize);
            top.transform.position = new Vector3(tmpVec.x, tmpVec.y, -6.85f);
            PrepareTopCtrl prepareTopCtrl = top.GetComponent<PrepareTopCtrl>();
            prepareTopCtrl.topId = topArignInfo[i];
            prepareTopCtrl.movePermitFlg = false;
            prepareTopCtrl.xIndex = i;
            prepareTopCtrl.yIndex = 5;
            board.SetBoardInf(topArignInfo[i], top, i, boardSize);

            //コマを配置したら持ち駒情報から減らす
            //topDic[topArignInfo[i]]--;
            //topCntText[topArignInfo[i]].text = "×  " + topDic[topArignInfo[i]];
        }

        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
            {
                Vector2 tmpVal = board.GetBoardPosByIndex(i, j);
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


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log(Input.mousePosition);
        }
    }

    public void RefShogiBoard(ref ShogiBoard board)
    {
        board = this.board;
    }

    public void PutATop(int topId, GameObject obj, int xIndex, int yIndex)
    {
        board.SetBoardInf(topId, obj, xIndex, yIndex);
    }

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

    public void PullOutATop(int topId)
    {
        topDic[topId]--;
        topCntText[topId].text = "×  " + topDic[topId];
    }


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
        for (int i = 1; i <= boardSize; i++)
        {
            for (int j = 1; j <= boardSize; j++)
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
        for (int i = 1; i <= boardSize; i++)
        {
            int topId = board.GetTopIdByIndex(i, boardSize);
            topArignInfo[i] = topId;

            //減らした分つじつま合わせ
            //5行目にしか置けないのでここだけ回収しとけばいいはず
            if (topId != 0)
            {
                topDic[topArignInfo[i]]++;
            }
        }

        GameManager.Instance.SetAlignInfo(topArignInfo);

        GameManager.Instance.LoadScene("MenuScene");
    }
}
