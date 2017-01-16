using System;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

private static readonly string[] _validCommands = new string[] { "1", "2", "3", "4", "5" };
private MqttClient _client = new MqttClient(ConfigurationManager.AppSettings["mqtt_host"], 16742, false, null, null, MqttSslProtocols.None);

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    var data = await req.Content.ReadAsFormDataAsync();
    log.Info($"Triggered WebHook with text data '{data["text"]}' by '{data["user_name"]}'");

    if (!_validCommands.Contains(data["text"]))
        return req.CreateResponse(HttpStatusCode.OK, $"{data["user_name"]}, the only valid commands are '{String.Join(", ", _validCommands)}'.");
    
    if(!_client.IsConnected)
    {
        var clientId = Guid.NewGuid().ToString();
        log.Info($"Client {clientId} connecting");
        var connectResponse = _client.Connect(
            Guid.NewGuid().ToString(), 
            ConfigurationManager.AppSettings["mqtt_user"], 
            ConfigurationManager.AppSettings["mqtt_password"]);
        log.Info($"Client {clientId} connection result {connectResponse}");
    }

    var messageId = _client.Publish(
        ConfigurationManager.AppSettings["mqqt_topic"],
        Encoding.ASCII.GetBytes(data["text"]),
        MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
        false);

    log.Info($"Published message {messageId}");

    return req.CreateResponse(HttpStatusCode.OK, $"I'm opening capsule '{data["text"]}'");
}