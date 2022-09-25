using JHACodingChallenge.Configurations;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using JHACodingChallenge.Controllers;
using JHACodingChallenge.Extensions;
using JHACodingChallenge.Models;
using Newtonsoft.Json;

namespace JHACodingChallenge.Services
{
    public class TwitterStreamService : ITwitterStreamService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TwitterConfiguration _twitterConfiguration;
        private readonly ILogger<TwitterStreamService> _logger;
        private HttpClient _httpClient = null;
        private string _authorization=string.Empty;
        private Uri _targetUri=null;
        public TwitterStreamService(IHttpClientFactory httpClientFactory,
            TwitterConfiguration twitterConfiguration,
            ILogger<TwitterStreamService> logger)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _twitterConfiguration = twitterConfiguration ?? throw new ArgumentNullException(nameof(twitterConfiguration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeStreamClient();
        }

        public async Task<int> GetStreamCount(DateTime runingdate)
        {
            int total_number = 0;                       
            try
            {
                _logger.LogTrace("Get total twitter live stream number ");
                using (var request = new HttpRequestMessage(HttpMethod.Get, _targetUri))
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
                _logger.LogError($"Error happen when call httpclient object, {ex.Message}");
                throw new Exception("Error happen when call Twitter API", ex);
            }

            return total_number;
        }

        public async Task<List<string>> CalculateTop10Hashtags(int stream_count)
        {
            TweetStream tweet = new TweetStream();
            
            List<Hashtags> hashtaglist = new List<Hashtags>();
            List<string> topHashtag = new List<string>();

            _logger.LogTrace("Add the specified parameter to the Query String.");

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.ParamName))
            {
                _logger.LogError("TwitterConfiguration ParmName in appseetting.json is missing");
                throw new ArgumentNullException(nameof(_twitterConfiguration.ParamName));
            }

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.ParamValue))
            {
                _logger.LogError("TwitterConfiguration ParamValue in appseetting.json is missing");
                throw new ArgumentNullException(nameof(_twitterConfiguration.ParamValue));
            }
            _targetUri = _targetUri.AddOrUpdateParameter(_twitterConfiguration.ParamName, _twitterConfiguration.ParamValue);
            
            try
            {
                _logger.LogTrace("Get top 10 twitter live stream hashtags.");
                using (var request = new HttpRequestMessage(HttpMethod.Get, _targetUri))
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
                _logger.LogError($"Error happen when call httpclient object, {ex.Message}");
                throw new Exception("Error happen when call Twitter API", ex);
            }

            try
            {
                _logger.LogTrace($"Calling GetTop10Hashtags");

                topHashtag = GetTop10Hashtags(hashtaglist);

                _logger.LogTrace($"completed GetTop10Hashtags successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error happen when call GetTop10Hashtags, {ex.Message}");
                throw new Exception("Error happen in GetTop10Hashtags", ex);
            }
            return topHashtag;
        }

        private void InitializeStreamClient()
        {
            _logger.LogTrace("Initialize HttpClient object...");
            if (string.IsNullOrWhiteSpace(_twitterConfiguration.BearerToken))
            {
                _logger.LogError("TwitterConfiguration BearerToken in appseetting.json is missing");
                throw new ArgumentNullException(nameof(_twitterConfiguration.BearerToken));
            }
            _authorization = $"Bearer {_twitterConfiguration.BearerToken}";

            if (string.IsNullOrWhiteSpace(_twitterConfiguration.BaseUrl))
            {
                _logger.LogError("TwitterConfiguration BaseUrl in appseetting.json is missing");
                throw new ArgumentNullException(nameof(_twitterConfiguration.BaseUrl));
            }

            _targetUri = new Uri(_twitterConfiguration.BaseUrl);

            try
            {
                _logger.LogTrace("Create HttpClient object");
                _httpClient = _httpClientFactory.CreateClient();

                _logger.LogTrace("Add Bearer token in HttpClient Header");
                _httpClient.DefaultRequestHeaders.Add("Authorization", _authorization);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error happen when create httpclient object, {ex.Message}");
                throw new Exception("Error happen when create httpclient object", ex);
            }


        }
        private List<string> GetTop10Hashtags(List<Hashtags> hashtaglist)
        {
            List<string> topHashtag = new List<string>();
            List<IGrouping<string, Hashtags>> list;
            list = (from hash in hashtaglist
                    group hash by hash.tag into hashs
                    orderby hashs.Count() descending
                    select hashs).Take(10).ToList();

            topHashtag = (from tag in list
                          select tag.Key).ToList();

            return topHashtag;
        }
    }
}