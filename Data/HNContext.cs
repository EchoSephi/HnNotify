using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HnNotify.Data {
    public class HNContext : DbContext {

        public HNContext (DbContextOptions<HNContext> options) : base (options) { }
        protected override void OnModelCreating (ModelBuilder modelBuilder) {}

        public virtual DbSet<spMember> spMember { get; set; }
        public virtual DbSet<Notify> Notify { get; set; }
        public virtual DbSet<NotifyItem> NotifyItem { get; set; }

    }

}