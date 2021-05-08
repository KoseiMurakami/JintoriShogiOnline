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
    private Button BattleButton = default;
    [SerializeField]
    private Button PreparationButton = default;
    [SerializeField]
    private Button GachaButton = default;
    [SerializeField]
    private Text evtText = default;
    [SerializeField]
    private Text regionText = default;
    [SerializeField]
    private Text onlineCntText = default;

    private PlayerInfo playerInfo;        /* プレイヤー情報   */
    private List<string> textBuff;        /* テキストバッファ */

    void Start()
    {
        /* PhotonServerSettingに設定した内容を使ってマスターサーバに接続する */
        ConnectMasterServer();
        GameManager.Instance.RefPlayerInfo(ref playerInfo);
        textBuff = GetTextBuff();

        //プレイヤー情報をテキストに書き込む
        nameText.text = playerInfo.playerName;
        coinText.text = playerInfo.coins.ToString();

        //すでにロビーインしているならボタンをアクティブにする
        if (GameManager.Instance.OnLobbyFlg)
        {
            SetButtonActive(true);
        }
        //ロビーインしていない状態であればロビーインするまで待つ
        else
        {
            SetButtonActive(false);
        }

        //テキストを設定する
        SetText();

        //メニューインカウントを1つ増やす
        GameManager.Instance.MenuInCnt++;
    }

    private void Update()
    {
        //リージョン
        regionText.text = "サーバー：" + PhotonNetwork.CloudRegion;
        //オンライン人数を取得する
        onlineCntText.text = "オンライン人数：" + (PhotonNetwork.CountOfPlayers) + "人";

        if (PhotonNetwork.CountOfPlayersInRooms % 2 == 1)
        {
            evtText.text = "他の誰かがマッチング相手を探しています！\r\n対局開始しましょう！";
        }
        else
        {
            if (evtText.text == "他の誰かがマッチング相手を探しています！\r\n対局開始しましょう！")
            {
                SetText();
            }
        }
    }

    /// <summary>
    /// マスターサーバに接続する
    /// </summary>
    public void ConnectMasterServer()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
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

    /// <summary>
    /// ボタンのアクティブ設定を一括で行う
    /// </summary>
    /// <param name="activeFlg"></param>
    public void SetButtonActive(bool activeFlg)
    {
        BattleButton.interactable = activeFlg;
        PreparationButton.interactable = activeFlg;
        GachaButton.interactable = activeFlg;
    }

    /// <summary>
    /// テキストバッファ情報を取得する
    /// </summary>
    /// <returns></returns>
    public List<string> GetTextBuff()
    {
        List<string> textBuff = new List<string>();

        string[] strings = new string[]
        {
            "まずはガチャでコマを集めよう。\r\n1回100コインでガチャを回すことができるよ。",
            "次に対局準備をしよう。\r\nここで初期配置を変更することができるよ。",
            "準備ができたら対局だ。\r\n対局開始ボタンから相手を探そう。",
            "「狙撃」は4マス前にジャンプできるぞ。\r\n初手で動いて先制パンチだ。",
            "「飛車」は2マス前後に移動できるぞ。\r\n機動力を生かして有利に戦おう。",
            "「角行」は2マス斜めに移動できるぞ。\r\n相手の意表をついて戦おう。",
            "「香車」は2マス前に移動できるぞ。\r\n対戦序盤に低リスクで自分のマスを稼ごう。",
            "「歩」は1マス前に移動できるぞ。\r\n正直、あえて使う必要はないぞ。",
            "「桂馬」は2マス前、1マス左右にジャンプできるぞ。\r\n変則的な動きで相手を翻弄しよう。",
            "「金将」は1マス後、1マス左右以外に移動できるぞ。\r\n守備を固めたいなら金将を使え。",
            "「銀将」は左右、後以外に移動できるぞ。\r\n攻撃に特化したいなら銀将を使え。",
            "「王将」は周り1マスに移動できるぞ。\r\nこのゲームでは王をとられてもゲームは続く。気にせず攻めろ。",
        };

        for (int i = 0; i < strings.Length; i++)
        {
            textBuff.Add(strings[i]);
        }

        return textBuff;
    }

    /// <summary>
    /// テキストバッファからランダムでテキストを表示する
    /// </summary>
    private void SetText()
    {
        int menuInCnt = GameManager.Instance.MenuInCnt;

        //最初の3回は決まったテキストを出す
        if (menuInCnt == 0 || menuInCnt == 1 || menuInCnt == 2)
        {
            evtText.text = textBuff[menuInCnt];
        }
        else
        {
            int buffIndex = Random.Range(3, textBuff.Count);
            evtText.text = textBuff[buffIndex];
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
        SetButtonActive(true);
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
