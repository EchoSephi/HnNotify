using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
namespace HnNotify.Data {
    public class Notify {
        [Key]
        public Guid Guid { get; set; }
        public int NotifyItem_Counter { get; set; }
        public int Member_Counter { get; set; }
        public bool Line { get; set; }
        public bool Mail { get; set; }
    }

    public interface INotifyService {
        Task<List<dtoNotify>> GetNotify (int MemberCounter);
        Task SetNotify (List<dtoNotify> dto);
        Task<dtoShowLineNotifyS> ShowNotify (int MemberCounter);
    }

    public class NotifyService : INotifyService {
        private HNContext _dbContext;
        public NotifyService (HNContext dbContext) {
            this._dbContext = dbContext;
        }

        // 取得user訂閱的服務
        public async Task<List<dtoNotify>> GetNotify (int MemberCounter) {
            var q = await (from p in this._dbContext.Notify join p2 in this._dbContext.NotifyItem on p.NotifyItem_Counter equals p2.Counter where p.Member_Counter == MemberCounter orderby p.NotifyItem_Counter select new dtoNotify {
                Member_Counter = p.Member_Counter,
                    NotifyItem_Counter = p.NotifyItem_Counter,
                    NotifyItem_Name = p2.Name,
                    Line = p.Line,
                    Mail = p.Mail
            }).ToAsyncEnumerable ().ToList ();

            return q;
        }

        // 更新(或第一次新增)user訂閱的服務
        public async Task SetNotify (List<dtoNotify> dto) {
            var Member_Counter = dto.FirstOrDefault ().Member_Counter;
            var qcnt = (from p in this._dbContext.Notify where p.Member_Counter == Member_Counter select p).AsEnumerable ().AsQueryable ();

            if (qcnt.FirstOrDefault () != null) {
                // 更新
                foreach (var p in dto) {
                    var q = (from pp in this._dbContext.Notify where pp.Member_Counter == p.Member_Counter &&
                        pp.NotifyItem_Counter == p.NotifyItem_Counter select pp).AsEnumerable ().FirstOrDefault ();

                    var notify = await this._dbContext.Notify.FindAsync (q.Guid);
                    this._dbContext.Notify.Attach (notify);
                    notify.Line = p.Line;
                    notify.Mail = p.Mail;
                    await this._dbContext.SaveChangesAsync ();
                }
            } else {
                // 第一次新增
                foreach (var p in dto) {
                    var notify = new Notify ();
                    notify.Guid = Guid.NewGuid ();
                    notify.NotifyItem_Counter = p.NotifyItem_Counter;
                    notify.Member_Counter = p.Member_Counter;
                    notify.Line = p.Line;
                    notify.Mail = p.Mail;
                    await this._dbContext.Notify.AddAsync (notify);
                    await this._dbContext.SaveChangesAsync ();
                }
            }
        }

        // 顯示使用者訂閱的項目
        public async Task<dtoShowLineNotifyS> ShowNotify (int MemberCounter) {
            var line = await (from p1 in this._dbContext.Notify join p2 in this._dbContext.NotifyItem on p1.NotifyItem_Counter equals p2.Counter where p1.Member_Counter == MemberCounter &&
                p1.Line == true && p2.deleted == 0 orderby p2.Counter select new dtoShowLineNotify {
                    Type = "Line",
                        Name = p2.Name
                }).ToAsyncEnumerable ().ToList ();

            var mail = await (from p1 in this._dbContext.Notify join p2 in this._dbContext.NotifyItem on p1.NotifyItem_Counter equals p2.Counter where p1.Member_Counter == MemberCounter &&
                p1.Mail == true && p2.deleted == 0 orderby p2.Counter select new dtoShowLineNotify {
                    Type = "Line",
                        Name = p2.Name
                }).ToAsyncEnumerable ().ToList ();

            var q = new dtoShowLineNotifyS ();
            q.dtoLine = line;
            q.dtoMail = mail;

            return q;
        }
    }

    // data to model 物件,用來串前後端資料用的
    public class dtoNotify {
        public int Member_Counter { get; set; }
        public int NotifyItem_Counter { get; set; }
        public string NotifyItem_Name { get; set; }
        public bool Line { get; set; }
        public bool Mail { get; set; }
    }

    public class dtoNotifyS {
        public string MAIL { get; set; }
        public List<dtoNotify> dtoNotify { get; set; }
        public string errMsg { get; set; }
    }

    public class dtoShowLineNotify {
        public string Type { get; set; }
        public string Name { get; set; }
    }

    public class dtoShowLineNotifyS {
        public string Name { get; set; }
        public List<dtoShowLineNotify> dtoLine { get; set; }
        public List<dtoShowLineNotify> dtoMail { get; set; }
    }
}