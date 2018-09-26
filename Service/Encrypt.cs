// using System;
// using System.Text;
// using Microsoft.CodeAnalysis.CSharp;

// namespace HnNotify.Service {
//     public class Encrypt {
//         public static string encrypt (string pwd) {
//             string pw;
//             Int32 I;
//             Int32 j;
//             string h1;
//             string h2;
//             string H;       
//             int c;

//             pw = char.ConvertFromUtf32 ((int) 84) +
//                 char.ConvertFromUtf32 ((int) 101) +
//                 char.ConvertFromUtf32 ((int) 99) +
//                 char.ConvertFromUtf32 ((int) 104) +
//                 char.ConvertFromUtf32 ((int) 114) +
//                 char.ConvertFromUtf32 ((int) 111) +
//                 char.ConvertFromUtf32 ((int) 117) +
//                 char.ConvertFromUtf32 ((int) 112);

//             pwd = pwd.ToUpper ();
//             I = 0 ;
//             var newpwd = "";
//             for (j = 0; j < pwd.Length; j++) {
//                 ASCIIEncoding asciiEncoding = new ASCIIEncoding ();
//                 c = (int) asciiEncoding.GetBytes (pw.Substring (I, 1)) [0];

//                 I = I + 1;
//                 if (I > pw.Length) {
//                     I = 1  ;
//                 }

//                 ASCIIEncoding ascii2 = new ASCIIEncoding ();
//                 var c1 = (int) ascii2.GetBytes (pwd.Substring (j, 1)) [0];
//                 var s = char.ConvertFromUtf32 (c1 ^ c);
                
//                 newpwd = newpwd + s;
//             }
//             pwd = newpwd;
//             h1 = ""     ;   
//             h2 = ""   ;                                             
//             for (j = 0; j < pwd.Length; j++) { 
//                 ASCIIEncoding ascii1 = new ASCIIEncoding ();
//                 var cc = (int) ascii1.GetBytes (pwd.Substring (j, 1)) [0];
//                 H = Convert.ToString (cc, 16);

//                 if (H.Length == 1) {
//                     H = "0" + H;
//                 }
//                 h1 = h1 + H.Substring (0, 1);
//                 h2 = h2 + H.Substring (H.Length - 1, 1);

//             }
//             var nP = h1.ToUpper () + h2.ToUpper ();
//             return nP;
//         }
//     } 
// }