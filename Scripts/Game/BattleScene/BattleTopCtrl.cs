using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTopCtrl : MonoBehaviour
{
    private BattleSceneManager battleSceneManager;
    private ShogiBoard board;
    private Vector3 pos;
    private Vector2 tmpPos;
    private Vector3 inputPos;
    private BoardIndex index;
    private BoardIndex befIndex;

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
        index = new BoardIndex(0, 0);

        //自身のオブジェクトから盤面インデックスを把握しておく
        pos = gameObject.transform.position;
        board.GetIndexByPos(new Vector2(pos.x, pos.z), ref index);
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
                    movePermitFlg = true;
                    befIndex = index;
                    if (isBring)
                    {
                        battleSceneManager.UseATop(index);

                        //空のインデックスをすべて取得する
                        indexies = board.GetEmptyBoardIndex();
                    }
                    //盤面上のコマを動かしたとき
                    else
                    {
                        board.DelBoardInf(index);

                        //可動インデックスをすべて取得する
                        indexies = board.GetMovableIndex(topId, index);
                    }

                    //取得したインデックスをすべてハイライトする
                    battleSceneManager.CellHighLight(indexies);
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
                tmpPos = board.GetCellPosByPos(new Vector2(this.pos.x, this.pos.z), ref index);
                transform.position = new Vector3(tmpPos.x, 1, tmpPos.y);

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
                }
                else if (!ChkIsMovableIndex(index))
                {
                    reverseFlg = true;
                    transform.position = this.pos;
                }
                else
                {
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
                    index = befIndex;
                    //コマ置き場→盤面
                    if (isBring)
                    {
                        battleSceneManager.GetATop(gameObject);
                    }
                    //盤面→盤面
                    else
                    {
                        battleSceneManager.PutATop(topId, gameObject, befIndex);
                        Vector2 tmpVec = board.GetBoardPosByIndex(befIndex);
                        gameObject.transform.position = new Vector3(tmpVec.x, gameObject.transform.position.y, tmpVec.y);
                    }
                }
                //ドロップ位置にコマがない場合、盤面情報に登録する
                else if (!board.ChkBoardTop(index))
                {
                    battleSceneManager.PutATop(topId, gameObject, index);

                    //ターンエンド手続き
                    battleSceneManager.TurnEnd(befIndex, index, isBring);

                    //とびがないかチェック(狙撃手はとび確認不要)
                    if (topId != 9)
                    {
                        List<BoardIndex> indexList =
                            board.GetCellIndexBitweenCells(befIndex, index);

                        foreach (BoardIndex index in indexList)
                        {
                            battleSceneManager.PaintCell(index, true);
                        }
                    }
                    battleSceneManager.PaintCell(index, true);

                    //持ち駒を置くことができるのはここのパスのみ
                    if (isBring)
                    {
                        isBring = false;
                    }
                }
                //コマがある場合はそのコマを削除し、上書きする
                else
                {
                    GameObject obj = board.DelBoardInf(index);
                    battleSceneManager.GetATop(obj);
                    battleSceneManager.PutATop(topId, gameObject, index);

                    //ターンエンド手続き
                    battleSceneManager.TurnEnd(befIndex, index, false);

                    //とびがないかチェック(狙撃手はとび確認不要)
                    if (topId != 9)
                    {
                        List<BoardIndex> indexList =
                            board.GetCellIndexBitweenCells(befIndex, index);

                        foreach (BoardIndex index in indexList)
                        {
                            battleSceneManager.PaintCell(index, true);
                        }
                    }
                    battleSceneManager.PaintCell(index, true);
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
        this.index = index;
    }
}
