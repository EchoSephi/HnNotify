using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace HnNotify.Service {
    public class UStore {
        public static string GetUStore (string ConfigurationStirng) {
            string _apServerName = "";
            string _apDatabseName = "";
            string _apLogin = "";
            string _apPassword = "";
            string _connectionStr = "";

            try {
                using (StreamReader r = new StreamReader (@"/Users/echok/Release/HNapi.json")) {
                    string json = r.ReadToEnd ();
                    dynamic jr = JArray.Parse (json);
                    dynamic p = jr[0];

                    _apServerName = p.APServerName;
                    _apDatabseName = p.APDatabaseName;
                    _apLogin = p.APLogin;
                    _apPassword = p.APpassword;

                    _connectionStr = ConfigurationStirng;
                    _connectionStr = _connectionStr.Replace ("APServerName", _apServerName);
                    _connectionStr = _connectionStr.Replace ("APDatabaseName", _apDatabseName);
                    _connectionStr = _connectionStr.Replace ("APLogin", _apLogin);
                    _connectionStr = _connectionStr.Replace ("APpassword", _apPassword);
                }
            } catch (System.Exception) {
                _connectionStr = ConfigurationStirng;
            }

            return _connectionStr;

        }

    }
}