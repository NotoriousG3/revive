using System.Net;

namespace TaskBoard;

public static class HttpHandlerGenerator
{
    public static HttpClientHandler WithProxy(IServiceProvider provider)
    {
        var scope = provider.CreateScope();
        var proxyManager = scope.ServiceProvider.GetRequiredService<IProxyManager>();
        var proxyInfo = proxyManager.Take().Result;
        var credentials = new NetworkCredential(proxyInfo.User, proxyInfo.Password);

        var proxy = new WebProxy(proxyInfo.Address);
        proxy.Credentials = credentials;
        return new HttpClientHandler
        {
            Proxy = proxy
        };
    }
}