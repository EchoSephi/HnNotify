using System.Collections.Generic;
using HnNotify.Data;

namespace HnNotify.Models {
    public class LoginViewModel {
        public string Account { get; set; }
        public string Password { get; set; }
        public string errMsg { get; set; }
    }
}