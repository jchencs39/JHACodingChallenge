using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Runtime;
using System.Text;
using Newtonsoft.Json;
using JHACodingChallenge.Extensions;
using static System.Net.Mime.MediaTypeNames;
using JHACodingChallenge.Models;
using System.Collections.Generic;
using System.Net.Http;
using JHACodingChallenge.Configurations;
using JHACodingChallenge.Services;

namespace JHACodingChallenge.Controllers
{
   
    public class TwitterStreamController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TwitterConfiguration _twitterConfiguration;
        private readonly IHashTagService _hashTagService;
        private HttpClient _httpClient=null;
        public TwitterStreamController(IHttpClientFactory httpClientFactory,
            TwitterConfiguration twitterConfiguration, 
            IHashTagService hashTagService)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _twitterConfiguration = twitterConfiguration ?? throw new ArgumentNullException(nameof(twitterConfiguration));
            _hashTagService= hashTagService ?? throw new ArgumentNullException(nameof(hashTagService));
        }

        [HttpGet("api/stream/count")]
        public async Task<IActionResult> StreamCount(int seconds=60)
        {
            int total_number = 0;
            DateTime runingdate = DateTime.Now.AddSeconds(seconds);

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.BearerToken))
                throw new ArgumentNullException(nameof(_twitterConfiguration.BearerToken));

            string Authorization = $"Bearer {_twitterConfiguration.BearerToken}";

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.BaseUrl))
                throw new ArgumentNullException(nameof(_twitterConfiguration.BaseUrl));

            Uri targetUri = new Uri(_twitterConfiguration.BaseUrl);

            try
            {
                _httpClient = _httpClientFactory.CreateClient();
            }
            catch (Exception ex)
            {
                throw new Exception("Error happen when create httpclient object", ex);
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", Authorization);

                using (var request = new HttpRequestMessage(HttpMethod.Get, targetUri))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                    request.Headers.Connection.Add("keep-alive");
                    using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        using (var sr = new StreamReader(content, Encoding.UTF8))
                        {
                            while (runingdate > DateTime.Now)
                            {
                                var line = await sr.ReadLineAsync().ConfigureAwait(false);
                                total_number++;
                            }
                        }
                    }
                }               
            }
            catch (Exception ex)
            {
                throw new Exception("Error happen when call Twitter API", ex);
            }
            string received_number = $"Total number of tweet received in {seconds.ToString()} seconds : {total_number.ToString()}";
            return Ok(received_number);
        }

 


        [HttpGet("api/stream/hashtags")]
        public async Task<IActionResult> Top10Hashtags(int stream_count = 500)
        {            
            
            TweetStream tweet = new TweetStream();
            //List<TweetData> tweetDatas = new List<TweetData>();
            List<Hashtags> hashtaglist = new List<Hashtags>();
            List<string> topHashtag = new List<string>();

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.BearerToken))
                throw new ArgumentNullException(nameof(_twitterConfiguration.BearerToken));

            string Authorization = $"Bearer {_twitterConfiguration.BearerToken}";

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.BaseUrl))
                throw new ArgumentNullException(nameof(_twitterConfiguration.BaseUrl));

            Uri targetUri = new Uri(_twitterConfiguration.BaseUrl);

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.ParamName))
                throw new ArgumentNullException(nameof(_twitterConfiguration.ParamName));
            
            if (string.IsNullOrWhiteSpace(_twitterConfiguration.ParamValue))
                throw new ArgumentNullException(nameof(_twitterConfiguration.ParamValue));
            
            targetUri = targetUri.AddOrUpdateParameter(_twitterConfiguration.ParamName, _twitterConfiguration.ParamValue);

            try
            {
                _httpClient = _httpClientFactory.CreateClient();
            }
            catch (Exception ex)
            {
                throw new Exception("Error happen when create httpclient object", ex);
            }

            try
            {
                _httpClient.DefaultRequestHeaders.Add("Authorization", Authorization);

                using (var request = new HttpRequestMessage(HttpMethod.Get, targetUri))
                {
                    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
                    request.Headers.Connection.Add("keep-alive");
                    using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        var content = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                        using (var sr = new StreamReader(content, Encoding.UTF8))
                        {
                            while (stream_count > 0)
                            {
                                string line = await sr.ReadLineAsync().ConfigureAwait(false);

                                TweetData? tweetData = JsonConvert.DeserializeObject<TweetStream>(line).data;
                                if (tweetData != null && tweetData.entities != null && tweetData.entities.hashtags != null)
                                {
                                    List<Hashtags> hashtags = tweetData.entities.hashtags;
                                    hashtaglist.AddRange(hashtags);

                                }
                                stream_count--;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error happen when call Twitter API", ex);
            }

            try
            {
                topHashtag = _hashTagService.GetTop10Hashtags(hashtaglist);
            }
            catch (Exception ex)
            {
                throw new Exception("Error happen in HashTagService", ex);
            }

           return Ok(topHashtag); 
        }
    }
}
