using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GachaMachine : MonoBehaviour
{
    private GachaSceneManager gachaSceneManager;
    private Transform GachaBar;
    private float initRotZ;
    private float moveRotZ;
    private Vector3 startPos;
    private Vector3 movePos;
    private bool gachaFlg;


    void Start()
    {
        gachaSceneManager = FindObjectOfType<GachaSceneManager>();
        GachaBar = transform.Find("GachaBar");
        Debug.Log(GachaBar.transform.position);
        initRotZ = GachaBar.transform.eulerAngles.z;
        gachaFlg = false;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            movePos = Input.mousePosition;
            moveRotZ = Mathf.Clamp((startPos.y - movePos.y), initRotZ, 90);
            GachaBar.transform.localRotation = Quaternion.Euler(0, 0, moveRotZ);

            if (moveRotZ >= 90 && !gachaFlg)
            {
                gachaFlg = true;
                gachaSceneManager.DoGacha();
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (gachaFlg)
            {
                gachaFlg = false;
            }

            GachaBar.transform.localRotation = Quaternion.Euler(0, 0, initRotZ);
        }
    }
}
