using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.SceneManagement;
using NCMB;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    private MasterDataRepository masterDataRepository; /* マスターデータ         */
    private List<TopTable> topTableList;               /* コマ情報テーブルリスト */
    private PlayerInfo playerInfo;                     /* プレイヤー情報         */
    private int BoardSize = 5;                         /* 盤面サイズ             */
    private int[] topAlignInfo;                        /* コマ配置情報           */
    public  bool OnLobbyFlg = false;                   /* ロビーインフラグ       */
    public  int MenuInCnt = 0;                         /* メニューインカウント   */

    private void Start()
    {
        masterDataRepository = Resources.Load<MasterDataRepository>("MasterData/MasterDataRepository");
        masterDataRepository.GetMstDataLoadAll(out topTableList);
        topAlignInfo = new int[GameDef.BOARD_CELLS + 1];
    }

    /// <summary>
    /// ゲームシーンをロードする
    /// </summary>
    /// <param name="sceneName"></param>
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// プレイヤー情報を設定する
    /// </summary>
    /// <param name="id"></param>
    /// <param name="name"></param>
    /// <param name="rate"></param>
    /// <param name="coins"></param>
    /// <param name="items"></param>
    public void SetPlayerInfo(int id, string name, int rate, int coins, PossessingItems items, float alignInfo)
    {
        playerInfo = new PlayerInfo(id, name, rate, coins, items, alignInfo);

        topAlignInfo = playerInfo.GetAlignInfo();
    }

    /// <summary>
    /// プレイヤー情報を参照する
    /// </summary>
    /// <param name="info"></param>
    public void RefPlayerInfo(ref PlayerInfo info)
    {
        info = this.playerInfo;
    }

    /// <summary>
    /// 所持コマ情報を取得する
    /// </summary>
    /// <param name="items"></param>
    public Dictionary<int, int> GetPossessingTops()
    {
        Dictionary<int, int> topDic = new Dictionary<int, int>();
        PossessingItems items = new PossessingItems(0, 0);

        items = playerInfo.GetPossessingItems();
        topDic = items.GetTopDic();

        return topDic;
    }

    /// <summary>
    /// 将棋盤サイズを取得する
    /// </summary>
    public int GetShogiBoard()
    {
        return BoardSize;
    }

    /// <summary>
    /// コマ情報を参照する
    /// </summary>
    /// <param name="topTableList"></param>
    public void RefTopTableList(ref List<TopTable> topTableList)
    {
        topTableList = this.topTableList;
    }

    /// <summary>
    /// コマ配置情報を参照する
    /// </summary>
    /// <param name="info"></param>
    public void RefTopAlignInfo(ref int[] info)
    {
        info = topAlignInfo;
    }

    /// <summary>
    /// コマ配置情報を設定する
    /// </summary>
    public void SetAlignInfo(int[] info)
    {
        bool chgFlg = false;
        float binData = 0;

        chgFlg = playerInfo.SetAlignInfo(info, ref binData);

        ///* データの更新が必要 */
        //if (chgFlg)
        //{
        //    NCMBUser user = new NCMBUser();
        //    user.ObjectId = NCMBUser.CurrentUser.ObjectId;
        //    user.FetchAsync((NCMBException e) => {
        //        if (e != null)
        //        {
        //            Debug.Log("データベース登録失敗");
        //            //エラー処理
        //        }
        //        else
        //        {
        //            //成功時の処理
        //            Debug.Log("ここでコマ配置情報の更新を行う");

        //            user["AlignInfo"] = binData.ToString();
        //            user.SaveAsync((NCMBException e2) => {
        //                if (e2 != null)
        //                {
        //                    Debug.Log("コマ配置情報登録失敗");
        //                }
        //                else
        //                {
        //                    Debug.Log("コマ配置情報登録完了");
        //                }
        //            });

        //        }
        //    });
        //}
    }
}
