using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace LineMessaging
{
    public partial class LineMessagingClient
    {
        private static readonly MediaTypeHeaderValue MediaTypeJson = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");
        private static readonly MediaTypeHeaderValue MediaTypeJpeg = MediaTypeHeaderValue.Parse("image/jpeg");
        private static readonly MediaTypeHeaderValue MediaTypePng = MediaTypeHeaderValue.Parse("image/png");
        private static readonly HttpClient HttpClient = new HttpClient
        {
            BaseAddress = LineConstants.BaseUri,
            Timeout = TimeSpan.FromSeconds(10)
        };
        private static readonly HttpClient HttpDataClient = new HttpClient
        {
            BaseAddress = LineConstants.BaseDataUri,
            Timeout = TimeSpan.FromSeconds(10)
        };

        private readonly AuthenticationHeaderValue accessTokenHeaderValue;

        public LineMessagingClient(string accessToken)
        {
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new ArgumentException($"{nameof(accessToken)} is null or empty.");
            }

            accessTokenHeaderValue = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        private async Task<T> Get<T>(string path, Dictionary<string, object> query = null)
        {
            return await SendRequest<T>(HttpMethod.Get, path, query);
        }

        private async Task<T> Post<T>(string path, object body)
        {
            return await SendRequest<T>(HttpMethod.Post, path, null, body);
        }

        private async Task Post(string path, object body = null)
        {
            await SendRequest(HttpMethod.Post, path, null, body);
        }

        private async Task PostJpeg(string path, byte[] image)
        {
            await PostImage(path, "jpeg", image);
        }

        private async Task PostPng(string path, byte[] image)
        {
            await PostImage(path, "png", image);
        }

        private async Task Delete(string path)
        {
            await SendRequest(HttpMethod.Delete, path);
        }


        /// <summary>
        /// Download message content, write to local storage. Determine file extension based on content-type.<para />
        /// Create by phidiassj.
        /// </summary>
        /// <param name="url">Line get content API url</param>
        /// <param name="path">Local path to write</param>
        /// <param name="filename">Filename without extension</param>
        /// <returns>Return full filename with path if success. Otherwise null.</returns>
        public async Task<string> GetContentAndSave(string url, string path, string filename)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri("https://api-data.line.me/");
                    httpClient.DefaultRequestHeaders.Authorization = accessTokenHeaderValue;
                    var response = await httpClient.GetAsync(url);
                    if (response == null) return null;
                    if (!response.IsSuccessStatusCode) return null;
                    if (response.Content.Headers.ContentType == null ||
                        response.Content.Headers.ContentType.MediaType == null) return null;

                    var contentStream = await response.Content.ReadAsStreamAsync();
                    var contentType = response.Content.Headers.ContentType.MediaType;
                    var extension = "";

                    switch (contentType.ToLower())
                    {
                        case "audio/wav":
                        case "audio/wave":
                        case "audio/x-wav":
                        case "audio/x-pn-wav":
                            extension = ".wav";
                            break;
                        case "audio/ac3":
                            extension = ".ac3";
                            break;
                        case "audio/aac":
                            extension = ".aac";
                            break;
                        case "audio/x-m4a":
                        case "audio/mp3":
                        case "audio/mpeg":
                            extension = ".mp3";
                            break;
                        case "audio/ogg":
                            extension = ".ogg";
                            break;
                        case "audio/mpa":
                            extension = ".mpa";
                            break;
                        case "audio/mp4":
                        case "video/mp4":
                            extension = ".mp4";
                            break;
                        case "image/gif":
                            extension = ".gif";
                            break;
                        case "image/jpeg":
                            extension = ".jpg";
                            break;
                        case "image/png":
                            extension = ".png";
                            break;
                        case "application/pdf":
                            extension = ".pdf";
                            break;
                        // Add more cases as needed
                        default:
                            extension = "";
                            break;
                    }

                    // Undefine file types.
                    if (string.IsNullOrWhiteSpace(extension)) return null;

                    string completeName = filename + extension;
                    var filePath = Path.Combine(path, completeName);

                    try
                    {
                        using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 3145728, true))
                        {
                            await contentStream.CopyToAsync(stream);
                        }
                    } catch 
                    {
                        return null;
                    }

                    return filePath;
                }
            }
            catch
            {
                return null;
            }
        }



        private async Task<byte[]> GetAsByteArray(string path)
        {
            using (var message = new HttpRequestMessage(HttpMethod.Get, path))
            {
                message.Headers.Authorization = accessTokenHeaderValue;
                HttpResponseMessage response;
                try
                {
                    response = await HttpDataClient.SendAsync(message);
                }
                catch (TaskCanceledException)
                {
                    throw new LineMessagingException(path, "Request Timeout");
                }

                await CheckStatusCode(path, response);
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        private async Task<T> SendRequest<T>(HttpMethod method, string path,
            Dictionary<string, object> query = null, object body = null)
        {
            var responseJson = await SendRequest(method, path, query, body);
            return JsonConvert.DeserializeObject<T>(responseJson);
        }

        private async Task PostImage(string path, string imageFormat, byte[] image)
        {
            using (var message = new HttpRequestMessage(HttpMethod.Post, path))
            {
                message.Content = new ByteArrayContent(image);
                switch (imageFormat)
                {
                    case "jpeg":
                        message.Content.Headers.ContentType = MediaTypeJpeg;
                        break;

                    case "png":
                        message.Content.Headers.ContentType = MediaTypePng;
                        break;

                    default:
                        throw new LineMessagingException(path, $"{imageFormat} is not supported.");
                }
                message.Headers.Authorization = accessTokenHeaderValue;

                HttpResponseMessage response;
                try
                {
                    response = await HttpClient.SendAsync(message);
                }
                catch (TaskCanceledException)
                {
                    throw new LineMessagingException(path, "Request Timeout");
                }
            }
        }

        private async Task<string> SendRequest(HttpMethod method, string path,
            Dictionary<string, object> query = null, object body = null)
        {
            string queryString = null;
            if (query != null)
            {
                queryString = query.ToQueryString();
            }

            using (var message = new HttpRequestMessage(method, path + queryString))
            {
                if (body != null)
                {
                    message.Content = new StringContent(JsonConvert.SerializeObject(body));
                    message.Content.Headers.ContentType = MediaTypeJson;
                }
                message.Headers.Authorization = accessTokenHeaderValue;

                HttpResponseMessage response;
                try
                {
                    response = await HttpClient.SendAsync(message);
                }
                catch (TaskCanceledException)
                {
                    throw new LineMessagingException(path, "Request Timeout");
                }

                await CheckStatusCode(path, response);
                return await response.Content.ReadAsStringAsync();
            }
        }

        private static async Task CheckStatusCode(string path, HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var error = JsonConvert.DeserializeObject<LineErrorResponse>(responseJson);
                if (error != null)
                {
                    throw new LineMessagingException(path, error);
                }
                throw new LineMessagingException(path,
                    $"Error has occurred. Response StatusCode:{response.StatusCode} ReasonPhrase:{response.ReasonPhrase}.");
            }
        }
    }
}
