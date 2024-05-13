namespace UserAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateHouseTableColumn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Houses", "district", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Houses", "district");
        }
    }
}
