namespace UserAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        firstName = c.String(nullable: false, maxLength: 10),
                        lastName = c.String(nullable: false, maxLength: 10),
                        email = c.String(),
                        salt = c.String(),
                        password = c.String(nullable: false),
                        telphone = c.String(nullable: false, maxLength: 10),
                        gender = c.Int(nullable: false),
                        job = c.Int(nullable: false),
                        photo = c.String(),
                        role = c.Int(nullable: false),
                        averageRating = c.Single(nullable: false),
                        ratingCount = c.Int(nullable: false),
                        CreateAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Users");
        }
    }
}
