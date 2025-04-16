using Dummiesman;
using M2MqttUnity;
using Microsoft.MixedReality.OpenXR;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;
using uPLibrary.Networking.M2Mqtt.Messages;

[System.Serializable]
public struct Anatomy
{
    public string UrlName;
    public Material material;
}
public class M2MClient : M2MqttUnityClient
{
    [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
    public bool autoTest = false;
    [SerializeField]
    private bool updatePath;
    private List<string> eventMessages = new List<string>();
    private Vector3 bronchoscopePosition = Vector3.zero;
    public Vector3 BronchoscopePosition => bronchoscopePosition;
    [SerializeField]
    private CutoutPath cutoutPath;
    [SerializeField]
    private PathToTumorVisualizer pathVisualizer;
    [SerializeField]
    private Transform anatomyHolder;
    [SerializeField]
    private ARMarkerManager markerManager;
    // TODO: Recieve and handle bronchoscope position
    // TODO: Recieve each piece of selected anatomy
    [SerializeField]
    private GameObject downloadButton;
    [SerializeField]
    private Anatomy[] anatomy;
    protected override void Start()
    {
        markerManager.markersChanged += OnQRCodesChanged;
        base.Start();
    }

    void OnQRCodesChanged(ARMarkersChangedEventArgs args)
    {
        foreach (ARMarker qrCode in args.added)
        {
            var text = qrCode.GetDecodedString();
            // Expected formatting IP192.168.x.x
            if (text.Contains("IP"))
            {
                brokerAddress = text[2..];
                Connect();
            }
            Debug.Log($"QR code text: {text}");
        }
    }

    protected override void DecodeMessage(string topic, byte[] message)
    {
        string msg = System.Text.Encoding.UTF8.GetString(message);
        Debug.Log("Received: " + msg);
        switch (topic)
        {
            case "M2MQTT_Unity/bronchoscope":
                try
                {
                    // TODO: This format instead
                    /*
                    string[] numbers = msg.Split(",");
                    float x = float.Parse(numbers[0]);
                    float y = float.Parse(numbers[1]);
                    float z = float.Parse(numbers[2]);
                    bronchoscopePosition = new Vector3(x, y, z);
                    */
                    cutoutPath.NormalizedPathPosition = float.Parse(msg, CultureInfo.InvariantCulture);
                }
                catch 
                {
                    Debug.Log("Bronchoscope position was not formated correctly, expected float");
                }
                break;
            case "M2MQTT_Unity/anatomy":
                // TODO: Split into seperate pieces of anatomy and attach correct material to each
                try
                {
                    var textStream = new MemoryStream(Encoding.UTF8.GetBytes(msg));
                    var loadedObj = new OBJLoader().Load(textStream);
                    loadedObj.transform.SetParent(anatomyHolder, false);
                    loadedObj.transform.localScale *= 0.0025f;
                    loadedObj.transform.localRotation = Quaternion.FromToRotation(Vector3.up, Vector3.back);
                }
                catch
                {
                    Debug.Log("Failed reading anatomy values");
                }
                break;
            case "M2MQTT_Unity/path":
                try
                {
                    pathVisualizer.ParseProcessedData(msg);
                }
                catch
                {
                    Debug.Log("Failed reading path value");
                }
                break;
            default:
                break;
        }
    }
    protected override void OnConnecting()
    {
        base.OnConnecting();
        Debug.Log("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Connected to broker on " + brokerAddress + "\n");
        downloadButton.SetActive(true);
    }
    protected override void OnConnectionFailed(string errorMessage)
    {
        Debug.Log("CONNECTION FAILED! " + errorMessage);
    }

    protected override void Update()
    {
        base.Update(); // call ProcessMqttEvents()

        if (eventMessages.Count > 0)
        {
            foreach (string msg in eventMessages)
            {
                ProcessMessage(msg);
            }
            eventMessages.Clear();
        }
        if (updatePath)
        {
            //UpdatePath();
        }
    }

    private void ProcessMessage(string msg)
    {
        //AddUiMessage("Received: " + msg);
    }

    private void OnDestroy()
    {
        Disconnect();
    }

    protected override void SubscribeTopics()
    {
        client.Subscribe(new string[] { "M2MQTT_Unity/bronchoscope" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { "M2MQTT_Unity/anatomy" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        client.Subscribe(new string[] { "M2MQTT_Unity/path" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
    }

    protected override void UnsubscribeTopics()
    {
        client.Unsubscribe(new string[] { "M2MQTT_Unity/bronchoscope" });
        client.Unsubscribe(new string[] { "M2MQTT_Unity/anatomy" });
        client.Unsubscribe(new string[] { "M2MQTT_Unity/path" });
    }

    public void DownloadAllMesh()
    {
        foreach (var item in anatomy)
        {
            //brokerAddress + "/" + item.UrlName;
        }
    }
}
