using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareTopCtrl : MonoBehaviour
{
    private PrepareSceneManager prepareSceneManager;
    private ShogiBoard board;
    private Vector3 pos = new Vector3();
    private Vector2 tmpPos;
    public int xIndex;
    public int yIndex;

    public int topId;
    public bool movePermitFlg;
    private bool hideFlg;
    List<BoardIndex> indexies = new List<BoardIndex>();
    private Vector3 scale;
    GameObject tilePrefs;
    List<GameObject> tiles;

    void Start()
    {
        prepareSceneManager = FindObjectOfType<PrepareSceneManager>();
        prepareSceneManager.RefShogiBoard(ref board);
        hideFlg = true;
        scale = transform.localScale;
        tilePrefs = Resources.Load<GameObject>("GameObject/PrepareScene/CellObject");

        //コマ引き出し不可であれば即削除する
        if (!prepareSceneManager.ChkPullOut(topId))
        {
            Destroy(gameObject);
        }
        else
        {
            //コマを引き出した
            prepareSceneManager.PullOutATop(topId);
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
                //if (hit.collider.gameObject.CompareTag("Top"))
                if (hit.transform.gameObject == this.gameObject)
                {
                    movePermitFlg = true;
                    board.DelBoardInf(xIndex, yIndex);
                    Debug.Log("消える");

                    indexies = board.GetMovableIndex(topId, xIndex, yIndex);

                    prepareSceneManager.CellHighLight(indexies);
                    foreach (BoardIndex index in indexies)
                    {
                        Vector2 tmpVal = board.GetBoardPosByIndex(index.xIndex, index.yIndex);
                        Vector3 tmpVal2 = new Vector3(tmpVal.x, tmpVal.y, -6.85f);
                        Debug.Log(index.xIndex + ", " + index.yIndex);
                    }
                }
            }
        }

        if (Input.GetMouseButton(0))
        {
            if (movePermitFlg)
            {
                tmpPos = Input.mousePosition;
                this.pos = Camera.main.WorldToScreenPoint(transform.position);
                Vector3 a = new Vector3(tmpPos.x, tmpPos.y, pos.z);
                this.pos = Camera.main.ScreenToWorldPoint(a);
                tmpPos = board.GetCellPosByPos(this.pos, ref xIndex, ref yIndex);
                transform.position = new Vector3(tmpPos.x, tmpPos.y, pos.z);


                indexies = board.GetMovableIndex(topId, xIndex, yIndex);
                prepareSceneManager.CellHighLight(indexies);

                //持っている間少しでかくする
                transform.localScale = 1.2f * scale;

                //位置が下すぎる場合は非表示にしておく
                if (this.pos.y < 0.58f)
                {
                    hideFlg = true;
                    transform.position = this.pos;
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
                else if (yIndex != 5)
                {
                    //コマをしまった
                    prepareSceneManager.EndOutATop(topId);

                    Destroy(gameObject);
                }
                //ドロップ位置にコマがない場合、盤面情報に登録する
                else if (!board.ChkBoardTop(xIndex, yIndex))
                {
                    prepareSceneManager.PutATop(topId, gameObject, xIndex, yIndex);
                }
                //コマがある場合はそのコマを削除し、上書きする
                else
                {
                    int delId = board.GetTopIdByIndex(xIndex, yIndex);

                    //コマをしまった
                    prepareSceneManager.EndOutATop(delId);

                    GameObject obj = board.DelBoardInf(xIndex, yIndex);

                    Destroy(obj);
                    prepareSceneManager.PutATop(topId, gameObject, xIndex, yIndex);
                }

                movePermitFlg =false;
            }
        }
    }
}
