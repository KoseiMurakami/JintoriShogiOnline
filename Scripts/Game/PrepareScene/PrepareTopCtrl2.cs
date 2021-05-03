using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrepareTopCtrl2 : MonoBehaviour
{
    private PrepareSceneManager prepareSceneManager;
    private ShogiBoard board;
    private Vector3 pos = new Vector3();
    private Vector2 tmpPos;
    private int xIndex;
    private int yIndex;

    public int topId;
    public bool movePermitFlg;
    private bool hideFlg;

    void Start()
    {
        prepareSceneManager = FindObjectOfType<PrepareSceneManager>();
        prepareSceneManager.RefShogiBoard(ref board);
        movePermitFlg = false;
        hideFlg = true;

        //自身のオブジェクトから盤面インデックスを把握しておく
        tmpPos = gameObject.transform.position;
        board.GetIndexByPos(tmpPos, ref xIndex, ref yIndex);
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
                //位置が下すぎる場合は非表示にしておく
                if (this.pos.y < 0.58f)
                {
                    hideFlg = true;
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
                //非表示状態のときは削除する
                if (hideFlg)
                {
                    Debug.Log("消えました");
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
                    GameObject obj = board.DelBoardInf(xIndex, yIndex);
                    Destroy(obj);
                    prepareSceneManager.PutATop(topId, gameObject, xIndex, yIndex);
                }

                movePermitFlg = false;
            }
        }
    }
}
