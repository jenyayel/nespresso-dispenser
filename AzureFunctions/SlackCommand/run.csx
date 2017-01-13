using System;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Azure.Devices;
using System.Text;
using System.Configuration;

private static readonly string[] _validCommands = new string[] { "1", "2", "3", "4", "5" };
private static ServiceClient serviceClient = ServiceClient.CreateFromConnectionString(
    ConfigurationManager.ConnectionStrings["iot-hub"].ConnectionString);
private const string deviceId = "nespresso-alpha";

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log)
{
    var data = await req.Content.ReadAsFormDataAsync();
    log.Info($"Triggered WebHook with text data '{data["text"]}' by 'data["user_name"]'");

    if (!_validCommands.Contains(data["text"]))
        return req.CreateResponse(HttpStatusCode.OK, $"{data["user_name"]}, the only valid commands are '{String.Join(", ", _validCommands)}'.");

    await send(data["text"]);

    return req.CreateResponse(HttpStatusCode.OK, $"I'm opening capsule '{data["text"]}'");
}

private async static Task send(string message)
{
    var commandMessage = new Message(Encoding.ASCII.GetBytes(message));
    await serviceClient.SendAsync(deviceId, commandMessage);
}