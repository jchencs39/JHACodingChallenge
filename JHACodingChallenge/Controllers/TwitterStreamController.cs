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
        private readonly ITwitterStreamService _twitterStreamService;              
        private readonly ILogger<TwitterStreamController> _logger;

        public TwitterStreamController(
            ITwitterStreamService twitterStreamService,
            ILogger<TwitterStreamController> logger)
        {           
            _twitterStreamService = twitterStreamService ?? throw new ArgumentNullException(nameof(twitterStreamService));
            _logger= logger ?? throw new ArgumentNullException(nameof(logger)); 
        }

        [HttpGet("api/stream/count")]
        public async Task<IActionResult> StreamCount(int seconds=60)
        {
            int total_number = 0;
            DateTime runingdate = DateTime.Now.AddSeconds(seconds);

            try
            {
                _logger.LogInformation("Starting reading twitter live stram data... ");
                _logger.LogTrace("Call TwitterStreamService.GetStreamCount()");
                
                total_number = await _twitterStreamService.GetStreamCount(runingdate);
                
                _logger.LogTrace($"Called TwitterStreamService.GetStreamCount(), received total number of tweets: {total_number.ToString()}");               
                _logger.LogInformation("End reading twitter live stram data... ");

                string received_number = $"Total number of tweet received in {seconds.ToString()} seconds : {total_number.ToString()}";
                
                return Ok(received_number);
            }
            catch(Exception ex)
            {
                _logger.LogError("Unexpected error occurs in TwitterStreamService.GetStreamCount()");
                return BadRequest($"unexpected error occurs {ex.Message}");
            }                      
        }


        [HttpGet("api/stream/hashtags")]
        public async Task<IActionResult> CalculateTop10Hashtags(int stream_count = 500)
        {
            List<string> topHashtag = new List<string>();
            try
            {
                _logger.LogInformation("Starting reading twitter live stram data... ");
                _logger.LogTrace("Call TwitterStreamService.CalculateTop10Hashtags()");
                
                topHashtag = await _twitterStreamService.CalculateTop10Hashtags(stream_count);

                _logger.LogTrace($"Called TwitterStreamService.CalculateTop10Hashtags(), top 10 hashtags list below: ");
                foreach(var tag in topHashtag)
                {
                    _logger.LogTrace($"{tag}");
                }

                _logger.LogInformation("End reading twitter live stram data... ");

                return Ok(topHashtag);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unexpected error occurs in TwitterStreamService.CalculateTop10Hashtags()");
                return BadRequest($"unexpected error occurs {ex.Message}");
            }           
        }
    }
}
