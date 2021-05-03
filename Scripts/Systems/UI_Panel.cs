using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UI_Panel : MonoBehaviour
{
    PointerEventData pointer;
    GameObject backPanel;
    bool flg = false;

    void Start()
    {
        pointer = new PointerEventData(EventSystem.current);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            List<RaycastResult> results = new List<RaycastResult>();
            // マウスポインタの位置にレイ飛ばし、ヒットしたものを保存
            pointer.position = Input.mousePosition;
            EventSystem.current.RaycastAll(pointer, results);

            flg = true;

            // ヒットしたUIの名前
            foreach (RaycastResult target in results)
            {
                Debug.Log(target.gameObject.name);

                if (target.gameObject.name == gameObject.name)
                {
                    flg = false;
                }

                if (target.gameObject.name == "BackPanel")
                {
                    backPanel = target.gameObject;
                }

            }

            if (flg)
            {
                backPanel.SetActive(false);
                gameObject.SetActive(false);
            }

            return;
        }
    }

    public void PushCancelButton()
    {
        //backPanelを消す
        FindObjectOfType<TitleSceneManager>().SetActiveBackPanel(false);
        //親のオブジェクトを消す
        gameObject.SetActive(false);
    }
}
