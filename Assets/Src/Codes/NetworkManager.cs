using IronSourceJSON;
using Newtonsoft.Json;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;

    public InputField ipInputField;
    public InputField portInputField;
    public InputField deviceIdInputField;

    //UI
    public GameObject uiNotice;
    public GameObject GameStartUI;

    private TcpClient tcpClient;
    private NetworkStream stream;
    
    WaitForSecondsRealtime wait;

    private byte[] receiveBuffer = new byte[4096];
    private List<byte> incompleteData = new List<byte>();

    void Awake() {        
        instance = this;
        wait = new WaitForSecondsRealtime(5);
    }

    public void OnStartButtonClicked() {
        string ip = ipInputField.text;
        string port = portInputField.text;

        if (IsValidPort(port)) {
            int portNumber = int.Parse(port);

            if (deviceIdInputField.text != "") {
                GameManager.instance.deviceId = deviceIdInputField.text;
            } else {
                if (GameManager.instance.deviceId == "") {
                    GameManager.instance.deviceId = GenerateUniqueID();
                }
            }
  
            if (ConnectToServer(ip, portNumber)) {
                StartGame();
            } else {
                AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
                StartCoroutine(NoticeRoutine(1));
            }
            
        } else {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
            StartCoroutine(NoticeRoutine(0));
        }
    }

    bool IsValidIP(string ip)
    {
        // 간단한 IP 유효성 검사
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    bool IsValidPort(string port)
    {
        // 간단한 포트 유효성 검사 (0 - 65535)
        if (int.TryParse(port, out int portNumber))
        {
            return portNumber > 0 && portNumber <= 65535;
        }
        return false;
    }

     bool ConnectToServer(string ip, int port) {
        try {
            tcpClient = new TcpClient(ip, port);
            stream = tcpClient.GetStream();
            Debug.Log($"Connected to {ip}:{port}");

            return true;
        } catch (SocketException e) {
            Debug.LogError($"SocketException: {e}");
            return false;
        }
    }

    string GenerateUniqueID() {
        return System.Guid.NewGuid().ToString();
    }

    void StartGame()
    {
        // 게임 시작 코드 작성
        Debug.Log("Game Started");
        StartReceiving(); // Start receiving data
        SendInitialPacket();
    }

    IEnumerator NoticeRoutine(int index) {
        
        uiNotice.SetActive(true);
        uiNotice.transform.GetChild(index).gameObject.SetActive(true);

        yield return wait;

        uiNotice.SetActive(false);
        uiNotice.transform.GetChild(index).gameObject.SetActive(false);
    }

    public static byte[] ToBigEndian(byte[] bytes) {
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        return bytes;
    }

    byte[] CreatePacketHeader(int dataLength, Packets.PacketType packetType) {
        int packetLength = 4 + 1 + dataLength; // 전체 패킷 길이 (헤더 포함)
        byte[] header = new byte[5]; // 4바이트 길이 + 1바이트 타입

        // 첫 4바이트: 패킷 전체 길이
        byte[] lengthBytes = BitConverter.GetBytes(packetLength);
        lengthBytes = ToBigEndian(lengthBytes);
        Array.Copy(lengthBytes, 0, header, 0, 4);

        // 다음 1바이트: 패킷 타입
        header[4] = (byte)packetType;

        return header;
    }

    // 공통 패킷 생성 함수
    public void SendPacket<T>(T payload, uint handlerId , Packets.PacketType type = Packets.PacketType.Normal)
    {
        // ArrayBufferWriter<byte>를 사용하여 직렬화
        var payloadWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(payloadWriter, payload);
        byte[] payloadData = payloadWriter.WrittenSpan.ToArray();

        CommonPacket commonPacket = new CommonPacket{};

        commonPacket.handlerId = handlerId;
        commonPacket.userId = GameManager.instance.deviceId;
        commonPacket.clientVersion = GameManager.instance.version;
        commonPacket.sequence = GameManager.instance.sequence;
        commonPacket.payload = payloadData;


        // ArrayBufferWriter<byte>를 사용하여 직렬화
        var commonPacketWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(commonPacketWriter, commonPacket);
        byte[] data = commonPacketWriter.WrittenSpan.ToArray();

        // 헤더 생성
        byte[] header = CreatePacketHeader(data.Length, type);

        // 패킷 생성
        byte[] packet = new byte[header.Length + data.Length];
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(data, 0, packet, header.Length, data.Length);

        // 패킷 전송
        stream.Write(packet, 0, packet.Length);
    }
    

    void SendInitialPacket() {

        InitialPayload initialPayload = new InitialPayload
        {
            deviceId = GameManager.instance.deviceId,
        };


        SendPacket(initialPayload, (uint)Packets.HandlerIds.Init);
    }
    public void SendEmoticonUpdatePacket(int emoticonId)
    {
        EmoticonPayload emoticonUpdatePayload = new()
        {
            emoticonId = emoticonId,
            gameId = roomManager.instance.nowSessionData.gameId,
        };
        SendPacket(emoticonUpdatePayload, (uint)Packets.HandlerIds.EMOTICON_UPDATE);
    }

    public void SendLocationUpdatePacket(float x, float y) {
        DateTime now = DateTime.UtcNow;
        LocationUpdatePayload locationUpdatePayload = new LocationUpdatePayload
        {
            gameId = roomManager.instance.nowSessionData.gameId,
            x = x,
            y = y,
            timestamp = ((DateTimeOffset)now).ToUnixTimeSeconds(),
        };

        SendPacket(locationUpdatePayload, (uint)Packets.HandlerIds.LOCATION_UPDATE);
    }

    public void SendJoinGamePacket(string id)
    {
        JoinGamePayload joinGamePayload = new JoinGamePayload();
        joinGamePayload.gameId = id;

        SendPacket(joinGamePayload,(uint)Packets.HandlerIds.JOIN_GAME);
    }

    public void SendShotUpdatePacket(float z)
    {
        DateTime now = DateTime.UtcNow;
        RotationUpdatePayload locationUpdatePayload = new RotationUpdatePayload
        {
            gameId = roomManager.instance.nowSessionData.gameId,
            z = z,
            timestamp = ((DateTimeOffset)now).ToUnixTimeSeconds(),
        };

        SendPacket(locationUpdatePayload, (uint)Packets.HandlerIds.SHOT_UPDATE);
    }
    void StartReceiving() {
        _ = ReceivePacketsAsync();
    }

    async System.Threading.Tasks.Task ReceivePacketsAsync() {
        while (tcpClient.Connected) {
            try {
                int bytesRead = await stream.ReadAsync(receiveBuffer, 0, receiveBuffer.Length);
                if (bytesRead > 0) {
                    ProcessReceivedData(receiveBuffer, bytesRead);
                }
            } catch (Exception e) {
                Debug.LogError($"Receive error: {e.Message}");
                break;
            }
        }
    }

    void ProcessReceivedData(byte[] data, int length) {
         incompleteData.AddRange(data.AsSpan(0, length).ToArray());

        while (incompleteData.Count >= 5)
        {
            // 패킷 길이와 타입 읽기
            byte[] lengthBytes = incompleteData.GetRange(0, 4).ToArray();
            int packetLength = BitConverter.ToInt32(ToBigEndian(lengthBytes), 0);
            Packets.PacketType packetType = (Packets.PacketType)incompleteData[4];

            if (incompleteData.Count < packetLength)
            {
                // 데이터가 충분하지 않으면 반환
                return;
            }

            // 패킷 데이터 추출
            byte[] packetData = incompleteData.GetRange(5, packetLength - 5).ToArray();
            incompleteData.RemoveRange(0, packetLength);

            switch (packetType)
            {
                case Packets.PacketType.Normal:
                    HandleNormalPacket(packetData);
                    break;
                case Packets.PacketType.Location:
                    HandleLocationPacket(packetData);
                    break;
                case Packets.PacketType.Ping:
                    HandlepingPacket(packetData);
                    break;
                case Packets.PacketType.Emoticon:
                    HandleEmoticonPacket(packetData);
                    break;
                case Packets.PacketType.BulletMove:
                    HandleShotPacket(packetData);
                    break;
            }
        }
    }
    void HandlepingPacket(byte[] packetData)
    {
        var response = Packets.Deserialize<PingPayload>(packetData);

        DateTime now = DateTime.UtcNow;
        DateTime dateTime = DateTimeOffset.FromUnixTimeSeconds(response.timestamp/1000).DateTime;
        float seconds = (float)(now - dateTime).TotalSeconds;
        GameManager.instance.latency = seconds;

        PingPayload pingPayload = new PingPayload
        {
            timestamp = ((DateTimeOffset)now).ToUnixTimeSeconds(),
        };

        var commonPacketWriter = new ArrayBufferWriter<byte>();
        Packets.Serialize(commonPacketWriter, pingPayload);

        byte[] data = commonPacketWriter.WrittenSpan.ToArray();

        // 헤더 생성
        byte[] header = CreatePacketHeader(data.Length, Packets.PacketType.Ping);

        // 패킷 생성
        byte[] packet = new byte[header.Length + data.Length];
        Array.Copy(header, 0, packet, 0, header.Length);
        Array.Copy(data, 0, packet, header.Length, data.Length);

        stream.Write(packet, 0, packet.Length);
    }

    void HandleNormalPacket(byte[] packetData) {
        // 패킷 데이터 처리
        var response = Packets.Deserialize<Response>(packetData);
        Debug.Log($"HandlerId: {response.handlerId}, Response: {response}");

        GameManager.instance.sequence = response.sequence;

        if (response.responseCode != 0 && !uiNotice.activeSelf) {
            AudioManager.instance.PlaySfx(AudioManager.Sfx.LevelUp);
            StartCoroutine(NoticeRoutine(2));
            return;
        }

        if (response.data != null && response.data.Length > 0) {
            Debug.Log(response.handlerId);
            // 여기서 핸들링 해주기 
            if (response.handlerId == 0) {
                roomManager.instance.startRoom(intalResponseData(response.data), GameStartUI);
            }
            
            if (response.handlerId == 2)
            {
                roomManager.instance.creatOK(inSessionResponseData(response.data));
            }
            if (response.handlerId == 3)
            {
                GameManager.instance.playerSpawn(inSessionResponseData(response.data));
            }
            
        }
    }

    GameSessions intalResponseData(byte[] data) {
        try {
            string jsonString = Encoding.UTF8.GetString(data);
            Debug.Log($"Processed SpecificDataType: {jsonString}");
            GameSessions session = JsonConvert.DeserializeObject<GameSessions>(jsonString);
            Debug.Log($"Processed SpecificDataType: {session.gameSessions.Count}");
            return session;
        } catch (Exception e) {
            Debug.LogError($"Error processing response data: {e.Message}");
            return null;
        }
    }

    inSession inSessionResponseData(byte[] data)
    {
        try
        {
            string jsonString = Encoding.UTF8.GetString(data);
            Debug.Log($"Processed SpecificDataType: {jsonString}");
            inSession session = JsonUtility.FromJson<inSession>(jsonString);
            return session;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing response data: {e.Message}");
            return null;
        }
    }

    void HandleLocationPacket(byte[] data) {
        try {
            LocationUpdate response;

            if (data.Length > 0) {
                Debug.Log("data");
                // 패킷 데이터 처리
                response = Packets.Deserialize<LocationUpdate>(data);
            } else {
                Debug.Log("빈값");
                // data가 비어있을 경우 빈 배열을 전달
                response = new LocationUpdate ();
            }
            GameManager.instance.playerMove(response);
        } catch (Exception e) {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }
    void HandleShotPacket(byte[] data)
    {
        try
        {
            bulletsData response;
            if (data.Length > 0)
            {
                ShotUpdate buffer = Packets.Deserialize<ShotUpdate>(data);
                string jsonString = Encoding.UTF8.GetString(buffer.data);
                response = JsonUtility.FromJson<bulletsData>(jsonString);
            }
            else
            {
                Debug.Log("빈값");
                // data가 비어있을 경우 빈 배열을 전달
                response = new bulletsData();
            }
            GameManager.instance.bulletMove(response);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }

    void HandleEmoticonPacket(byte[] data)
    {
        try
        {
            EmoticonUpdate response;

            if (data.Length > 0)
            {
                Debug.Log("data");
                // 패킷 데이터 처리
                response = Packets.Deserialize<EmoticonUpdate>(data);
            }
            else
            {
                Debug.Log("빈값");
                // data가 비어있을 경우 빈 배열을 전달
                response = new EmoticonUpdate();
            }

            GameManager.instance.playerEmoticon(response);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error HandleLocationPacket: {e.Message}");
        }
    }
}
