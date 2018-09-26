using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HnNotify.Data {
    public class spMember {
        [Key]
        public int Counter { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string PassWord { get; set; }
        public string Email { get; set; }
        public string LineToken { get; set; }
    }

    public interface IMembersService {
        Task<spMember> ChkUserLogin (string account);
        Task<spMember> SetUserToken (int counter, string token);
        Task<spMember> SetUserMail (int counter, string mail);
    }

    public class MembersService : IMembersService {
        private HNContext _dbContext;
        public MembersService (HNContext dbContext) {
            this._dbContext = dbContext;
        }

        // 用登入帳號取得 user 資訊
        public async Task<spMember> ChkUserLogin (string account) {
            var Account = new SqlParameter("Account", account);
            var q = await this._dbContext.spMember
                .FromSql("EXECUTE dbo.spGetMember @Account", Account)
                .FirstOrDefaultAsync();
            // string sqlstr = string.Format ($"execute spGetMember '{account}'");
            // var q = await this._dbContext.spMember.FromSql (sqlstr).ToAsyncEnumerable ()
            //     .FirstOrDefault ();

            return q;
        }

        // 更新user的 Line token
        public async Task<spMember> SetUserToken (int counter, string token) {
            var _counter = new SqlParameter("counter", counter);
            var _token = new SqlParameter("token", token);
            var q = await this._dbContext.spMember
                .FromSql("EXECUTE dbo.spUpdateMemberLine @counter , @token ", _counter , _token)
                .FirstOrDefaultAsync();

            // string sqlstr = string.Format ($"execute spUpdateMemberLine {counter} , '{token}'");
            // var q = await this._dbContext.spMember.FromSql (sqlstr).ToAsyncEnumerable ()
            //     .FirstOrDefault ();

            return q;
        }

        // 更新user的mail
        public async Task<spMember> SetUserMail (int counter, string mail) {
            var _counter = new SqlParameter("counter", counter);
            var _mail = new SqlParameter("mail", mail);
            var q = await this._dbContext.spMember
                .FromSql("EXECUTE dbo.spUpdateMemberMail @counter , @mail ", _counter , _mail)
                .FirstOrDefaultAsync();
            // string sqlstr = string.Format ($"EXECUTE spUpdateMemberMail {counter} , '{mail}'");
            // var q = await this._dbContext.spMember.FromSql (sqlstr).FirstOrDefaultAsync ();

            return q;
        }
    }

}