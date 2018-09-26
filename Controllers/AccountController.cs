using System;
using System.Collections.Generic;
// using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using HnNotify.Data;
using HnNotify.Models;
using HnNotify.Service;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// 上面是using
namespace HnNotify.Controllers {
    public class AccountController : Controller {
        private readonly string _authorizeUrl;
        private readonly string _tokenUrl;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _redirectUri;
        private readonly string _state;
        private static int _SessionCounter { get; set; }
        private static string _SessionName { get; set; }
        private static string _SessionMail { get; set; }
        private static string _testAccount { get; set; }
        private static string _testPassword { get; set; }
        private IMembersService _IMembersService;
        private INotifyItemService _INotifyItemService;
        private INotifyService _INotifyService;
        private IConfiguration _Configuration { get; }

        public AccountController (IMembersService IMembersService,
            INotifyItemService INotifyItemService,
            INotifyService INotifyService,
            IConfiguration Configuration) {
            this._IMembersService = IMembersService;
            this._INotifyItemService = INotifyItemService;
            this._INotifyService = INotifyService;
            this._Configuration = Configuration;

            // Line Notify 服務網址
            _authorizeUrl = "https://notify-bot.line.me/oauth/authorize";
            _tokenUrl = "https://notify-bot.line.me/oauth/token";

            // Line Notify 服務的識別碼
            _clientId = "RZ8jjLMT1xtmzKoW6LxLKQ";
            _clientSecret = "w0GXh4Ig1rVwZgRRySDcnSJXk8fg3jVsWexIyvgiMuf";

            // 後端接收callback的網址
            // _redirectUri = "http://localhost:5000/Account/LineNotify";
            _redirectUri = _Configuration["LineCallBack:Url"];

            // 可透過 State 避免 CSRF 攻擊
            _state = "NO_STATE";

            if (_Configuration["TestAccount:account"] != null) {
                var t1 = _Configuration["TestAccount:account"];
                _testAccount = t1;
            } else {
                _testAccount = "";
            }

            if (_Configuration["TestAccount:password"] != null) {
                var p1 = _Configuration["TestAccount:password"];
                _testPassword = p1;
            } else {
                _testPassword = "";
            }

        }

        [HttpGet]
        public IActionResult Login (string errMsg = "", string account = "") {
            var lv = new LoginViewModel ();
            if (errMsg != "") {
                lv.Account = account;
                lv.Password = "";
                lv.errMsg = errMsg;
            } else {
                lv.Account = _testAccount;
                lv.Password = _testPassword;
                lv.errMsg = "";
            }

            return View (lv);
        }

        [HttpPost]
        public async Task<IActionResult> Login (LoginViewModel dto) {
            string errMsg = "";
            if (dto.Account == "" || dto.Account == null) {
                errMsg = "請輸入帳號";
                return RedirectToAction ("Login", "Account", new { errMsg = errMsg, account = dto.Account });
            }

            if (dto.Password == "" || dto.Password == null) {
                errMsg = "請輸入密碼";
                return RedirectToAction ("Login", "Account", new { errMsg = errMsg, account = dto.Account });
            }

            var member = await this._IMembersService.ChkUserLogin (dto.Account);

            if (member != null) {
                _SessionCounter = member.Counter;
                _SessionName = member.Name;
                _SessionMail = member.Email;
                var encryptPwd = Tool.Encrypt (dto.Password);
                if (member.PassWord == encryptPwd) {
                    // 登入成功後,若沒有 LineToken , 先做驗證取得
                    if (member.LineToken == "" || member.LineToken == null) {
                        // 先確定有聯網再做
                        // 2018,8,13 ,Akai設定外部網路所以不需要判斷此項了
                        // if (Tool.tPing () == true) {
                            var URL = Uri.EscapeUriString (
                                _authorizeUrl + "?" +
                                "response_type=code" +
                                "&client_id=" + _clientId +
                                "&redirect_uri=" + _redirectUri +
                                "&scope=notify" +
                                "&state=" + _state
                            );
                            return Redirect (URL);
                        // } else {
                        //     // todo 直接取得服務訂閱?
                        //     return RedirectToAction ("Notify", "Account", new { memberCounter = member.Counter, memberEmail = member.Email });
                        // }
                    } else {
                        // 登入成功後,若有LineToken,直接取得服務訂閱
                        return RedirectToAction ("Notify", "Account", new { memberCounter = member.Counter, memberEmail = member.Email });
                    }
                } else {
                    errMsg = "密碼錯誤,請重新登入";
                    return RedirectToAction ("Login", "Account", new { errMsg = errMsg, account = dto.Account });
                }
            } else {
                errMsg = "無此帳號資訊";
                return RedirectToAction ("Login", "Account", new { errMsg = errMsg, account = dto.Account });
            }
        }

        // 顯示訂閱資料
        [HttpGet]
        public async Task<IActionResult> Notify (int memberCounter, string memberEmail, string errMsg = "") {
            var qNotify = await this._INotifyService.GetNotify (memberCounter);
            var _dtoNotifyS = new dtoNotifyS ();
            _dtoNotifyS.MAIL = memberEmail;

            if (errMsg != "") {
                _dtoNotifyS.errMsg = errMsg;
            } else {
                _dtoNotifyS.errMsg = "";
            }

            if (qNotify.Count == 0) {
                // 無訂閱資料
                var q = await this._INotifyItemService.GetNotifyItem ();
                List<dtoNotify> d = new List<dtoNotify> ();
                foreach (var p in q) {
                    var notify = new dtoNotify ();
                    notify.NotifyItem_Counter = p.Counter;
                    notify.NotifyItem_Name = p.Name;
                    notify.Member_Counter = memberCounter;
                    notify.Line = false;
                    notify.Mail = false;

                    d.Add (notify);
                }
                _dtoNotifyS.dtoNotify = d;
            } else {
                _dtoNotifyS.dtoNotify = qNotify;
            }

            return View (_dtoNotifyS);
        }

        // 更新或新增訂閱資料
        [HttpPost]
        public async Task<IActionResult> Notify (dtoNotifyS dto) {
            var memberCounter = dto.dtoNotify.FirstOrDefault ().Member_Counter;

            bool isCheckMail = false;
            foreach (var p in dto.dtoNotify) {
                if (p.Mail == true) {
                    isCheckMail = true;
                    break;
                }
            }
            // check email format
            if (isCheckMail == true) {
                if (dto.MAIL == "" || dto.MAIL == null) {
                    return RedirectToAction ("Notify", "Account", new {
                        memberCounter = memberCounter,
                            memberEmail = "",
                            errMsg = "請輸入電子郵件"
                    });
                }
                var s = new Tool ().IsValidEmail (dto.MAIL);
                if (s == false) {
                    return RedirectToAction ("Notify", "Account", new {
                        memberCounter = memberCounter,
                            memberEmail = dto.MAIL,
                            errMsg = "電子郵件格式不正確"
                    });
                }

                // 更新mail
                await this._IMembersService.SetUserMail (memberCounter, dto.MAIL);

            }

            // 更新或新增訂閱資料
            await this._INotifyService.SetNotify (dto.dtoNotify);

            // 顯示訂閱結果
            return RedirectToAction ("ShowNotify", "Account", new { memberCounter = memberCounter });
        }

        // 顯示訂閱結果
        [HttpGet]
        public async Task<IActionResult> ShowNotify (int memberCounter) {
            var q = await this._INotifyService.ShowNotify (memberCounter);
            q.Name = _SessionName;
            if (q.dtoLine.FirstOrDefault () != null) {
                ViewBag.Line = "您訂閱的 Line 服務";
            } else {
                ViewBag.Line = "您尚無訂閱 Line 服務";
            }

            if (q.dtoMail.FirstOrDefault () != null) {
                ViewBag.Mail = "您訂閱的 Mail 服務";
            } else {
                ViewBag.Mail = "您尚無訂閱 Mail 服務";
            }
            return View (q);
        }

        // 取回line回傳的token
        [HttpGet]
        public async Task<IActionResult> LineNotify (
            [FromQuery] string code, [FromQuery] string state, [FromQuery] string error, [FromQuery][JsonProperty ("error_description")] string errorDescription) {
            if (!string.IsNullOrEmpty (error))
                return new JsonResult (new {
                    error,
                    state,
                    errorDescription
                });

            var token = await FetchToken (code);

            var memberCounter = _SessionCounter;
            var memberName = _SessionName;
            var memberEmail = _SessionMail;

            // 設定使用者的 line token
            await this._IMembersService.SetUserToken (memberCounter, token);

            // 發送第一次歡迎使用通知
            using (var client = new HttpClient ()) {
                client.BaseAddress = new Uri ("https://notify-api.line.me/api/notify");
                client.DefaultRequestHeaders.Add ("Authorization", "Bearer " + token);

                string msg = string.Format ("Hello {0} , 若您有設定宏恩醫院通知功能,以後會在此通知您!", memberName);

                var form = new FormUrlEncodedContent (new [] {
                    new KeyValuePair<string, string> ("message", msg)
                });

                await client.PostAsync ("", form);
            }

            // 返回設定訂閱服務畫面
            return RedirectToAction ("Notify", "Account", new { memberCounter = memberCounter, memberEmail = memberEmail });

        }

        //  解析line傳資料取得token
        private async Task<string> FetchToken (string code) {
            using (var client = new HttpClient ()) {
                client.Timeout = new TimeSpan (0, 0, 60);
                client.BaseAddress = new Uri (_tokenUrl);

                var content = new FormUrlEncodedContent (new [] {
                    new KeyValuePair<string, string> ("grant_type", "authorization_code"),
                        new KeyValuePair<string, string> ("code", code),
                        new KeyValuePair<string, string> ("redirect_uri", _redirectUri),
                        new KeyValuePair<string, string> ("client_id", _clientId),
                        new KeyValuePair<string, string> ("client_secret", _clientSecret)
                });
                var response = await client.PostAsync ("", content);
                var data = await response.Content.ReadAsStringAsync ();

                return JsonConvert.DeserializeObject<JObject> (data) ["access_token"].ToString ();
            }
        }

    }
}