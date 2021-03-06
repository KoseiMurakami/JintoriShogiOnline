using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareTopCtrl : MonoBehaviour
{
    private PrepareSceneManager prepareSceneManager;
    private ShogiBoard board;
    private BoardIndex index;

    public int topId;
    public bool movePermitFlg;
    private bool hideFlg;
    List<BoardIndex> indexies = new List<BoardIndex>();
    private Vector3 scale;

    void Start()
    {
        prepareSceneManager = FindObjectOfType<PrepareSceneManager>();
        prepareSceneManager.RefShogiBoard(ref board);
        hideFlg = true;
        scale = transform.localScale;

        //コマ引き出し不可であれば即削除する
        if (!prepareSceneManager.ChkPullOut(topId))
        {
            Destroy(gameObject);
        }
        else
        {
            //コマを引き出した
            prepareSceneManager.PullOutATop(topId);
            //自身のオブジェクトから盤面インデックスを把握しておく
            Vector3 pos = gameObject.transform.position;
            board.GetIndexByPos(new Vector2(pos.x, pos.y), ref index);
        }

    }

    void Update()
    {
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
                    board.DelBoardInf(index);

                    indexies = board.GetMovableIndex(topId, index);

                    prepareSceneManager.CellHighLight(indexies);
                    foreach (BoardIndex index in indexies)
                    {
                        Vector2 tmpVal = board.GetBoardPosByIndex(index);
                        Vector3 tmpVal2 = new Vector3(tmpVal.x, tmpVal.y, -6.85f);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (movePermitFlg)
            {
                Vector2 tmpPos = Input.mousePosition;
                Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 a = new Vector3(tmpPos.x, tmpPos.y, pos.z);
                pos = Camera.main.ScreenToWorldPoint(a);
                tmpPos = board.GetCellPosByPos(pos, ref index);
                transform.position = new Vector3(tmpPos.x, tmpPos.y, pos.z);


                indexies = board.GetMovableIndex(topId, index);
                prepareSceneManager.CellHighLight(indexies);

                //持っている間少しでかくする
                transform.localScale = 1.2f * scale;

                //位置が下すぎる場合はホバーする
                if (pos.y < 0.58f)
                {
                    hideFlg = true;
                    transform.position = pos;
                }
                else
                {
                    hideFlg = false;
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (movePermitFlg)
            {
                //離したら大きさを戻す
                transform.localScale = scale;

                //盤面も初期化しておく
                prepareSceneManager.CellHighLight(new List<BoardIndex>());

                //非表示状態のときは削除する
                if (hideFlg)
                {
                    //コマをしまった
                    prepareSceneManager.EndOutATop(topId);

                    Destroy(gameObject);
                }
                //ドロップ位置が5行目ではない場合は削除する
                else if (index.yIndex != 5)
                {
                    //コマをしまった
                    prepareSceneManager.EndOutATop(topId);

                    Destroy(gameObject);
                }
                //ドロップ位置にコマがない場合、盤面情報に登録する
                else if (!board.ChkBoardTop(index))
                {
                    prepareSceneManager.PutATop(topId, gameObject, index);
                }
                //コマがある場合はそのコマを削除し、上書きする
                else
                {
                    int delId = board.GetTopIdByIndex(index);

                    //コマをしまった
                    prepareSceneManager.EndOutATop(delId);

                    GameObject obj = board.DelBoardInf(index);

                    Destroy(obj);
                    prepareSceneManager.PutATop(topId, gameObject, index);
                }

                movePermitFlg =false;
            }
        }
    }

    public void SetTopIndex(BoardIndex index)
    {
        this.index = index;
    }
}
