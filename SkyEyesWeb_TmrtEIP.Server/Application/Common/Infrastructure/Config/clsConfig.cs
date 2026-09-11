using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class clsConfig
{
    public static string ConnectionStrings { get; set; }
	public static string SysVersion { get; set; }
	public static int TokenLimit { get; set; }

    public static string GetHttpClient(string url, object parameters = null, Dictionary<string, string> header = null)
    {
        return GetHttpClientAsync(url, parameters, header).GetAwaiter().GetResult();
    }

    public static async Task<string> GetHttpClientAsync(string url, object parameters = null, Dictionary<string, string> header = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        try
        {
            using HttpClient client = new HttpClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url);

            using StringContent content = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");
            request.Content = content;

            if (header != null)
            {
                foreach (KeyValuePair<string, string> kvp in header)
                {
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
                }
            }

            using HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }
        catch
        {
            throw;
        }
    }

}
