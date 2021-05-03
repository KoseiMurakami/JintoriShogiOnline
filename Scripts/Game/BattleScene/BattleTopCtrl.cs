using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTopCtrl : MonoBehaviour
{
    private BattleSceneManager battleSceneManager;
    private ShogiBoard board;
    private Vector3 pos = new Vector3();
    private Vector2 tmpPos;
    private Vector2 tmpPos2;
    private Vector3 inputPos;
    private Vector3 movePos;
    private int xIndex;
    private int yIndex;
    private int befIndexX;
    private int befIndexY;

    public bool movePermitFlg;
    private bool reverseFlg;
    List<BoardIndex> indexies = new List<BoardIndex>();

    public int topId;
    public bool isMyTurn = false;
    public bool isMine = true;
    public bool isBring = false;
    private Vector3 scale;

    void Start()
    {
        battleSceneManager = FindObjectOfType<BattleSceneManager>();
        battleSceneManager.RefShogiBoard(ref board);
        movePermitFlg = false;
        reverseFlg = false;
        scale = transform.localScale;

        //自身のオブジェクトから盤面インデックスを把握しておく
        pos = gameObject.transform.position;
        board.GetIndexByPos(new Vector2(pos.x, pos.z), ref xIndex, ref yIndex);
    }

    // Update is called once per frame
    void Update()
    {
        /* 自分のターンかつ自分のコマのみ移動可能 */
        if (!isMyTurn || !isMine)
        {
            return;
        }

        //レイを飛ばして当たっていれば移動許可フラグを立てる
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                if (hit.transform.gameObject == this.gameObject)
                {
                    if (isBring)
                    {
                        movePermitFlg = true;
                        befIndexX = xIndex;
                        befIndexY = yIndex;
                        battleSceneManager.UseATop(new BoardIndex(xIndex, yIndex));

                        //空のインデックスをハイライトする
                        indexies = board.GetEmptyBoardIndex();

                        battleSceneManager.CellHighLight(indexies);
                        foreach (BoardIndex index in indexies)
                        {
                            Vector2 tmpVal = board.GetBoardPosByIndex(index.xIndex, index.yIndex);
                            Vector3 tmpVal2 = new Vector3(tmpVal.x, tmpVal.y, -6.85f);
                            Debug.Log(index.xIndex + ", " + index.yIndex);
                        }
                    }
                    //盤面上のコマを動かしたとき
                    else
                    {
                        movePermitFlg = true;
                        befIndexX = xIndex;
                        befIndexY = yIndex;
                        board.DelBoardInf(xIndex, yIndex);

                        //可動範囲をハイライトする
                        indexies = board.GetMovableIndex(topId, xIndex, yIndex);

                        battleSceneManager.CellHighLight(indexies);
                        foreach (BoardIndex index in indexies)
                        {
                            Vector2 tmpVal = board.GetBoardPosByIndex(index.xIndex, index.yIndex);
                            Vector3 tmpVal2 = new Vector3(tmpVal.x, tmpVal.y, -6.85f);
                            Debug.Log(index.xIndex + ", " + index.yIndex);
                        }
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (movePermitFlg)
            {
                inputPos = Input.mousePosition;
                this.pos = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 a = new Vector3(inputPos.x, inputPos.y, pos.z);
                this.pos = Camera.main.ScreenToWorldPoint(a);
                tmpPos = board.GetCellPosByPos(new Vector2(this.pos.x, this.pos.z), ref xIndex, ref yIndex);
                transform.position = new Vector3(tmpPos.x, 1, tmpPos.y);

                BoardIndex tmpIndex = new BoardIndex(xIndex, yIndex);

                //持っている間少しでかくする
                transform.localScale = 1.2f * scale;

                //範囲外の場合、またはコマを置くことができない場合はセルフィットしない
                if (this.pos.x < -1.0f ||
                    this.pos.x > 1.0f ||
                    this.pos.z < -1.0f ||
                    this.pos.z > 1.0f)
                {
                    reverseFlg = true;
                    transform.position = this.pos;
                    Debug.Log("1");
                }
                else if (!ChkIsMovableIndex(tmpIndex))
                {
                    reverseFlg = true;
                    transform.position = this.pos;
                    Debug.Log("2");
                }
                else
                {
                    Debug.Log("3");
                    reverseFlg = false;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (movePermitFlg)
            {
                //離したら大きさを戻す
                transform.localScale = scale;

                //盤面のハイライトを初期化しておく
                battleSceneManager.CellHighLight(new List<BoardIndex>());

                //リバース状態のときは元の位置に戻す
                if (reverseFlg)
                {
                    //コマ置き場→盤面
                    if (isBring)
                    {
                        Debug.Log("元の位置に戻す");
                        xIndex = befIndexX;
                        yIndex = befIndexY;
                        battleSceneManager.GetATop(gameObject);
                    }
                    //盤面→盤面
                    else
                    {
                        Debug.Log("元の位置に戻す");
                        xIndex = befIndexX;
                        yIndex = befIndexY;
                        battleSceneManager.PutATop(topId, gameObject, befIndexX, befIndexY);
                        Vector2 tmpVec = board.GetBoardPosByIndex(befIndexX, befIndexY);
                        gameObject.transform.position = new Vector3(tmpVec.x, gameObject.transform.position.y, tmpVec.y);
                    }
                }
                //ドロップ位置にコマがない場合、盤面情報に登録する
                else if (!board.ChkBoardTop(xIndex, yIndex))
                {
                    Debug.Log("保存しました");
                    battleSceneManager.PutATop(topId, gameObject, xIndex, yIndex);

                    //ターンエンド手続き
                    BoardIndex source = new BoardIndex(befIndexX, befIndexY);
                    BoardIndex destination = new BoardIndex(xIndex, yIndex);
                    battleSceneManager.TurnEnd(source, destination, isBring);

                    //とびがないかチェック(狙撃手はとび確認不要)
                    if (topId != 9)
                    {
                        List<BoardIndex> indexList =
                            board.GetCellIndexBitweenCells(source, destination);

                        foreach (BoardIndex index in indexList)
                        {
                            battleSceneManager.PaintCell(index, true);
                        }
                    }
                    battleSceneManager.PaintCell(destination, true);

                    //持ち駒を置くことができるのはここのパスのみ
                    if (isBring)
                    {
                        isBring = false;
                    }
                }
                //コマがある場合はそのコマを削除し、上書きする
                else
                {
                    Debug.Log("コマをとりました");
                    GameObject obj = board.DelBoardInf(xIndex, yIndex);
                    battleSceneManager.GetATop(obj);
                    battleSceneManager.PutATop(topId, gameObject, xIndex, yIndex);

                    //ターンエンド手続き
                    BoardIndex source = new BoardIndex(befIndexX, befIndexY);
                    BoardIndex destination = new BoardIndex(xIndex, yIndex);
                    battleSceneManager.TurnEnd(source, destination, false);

                    //とびがないかチェック(狙撃手はとび確認不要)
                    if (topId != 9)
                    {
                        List<BoardIndex> indexList =
                            board.GetCellIndexBitweenCells(source, destination);

                        foreach (BoardIndex index in indexList)
                        {
                            battleSceneManager.PaintCell(index, true);
                        }
                    }
                    battleSceneManager.PaintCell(destination, true);
                }

                movePermitFlg = false;
            }
        }
    }

    /// <summary>
    /// 指定インデックスは可動範囲内かどうか
    /// </summary>
    /// <param name="inputIndex"></param>
    /// <returns></returns>
    private bool ChkIsMovableIndex(BoardIndex inputIndex)
    {
        foreach (BoardIndex index in indexies)
        {
            if (index.xIndex == inputIndex.xIndex &&
                index.yIndex == inputIndex.yIndex)
            {
                return true;
            }
        }

        return false;
    }

    public void SetIsMyTurn(bool isMyTurn)
    {
        this.isMyTurn = isMyTurn;
    }

    public void SetIsMine(bool isMine)
    {
        if (this.isMine != isMine)
        {
            isBring = true;

            this.isMine = isMine;
        }

        if (isMine)
        {
            //180°
            gameObject.transform.localRotation = Quaternion.Euler(0, 180, 0);

        }
        else
        {
            //0°
            gameObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    /// <summary>
    /// 現在地情報を登録する
    /// </summary>
    /// <param name="index"></param>
    public void SetIndex(BoardIndex index)
    {
        xIndex = index.xIndex;
        yIndex = index.yIndex;
    }
}
