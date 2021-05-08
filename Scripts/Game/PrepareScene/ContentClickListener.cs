using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ContentClickListener : MonoBehaviour, IPointerDownHandler
{
    List<TopTable> topTableList;  /* コマ情報テーブルリスト */
    private GameObject itemPref;  /* コマPrefab             */
    private Vector3 initPos;      /* 初期クリック位置       */
    private Vector3 nowPos;       /* 現在のクリック位置     */
    bool clickFlg;                /* クリックフラグ         */
    public int topId;             /* コマID                 */

    private void Start()
    {
        GameManager.Instance.RefTopTableList(ref topTableList);
        itemPref = Resources.Load<GameObject>("GameObjects/PrepareScene/Tops/Top");
    }

    private void FixedUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            initPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            nowPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            clickFlg = false;
        }
    }

    private void Update()
    {
        if (!clickFlg)
        {
            return;
        }

        /* 上方向のマウス移動が一定以上であればコマを生成する */
        if (nowPos.y - initPos.y > 10)
        {
            clickFlg = false;

            //コマを生成
            GameObject itemObj = Instantiate(itemPref);
            Material[] mats = itemObj.GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == topId).AssetName);
            itemObj.GetComponent<MeshRenderer>().materials = mats;
            PrepareTopCtrl prepareTopCtrl = itemObj.GetComponent<PrepareTopCtrl>();
            prepareTopCtrl.topId = topId;
            prepareTopCtrl.movePermitFlg = true;

            //Viewportのスクロールを停止する
            gameObject.transform.parent.parent.parent.GetComponent<ScrollRect>().horizontal = false;
        }

        /* 横方向のマウス移動が一定以上であればコマを生成できないようにする */
        if (Mathf.Abs(nowPos.x - initPos.x) > 20)
        {
            clickFlg = false;
        }
    }


    /// <summary>
    /// クリック開始コールバック
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        clickFlg = true;
        gameObject.transform.parent.parent.parent.GetComponent<ScrollRect>().horizontal = true;
    }
}