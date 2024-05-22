using System;
using System.Data.Entity;
using System.Linq;

namespace UserAuth.Models
{
    public class DBModel : DbContext
    {
        // 您的內容已設定為使用應用程式組態檔 (App.config 或 Web.config)
        // 中的 'DBModel' 連接字串。根據預設，這個連接字串的目標是
        // 您的 LocalDb 執行個體上的 'UserAuth.Models.DBModel' 資料庫。
        //
        // 如果您的目標是其他資料庫和 (或) 提供者，請修改
        // 應用程式組態檔中的 'DBModel' 連接字串。
        public DBModel()
            : base("name=DBModel")
        {
        }

        // 針對您要包含在模型中的每種實體類型新增 DbSet。如需有關設定和使用
        // Code First 模型的詳細資訊，請參閱 http://go.microsoft.com/fwlink/?LinkId=390109。

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
        public virtual DbSet<User> UserEntities { get; set; }

        public virtual DbSet<House> HouseEntities { get; set; }
        public virtual DbSet<HouseImg> HouseImgsEntities { get; set; }

        public virtual DbSet<Appointment> AppointmentsEntities { get; set; }
        public virtual DbSet<Order> OrdersEntities { get; set; }
        public virtual DbSet<OrderRating> OrdersRatingEntities { get; set; }
        public virtual DbSet<ReplyRating> ReplyRatingEntities { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // Fluent API 配置
            // 當刪除House資料時，不要自動刪除相關聯的userIdFK（User表格中對應的資料）
            modelBuilder.Entity<House>()
                .HasRequired(c => c.userIdFK)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<OrderRating>().HasRequired(c => c.User).WithMany().WillCascadeOnDelete(false);
            modelBuilder.Entity<ReplyRating>().HasRequired(c => c.User).WithMany().WillCascadeOnDelete(false);
        }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}