using System.IO;
using System;

public class ProxyExtension : IDisposable
{
    private readonly string _directory;

    public ProxyExtension(string host, int port, string user, string password)
    {
        _directory = Path.GetFullPath(Path.Combine(Path.GetTempPath(), Path.GetRandomFileName()));
        System.IO.Directory.CreateDirectory(_directory);
        var manifestFile = Path.Combine(_directory, "manifest.json");
        File.WriteAllText(manifestFile, ManifestJson);

        var backgroundJs = string.Format(BackgroundJs, host, port, user, password);
        var backgroundFile = Path.Combine(_directory, "background.js");
        File.WriteAllText(backgroundFile, backgroundJs);
    }

    public string Directory => _directory;

    public void Dispose()
    {
        System.IO.Directory.Delete(_directory, recursive: true);
    }

    private const string ManifestJson = @"
        {
            ""version"": ""1.0.0"",
            ""manifest_version"": 2,
            ""name"": ""Chrome Proxy"",
            ""permissions"": [
                ""proxy"",
                ""tabs"",
                ""unlimitedStorage"",
                ""storage"",
                ""<all_urls>"",
                ""webRequest"",
                ""webRequestBlocking""
            ],
            ""background"": {""scripts"": [""background.js""]},
            ""minimum_chrome_version"": ""76.0.0""
        }
        ";

    private const string BackgroundJs = @"
        var config = {{
            mode: ""fixed_servers"",
            rules: {{
                singleProxy: {{
                    scheme: ""http"",
                    host: ""{0}"",
                    port: {1}
                }},
                bypassList: [""localhost""]
            }}
        }};

        chrome.proxy.settings.set({{value: config, scope: ""regular""}}, function() {{}});

        function callbackFn(details) {{
            return {{
                authCredentials: {{
                    username: ""{2}"",
                    password: ""{3}""
                }}
            }};
        }}

        chrome.webRequest.onAuthRequired.addListener(
            callbackFn,
            {{ urls: [""<all_urls>""] }},
            ['blocking']
        );
        ";
}