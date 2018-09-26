using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HnNotify.Controllers
{
    [Route("api/[controller]")]
    public class LineNotifyController : Controller
    {
        private readonly string _notifyUrl;

        public LineNotifyController()
        {
            _notifyUrl = "https://notify-api.line.me/api/notify";
        }

        // postman 測試方式 (localhost:5000是網址)
        // http://localhost:5000/api/LineNotify/SendMessage?token=VplEe3RvKjJgfuaoWG8otpixHfMLolxHuHS585zhDBq&message=HelloWorld
        [HttpGet]
        [Route("SendMessage")]
        public async Task<IActionResult> SendMessage(string token, string message)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(_notifyUrl);
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + token);

                var form = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("message", message)
                });

                await client.PostAsync("", form);
            }

            return new EmptyResult();
        }
    }
}