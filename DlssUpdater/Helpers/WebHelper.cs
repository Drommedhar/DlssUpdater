using System.Diagnostics;
using System.Net;
using System.Net.Http;

namespace DlssUpdater.Helpers;

public static class WebHelper
{
    /// <summary>
    ///     This method will check a url to see that it does not return server or protocol errors
    /// </summary>
    /// <param name="url">The path to check</param>
    /// <returns></returns>
    public static bool UrlIsValid(string url)
    {
        try
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            var request = WebRequest.Create(url) as HttpWebRequest;
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            if (request is null)
            {
                return false;
            }

            request.Timeout =
                5000; //set the timeout to 5 seconds to keep the user from waiting too long for the page to load
            request.Method = "HEAD"; //Get only the header information -- no need to download any content

            using (var response = request.GetResponse() as HttpWebResponse)
            {
                var statusCode = (int)response!.StatusCode;
                if (statusCode >= 100 && statusCode < 400) //Good requests
                {
                    return true;
                }

                if (statusCode >= 500 && statusCode <= 510) //Server Errors
                {
                    Debug.WriteLine(
                        string.Format("The remote server has thrown an internal error. Url is not valid: {0}", url));
                    return false;
                }
            }
        }
        catch (WebException ex)
        {
            if (ex.Status == WebExceptionStatus.ProtocolError) //400 errors
            {
                return false;
            }
        }
        catch (Exception)
        {
        }

        return false;
    }

    public static async Task<string> HttpGet(string url)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }

    public static async Task<string> HttpPost(string url, KeyValuePair<string, string>[] formData)
    {
        var httpClient = new HttpClient();
        var formContent = new FormUrlEncodedContent(formData);
        var response = await httpClient.PostAsync(url, formContent);
        return await response.Content.ReadAsStringAsync();
    }
}