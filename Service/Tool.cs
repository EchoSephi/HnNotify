using System;
using System.Globalization;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace HnNotify.Service {
    public class Tool {
        public static string Encrypt (string pwd) {
            string pw;
            Int32 I;
            Int32 j;
            string h1;
            string h2;
            string H;       
            int c;

            pw = char.ConvertFromUtf32 ((int) 84) +
                char.ConvertFromUtf32 ((int) 101) +
                char.ConvertFromUtf32 ((int) 99) +
                char.ConvertFromUtf32 ((int) 104) +
                char.ConvertFromUtf32 ((int) 114) +
                char.ConvertFromUtf32 ((int) 111) +
                char.ConvertFromUtf32 ((int) 117) +
                char.ConvertFromUtf32 ((int) 112);

            pwd = pwd.ToUpper ();
            I = 0 ;
            var newpwd = "";
            for (j = 0; j < pwd.Length; j++) {
                ASCIIEncoding asciiEncoding = new ASCIIEncoding ();
                c = (int) asciiEncoding.GetBytes (pw.Substring (I, 1)) [0];

                I = I + 1;
                if (I > pw.Length) {
                    I = 1  ;
                }

                ASCIIEncoding ascii2 = new ASCIIEncoding ();
                var c1 = (int) ascii2.GetBytes (pwd.Substring (j, 1)) [0];
                var s = char.ConvertFromUtf32 (c1 ^ c);

                newpwd = newpwd + s;
            }
            pwd = newpwd;
            h1 = ""     ;   
            h2 = ""   ;                                             
            for (j = 0; j < pwd.Length; j++) { 
                ASCIIEncoding ascii1 = new ASCIIEncoding ();
                var cc = (int) ascii1.GetBytes (pwd.Substring (j, 1)) [0];
                H = Convert.ToString (cc, 16);

                if (H.Length == 1) {
                    H = "0" + H;
                }
                h1 = h1 + H.Substring (0, 1);
                h2 = h2 + H.Substring (H.Length - 1, 1);

            }
            var nP = h1.ToUpper () + h2.ToUpper ();
            return nP;
        }

        public static bool tPing () {
            Ping a = new Ping ();
            PingReply re = a.Send ("8.8.8.8");
            if (re.Status == IPStatus.Success) {
                return true;
            } else {
                return false;
            }
        }

        bool invalid = false;
        public bool IsValidEmail (string strIn) {
            invalid = false;
            if (String.IsNullOrEmpty (strIn)) {
                return false;
            }

            try {
                strIn = Regex.Replace (strIn, @"(@)(.+)$", this.DomainMapper,
                    RegexOptions.None, TimeSpan.FromMilliseconds (200));
            } catch (RegexMatchTimeoutException) {
                return false;
            }

            if (invalid) {
                return false;
            }

            try {
                return Regex.IsMatch (strIn,
                    @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                    RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds (250));
            } catch (RegexMatchTimeoutException) {
                return false;
            }
        }

        private string DomainMapper (Match match) {
            IdnMapping idn = new IdnMapping ();

            string domainName = match.Groups[2].Value;
            try {
                domainName = idn.GetAscii (domainName);
            } catch (ArgumentException) {
                invalid = true;
            }
            return match.Groups[1].Value + domainName;
        }

    }
}