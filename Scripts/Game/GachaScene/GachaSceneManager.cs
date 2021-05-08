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

    PlayerInfo playerInfo;                    /* プレイヤー情報   */
    GameObject topPrefab;                     /* コマPrefab       */
    List<TopTable> topTableList;              /* コマ情報テーブル */

    void Start()
    {
        GameManager.Instance.RefPlayerInfo(ref playerInfo);
        topPrefab = Resources.Load<GameObject>("GameObjects/GachaScene/Top/Top");
        GameManager.Instance.RefTopTableList(ref topTableList);

        coinText.text = playerInfo.coins.ToString();
        messageText.text = "レバーを倒してコマをゲットしてね。";
    }

    /// <summary>
    /// ガチャを回す
    /// </summary>
    public void DoGacha()
    {
        /* コインチェック */
        if (!coinCheck())
        {
            return;
        }

        /* コインを失う */
        lostCoin(GameDef.GACHA_VALUE);

        /* ランダムに1つのコマを手に入れる */
        int topId = GetRandomTop();

        /* コマを排出する */
        if (topId != 0)
        {
            emissionTop(topId);
        }
    }

    /// <summary>
    /// ガチャできるだけのコインがあるかチェックする
    /// </summary>
    /// <returns></returns>
    private bool coinCheck()
    {
        bool retVal = true;

        if (playerInfo.coins < GameDef.GACHA_VALUE)
        {
            messageText.text = "ガチャを回すことができません";
            retVal = false;
        }

        return retVal;
    }

    /// <summary>
    /// コインを支払う
    /// </summary>
    /// <param name="price"></param>
    private void lostCoin(int price)
    {
        playerInfo.coins -= price;
        coinText.text = playerInfo.coins.ToString();
    }

    /// <summary>
    /// ランダムなコマを1つ獲得する
    /// </summary>
    private int GetRandomTop()
    {
        int topId = Random.Range(1, 10);
        if (playerInfo.AddTop(topId))
        {
            messageText.text = topTableList.Find(table => table.Id == topId).Name + "をゲットしました！";
        }
        else
        {
            topId = 0;
            messageText.text = "同じコマを5つ以上持つことはできません。\r\n返金します。";
            lostCoin(-GameDef.GACHA_VALUE);
        }

        return topId;
    }

    /// <summary>
    /// コマを排出する
    /// </summary>
    private void emissionTop(int topId)
    {
        /* コマを排出する */
        GameObject top = Instantiate(topPrefab, topSpawner.transform.position, Quaternion.identity);

        //マテリアル設定
        Material[] mats = top.GetComponent<MeshRenderer>().materials;
        mats[0] = Resources.Load<Material>("Materials/" + topTableList.Find(topTable => topTable.Id == topId).AssetName);
        top.GetComponent<MeshRenderer>().materials = mats;
    }

    /// <summary>
    /// メニューボタンを押したときの処理
    /// </summary>
    public void PushMenuButton()
    {
        GameManager.Instance.LoadScene("MenuScene");
    }
}
