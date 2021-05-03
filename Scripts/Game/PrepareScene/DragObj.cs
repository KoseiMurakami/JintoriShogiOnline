using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DragObj : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    private PrepareSceneManager prepareSceneManager;

    /*D&D範囲*/
    private Vector2 rangeLeftUp;
    private Vector2 rangeRightDown;

    private Vector3 position;
    private GameObject itemPref;
    private GameObject itemObj;

    public int ItemId { set; get; }

    private void Start()
    {
        prepareSceneManager = FindObjectOfType<PrepareSceneManager>();

        rangeLeftUp = new Vector2(-6.5f, 2.5f);
        rangeRightDown = new Vector2(-1.7f, -5.0f);

        itemPref = Resources.Load<GameObject>("GameObjects/PrepareScene/Tops/Top");
    }

    public void OnBeginDrag(PointerEventData data)
    {
        return;
        //GetComponent<CanvasGroup>().blocksRaycasts = false;
        itemObj = Instantiate(itemPref);
        itemObj.AddComponent<PrepareTopCtrl>();
        Debug.Log("OnBeginDrag");
    }

    public void OnDrag(PointerEventData data)
    {
        //position = data.position;
        //position = Input.mousePosition;
        //position.z = -6.85f;
        //itemObj.transform.position = Camera.main.ScreenToWorldPoint(position);
        //itemObj.transform.position = position;
        Debug.Log("OnDrag");
    }

    public void OnEndDrag(PointerEventData data)
    {
        //GetComponent<CanvasGroup>().blocksRaycasts = true;

        position = data.position;
        position.z = 10f;

        //mousePositionが対象の範囲内ならじわじわと消す
        if (InRange(Camera.main.ScreenToWorldPoint(position)))
        {
            //Destroy(itemObj);
        }
        //範囲外ならすぐにデストロイ
        else
        {
            //Destroy(itemObj);
        }
        Debug.Log("OnEndDrag");
    }

    private bool InRange(Vector3 position)
    {
        if (rangeLeftUp.x < position.x &&
            rangeLeftUp.y > position.y &&
            rangeRightDown.x > position.x &&
            rangeRightDown.y < position.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // オブジェクトの範囲内にマウスポインタが入った際に呼び出されます。
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("OnPointerEnter");
    }

    // オブジェクトの範囲内からマウスポインタが出た際に呼び出されます。
    // 
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit");
    }
}
