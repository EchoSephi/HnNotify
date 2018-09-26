using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace HnNotify.Data {
    public class NotifyItem {
        [Key]
        public int Counter { get; set; }
        public string Name { get; set; }
        public int deleted { get; set; }
    }

    public interface INotifyItemService {
        Task<List<NotifyItem>> GetNotifyItem ();
    }

    public class NotifyItemService : INotifyItemService {
        private HNContext _dbContext;
        public NotifyItemService (HNContext dbContext) {
            this._dbContext = dbContext;
        }

        // 取得訂閱服務的基本資料
        public async Task<List<NotifyItem>> GetNotifyItem () {
            var q = await (from p in this._dbContext.NotifyItem where p.deleted == 0 
            orderby p.Counter select p).ToAsyncEnumerable ().ToList ();

            return q;
        }
    }
}