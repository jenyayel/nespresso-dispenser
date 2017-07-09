using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Configuration;
using System.Threading;
using CM = System.Configuration.ConfigurationManager;

private static readonly HashSet<string> _validCommands = new HashSet<string>(CM.AppSettings["valid_commands"].Split(',').Select(s => s.Trim().ToLower()));
private static readonly HashSet<string> _validUsers = new HashSet<string>(CM.AppSettings["valid_users"].Split(',').Select(s => s.Trim().ToLower()));

public static async Task<object> Run(HttpRequestMessage req, TraceWriter log, out string outputMessage)
{
    outputMessage = null;
    if (req.Method == HttpMethod.Get)
        return req.CreateResponse(HttpStatusCode.OK); // test/ping  requests

    var data = await req.Content.ReadAsFormDataAsync();
    log.Info($"Triggered WebHook with text data '{data["text"]}' by '{data["user_name"]}'");

    var errorResponse = validateAndGetErrorResponse(req, log, data["token"], data["text"], data["user_name"]);
    if (errorResponse != null)
        return errorResponse;

    outputMessage = data["text"];
    return getResponse(req, $"I will pass forward '{data["text"]}'");
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
        return getResponse(req, $"Too bad {user}, you are not part of the *club* :(", false);
    }

    if (!_validCommands.Contains(command?.ToLowerInvariant()))
    {
        log.Info($"Not valid command {command}");
        return getResponse(req, $"Not a valid command, you can only say '{String.Join(", ", _validCommands)}'.", false);
    }

    return null;
}

private static object getResponse(HttpRequestMessage req, string message, bool? isGood = null)
{
    return req.CreateResponse(HttpStatusCode.OK, new
    {
        text = message,
        mrkdwn = true,
        color = isGood.HasValue ? (isGood.Value ? "good" : "danger") : null
    });
}