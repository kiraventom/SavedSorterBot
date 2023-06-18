using System.Net;
using VkNet.Utils;

namespace CLI;

public class AuthEndpoint
{
    private readonly string _authUrl;
    private readonly string _botUserName;

    private long SenderId { get; }
    public string Token { get; private set; }

    public AuthEndpoint(long senderId, string authUrl, string botUserName)
    {
        _authUrl = authUrl;
        _botUserName = botUserName;
        SenderId = senderId;
    }

    public async Task WaitForAuth(CancellationToken cancelToken)
    {
        var redirectUrl = _authUrl;
        if (!redirectUrl.EndsWith('/'))
            redirectUrl += '/';

        var redirectListener = CreateListener(redirectUrl);

        var realUrl = Url.Combine(_authUrl, "real/");
        var realListener = CreateListener(realUrl);

        var redirectListenerTask = StartListener(redirectListener, OnRedirectRequest, cancelToken);
        var realListenerTask = StartListener(realListener, context =>
        {
            var request = context.Request;
            if (request.HttpMethod != "GET")
                return false;

            var query = request.QueryString;
            var sender = query.Get("sender_id");
            if (!long.TryParse(sender, out var senderValue) || senderValue != SenderId)
                return false;

            Token = query.Get("access_token");

            var response = context.Response;
            response.StatusCode = (int) HttpStatusCode.Found;
            response.Headers.Add(HttpResponseHeader.Location, $"https://t.me/{_botUserName}");
            context.Response.Close();

            return true;
        }, cancelToken);

        await redirectListenerTask;
        await realListenerTask;
    }

    private bool OnRedirectRequest(HttpListenerContext context)
    {
        var request = context.Request;
        if (request.HttpMethod != "GET")
            return false;

        var query = request.QueryString;
        var sender = query.Get("sender_id");
        if (!long.TryParse(sender, out var senderValue) || senderValue != SenderId)
            return false;

        var response = context.Response;
        const string content = $"<html><script>window.location.href=window.location.href.replace('?sender_id', '/real?sender_id').replace('#', '&')</script></html>";
        const string contentType = "text/html";
        var buffer = System.Text.Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        response.ContentType = contentType;
        response.OutputStream.Write(buffer);

        context.Response.Close();

        return true;
    }

    private static HttpListener CreateListener(string url)
    {
        var listener = new HttpListener();
        listener.Prefixes.Add(url);

        return listener;
    }

    private static async Task StartListener(HttpListener listener, Func<HttpListenerContext, bool> handleRequest,
        CancellationToken token)
    {
        try
        {
            listener.Start();

            while (true)
            {
                var context = await Task.Run(listener.GetContext, token);
                var shouldStop = handleRequest(context);
                if (shouldStop)
                    break;
            }
        }
        finally
        {
            listener.Stop();
            listener.Close();
        }
    }
}