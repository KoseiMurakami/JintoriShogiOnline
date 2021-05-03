using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ContentClickListener : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
{
    private GameObject itemPref;
    private GameObject itemObj;
    private Vector3 initPos;
    private Vector3 nowPos;
    List<TopTable> topTableList;


    bool clickFlg;
    public int topId;

    private void Start()
    {
        GameManager.Instance.RefTopTableList(ref topTableList);
        itemPref = Resources.Load<GameObject>("GameObjects/PrepareScene/Tops/Top");
    }

    private void Update()
    {
        if (!clickFlg)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("クリック位置測定開始");
            initPos = Input.mousePosition;
            nowPos = new Vector3(0, 0, 0);
        }

        if (Input.GetMouseButton(0))
        {
            nowPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("クリック位置測定終了");
            clickFlg = false;
        }

        //上方向のマウス移動が一定以上であればコマをインスタンス化する
        if (nowPos.y - initPos.y > 10)
        {
            //押しはじめと移動中の差分を監視する
            itemObj = Instantiate(itemPref);

            //マテリアル設定
            Material[] mats = itemObj.GetComponent<MeshRenderer>().materials;
            mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == topId).AssetName);
            itemObj.GetComponent<MeshRenderer>().materials = mats;

            PrepareTopCtrl prepareTopCtrl = itemObj.GetComponent<PrepareTopCtrl>();
            prepareTopCtrl.topId = topId;
            prepareTopCtrl.movePermitFlg = true;

            //Viewportのスクロールを停止する
            gameObject.transform.parent.parent.parent.GetComponent<ScrollRect>().horizontal = false;

            clickFlg = false;
        }

        //横方向のマウス移動が一定以上であればclickFlgをOFFする
        if (Mathf.Abs(nowPos.x - initPos.x) > 20)
        {
            clickFlg = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("クリック開始");
        clickFlg = true;
        //Viewportのスクロールを開始する
        gameObject.transform.parent.parent.parent.GetComponent<ScrollRect>().horizontal = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("クリックされたよ。");
    }
}