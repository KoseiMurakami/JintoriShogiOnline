using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GachaSceneManager : MonoBehaviour
{
    [SerializeField]
    private GameObject topSpawner = default;

    [SerializeField]
    private Text coinText = default;

    [SerializeField]
    private Text messageText = default;

    PlayerInfo info;
    GameObject topPrefab;
    List<TopTable> topTableList;

    void Start()
    {
        GameManager.Instance.RefPlayerInfo(ref info);
        topPrefab = Resources.Load<GameObject>("GameObjects/GachaScene/Top/Top");
        GameManager.Instance.RefTopTableList(ref topTableList);

        coinText.text = info.coins.ToString();
    }

    void Update()
    {
    }

    /// <summary>
    /// ガチャを回す
    /// </summary>
    public void DoGacha()
    {
        /* コインチェック */
        if (info.coins < 100)
        {
            messageText.text = "ガチャを回すことができません";
            return;
        }

        Debug.Log("ガチャ抽選開始");
        /* コインを失う */
        info.coins -= 100;
        coinText.text = info.coins.ToString();

        /* ランダムに1つのコマを手に入れる */
        int topId = Random.Range(1,10);
        info.AddTop(topId);
        messageText.text = topTableList.Find(table => table.Id == topId).Name + "をゲットしました！";

        /* コマを排出する */
        GameObject top = Instantiate(topPrefab, topSpawner.transform.position, Quaternion.identity);

        //マテリアル設定
        Material[] mats = top.GetComponent<MeshRenderer>().materials;
        mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == topId).AssetName);
        top.GetComponent<MeshRenderer>().materials = mats;

        /* ユーザー情報の更新を行う */
        GameManager.Instance.SetPossessingItems();
    }

    /// <summary>
    /// メニューボタンを押したときの処理
    /// </summary>
    public void PushMenuButton()
    {
        GameManager.Instance.LoadScene("MenuScene");
    }
}
