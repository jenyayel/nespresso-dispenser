using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using System.Threading;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using CM = System.Configuration.ConfigurationManager;

private static readonly HashSet<string> _validCommands = new HashSet<string>(CM.AppSettings["valid_commands"].Split(',').Select(s => s.Trim().ToLower()));
private static readonly HashSet<string> _validUsers = new HashSet<string>(CM.AppSettings["valid_users"].Split(',').Select(s => s.Trim().ToLower()));

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    if (req.Method == HttpMethod.Get)
        return req.CreateResponse(HttpStatusCode.OK); // test/ping  requests

    var data = await req.Content.ReadAsFormDataAsync();
    log.Info($"Triggered WebHook with text data '{data["text"]}' by '{data["user_name"]}'");

    var errorResponse = validateAndGetErrorResponse(req, log, data["token"], data["text"], data["user_name"]);
    if (errorResponse != null)
        return errorResponse;

    sendMessage(log, data["text"]);

    return req.CreateResponse(HttpStatusCode.OK, new {
        text = $"I will pass forward '{data["text"]}'"
    });
}

private static object validateAndGetErrorResponse(HttpRequestMessage req, TraceWriter log, string token, string command, string user)
{
    if (CM.AppSettings["command_token"] != token)
    {
        log.Info($"Not valid token {token}");
        return req.CreateResponse(HttpStatusCode.Unauthorized, $"Not valid token");
    }

    if (!_validUsers.Contains(user?.ToLowerInvariant()))
    {
        log.Info($"Not valid user {user}");
        return req.CreateResponse(HttpStatusCode.OK, $"Too bad {user}, you are not part of the *club* :(");
    }

    if (!_validCommands.Contains(command?.ToLowerInvariant()))
    {
        log.Info($"Not valid command {command}");
        return req.CreateResponse(HttpStatusCode.OK, $"Not a valid command, you can only say '{String.Join(", ", _validCommands)}'.");
    }

    return null;
}

private static void sendMessage(TraceWriter log, string message)
{
    var client = new MqttClient(CM.AppSettings["mqtt_host"], 16742, false, null, null, MqttSslProtocols.None);
    var clientId = Guid.NewGuid().ToString();

    log.Info($"Client {clientId} connecting");
    var connectResponse = client.Connect(
        Guid.NewGuid().ToString(),
        CM.AppSettings["mqtt_user"],
        CM.AppSettings["mqtt_password"]);
    client.MqttMsgPublished += messagePublished;

    log.Info($"Client {clientId} connection result {connectResponse}");

    var messageId = client.Publish(
        CM.AppSettings["mqqt_topic"],
        Encoding.ASCII.GetBytes(message.ToLowerInvariant()),
        MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE,
        false);
}

private static void messagePublished(object sender, MqttMsgPublishedEventArgs e)
{
    ((MqttClient)sender).Disconnect();
}