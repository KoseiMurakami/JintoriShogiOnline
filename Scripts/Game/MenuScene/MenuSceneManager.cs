using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using NCMB;

public class MenuSceneManager : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Text nameText = default;
    [SerializeField]
    private Text coinText = default;
    [SerializeField]
    private Text rateText = default;
    [SerializeField]
    private Button BattleButton = default;
    [SerializeField]
    private Button PreparationButton = default;
    [SerializeField]
    private Button GachaButton = default;

    PlayerInfo playerInfo;


    void Start()
    {
        GameManager.Instance.RefPlayerInfo(ref playerInfo);

        nameText.text = playerInfo.playerId.ToString();
        coinText.text = playerInfo.coins.ToString();
        rateText.text = playerInfo.rate.ToString();

        /* PhotonServerSettingに設定した内容を使ってマスターサーバに接続する */
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }

        //Debug.Log(NCMBUser.CurrentUser.ObjectId); //ログインユーザーのオブジェクトIDを取得する
        if (GameManager.Instance.OnLobbyFlg)
        {
            BattleButton.interactable = true;
            PreparationButton.interactable = true;
            GachaButton.interactable = true;
        }
        else
        {
            BattleButton.interactable = false;
            PreparationButton.interactable = false;
            GachaButton.interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PushTitleButton()
    {
        GameManager.Instance.LoadScene("TitleScene");
    }

    /// <summary>
    /// バトルボタンを押したときの処理
    /// </summary>
    public void PushBattleButton()
    {
        /* 既存のランダムな部屋へ入ることを試みる     */
        /* 以降、PhotonCallBacks内で処理を進める      */
        /* ランダムな部屋に入ることができた場合       */
        /* →そのままマッチング完了となり、Battleへ   */
        /* ランダムな部屋に入ることができなかった場合 */
        /* →部屋を作成し、マッチング相手を待つ       */
        JoinRandomRoom();
    }

    /// <summary>
    /// PreraretionButtonを押したときの処理
    /// </summary>
    public void PushPreparationButton()
    {
        GameManager.Instance.LoadScene("PreparationScene");
    }

    /// <summary>
    /// GachaButtonを押したときの処理
    /// </summary>
    public void PushGachaButton()
    {
        GameManager.Instance.LoadScene("GachaScene");
    }

    /// <summary>
    /// ネットワークにニックネームを登録する
    /// </summary>
    /// <param name="nickName"></param>
    private void setMyNickName(string nickName)
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LocalPlayer.NickName = nickName;
        }
    }

    /// <summary>
    /// ロビーに入る
    /// </summary>
    private void joinLobby()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    /// <summary>
    /// 部屋を作成して入室する
    /// </summary>
    public void CreateAndJoinRoom()
    {
        // ルームオプションの基本設定
        RoomOptions roomOptions = new RoomOptions
        {
            // 部屋の最大人数
            MaxPlayers = (byte)2,

            // 公開
            IsVisible = true,

            // 入室可
            IsOpen = true
        };

        //// ルームオプションにカスタムプロパティを設定
        //ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable
        //{
        //    { "Stage", stageName },
        //    { "Difficulty", stageDifficulty }
        //};
        //roomOptions.CustomRoomProperties = customRoomProperties;

        //// ロビーに公開するカスタムプロパティを指定
        //roomOptions.CustomRoomPropertiesForLobby = new string[] { "Stage", "Difficulty" };

        // 部屋を作成して入室する
        if (PhotonNetwork.InLobby)
        {
            string roomName = PhotonNetwork.LocalPlayer.NickName;
            PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        }
    }

    /// <summary>
    /// ランダムな部屋に入る
    /// </summary>
    public void JoinRandomRoom()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinRandomRoom();
        }
    }

    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    /*★                         Pun Callback List                          ★*/
    /*★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★★*/
    /* Photonに接続したとき */
    public override void OnConnected()
    {
        Debug.Log("ネットワークに接続しました。");
        setMyNickName(playerInfo.playerName);
        Debug.Log("ようこそ、" + PhotonNetwork.LocalPlayer.NickName + "さん。");
    }

    /* Photonから切断されたとき */
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("ネットワークから切断されました。");
    }

    /* マスターサーバへ接続したとき */
    public override void OnConnectedToMaster()
    {
        Debug.Log("マスターサーバに接続しました。");

        //ロビーに入る
        joinLobby();
    }

    /* ロビーに入ったとき */
    public override void OnJoinedLobby()
    {
        Debug.Log("ロビーに入りました。");

        //ロビーインしたことを通知する
        GameManager.Instance.OnLobbyFlg = true;

        //各ボタンをアクティブにする
        BattleButton.interactable = true;
        PreparationButton.interactable = true;
        GachaButton.interactable = true;
    }

    /* ロビーから出たとき */
    public override void OnLeftLobby()
    {
        Debug.Log("ロビーから出ました。");
    }

    /* 部屋を作成したとき */
    public override void OnCreatedRoom()
    {
        Debug.Log("部屋を作成しました。");

        GameManager.Instance.LoadScene("BattleScene");
    }

    /* 部屋の作成に失敗したとき */
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("部屋の作成に失敗しました。");
    }

    /* 部屋に入室したとき */
    public override void OnJoinedRoom()
    {
        //遷移前のシーンでネットワークオブジェクトを生成しないようにする
        PhotonNetwork.IsMessageQueueRunning = false;
        //ルームに移動
        //GameManager.Instance.LoadGameScene("LobbyScene");

        Debug.Log("部屋に入室しました。");

        GameManager.Instance.LoadScene("BattleScene");
    }

    /* 特定の部屋への入室に失敗したとき */
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("部屋の入室に失敗しました。");
    }

    /* ランダムな部屋の入室に失敗したとき */
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("ランダムな部屋の入室に失敗しました。");

        /* ないのであれば作成して入室する */
        RoomOptions roomOptions = new RoomOptions
        {
            // 部屋の最大人数
            MaxPlayers = (byte)2,

            // 公開
            IsVisible = true,

            // 入室可
            IsOpen = true
        };
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    /* 部屋から退室したとき */
    public override void OnLeftRoom()
    {
        Debug.Log("部屋から退室しました。");
    }

    /* 他のプレイヤーが入室したとき */
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("他のプレイヤーが入室してきました。");
    }

    /* 他のプレイヤーが退室したとき */
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("他のプレイヤーが退室しました。");
    }

    /* マスタークライアントが変わったとき */
    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("マスタークライアントが変更されました。");
    }

    /* ロビーに更新があったとき */
    public override void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
    {
        Debug.Log("ロビーが更新されました。");
    }

    /* ルームリストに更新があったとき */
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        Debug.Log("ルームリストに更新がありました。");

        //roomInfos = roomList;
    }

    /* ルームプロパティが更新されたとき */
    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("ルームプロパティが更新されたとき");
    }

    /* プレイヤープロパティが更新されたとき */
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        Debug.Log("プレイヤープロパティが更新されました。");
    }

    /* フレンドリストに更新があったとき */
    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log("フレンドリストに更新がありました。");
    }

    /* 地域リストを受け取ったとき */
    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("地域リストを取得しました。");
    }

    /* WebRpcのレスポンスがあったとき */
    //public override void OnWebRpcResponse(OperationResponse response)
    //{
    //    Debug.Log("WebRcpの応答を検出しました。");
    //}

    /* カスタム認証のレスポンスがあったとき */
    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log("カスタム認証の応答を検出しました。");
    }

    /* カスタム認証に失敗したとき */
    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("カスタム認証に失敗しました。");
    }
}
