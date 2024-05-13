namespace UserAuth.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddHouseTable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Houses",
                c => new
                    {
                        id = c.Int(nullable: false, identity: true),
                        userId = c.Int(nullable: false),
                        name = c.String(maxLength: 100),
                        city = c.Int(nullable: false),
                        road = c.String(),
                        lane = c.String(),
                        alley = c.String(),
                        number = c.String(),
                        floor = c.String(),
                        floorTotal = c.String(),
                        Type = c.Int(nullable: false),
                        ping = c.String(),
                        roomNumbers = c.Int(),
                        livingRoomNumbers = c.Int(),
                        bathRoomNumbers = c.Int(),
                        balconyNumbers = c.Int(),
                        parkingSpaceNumbers = c.Int(),
                        isRentSubsidy = c.Boolean(nullable: false),
                        isPetAllowed = c.Boolean(nullable: false),
                        isCookAllowed = c.Boolean(nullable: false),
                        isSTRAllowed = c.Boolean(nullable: false),
                        isNearByDepartmentStore = c.Boolean(nullable: false),
                        isNearBySchool = c.Boolean(nullable: false),
                        isNearByMorningMarket = c.Boolean(nullable: false),
                        isNearByNightMarket = c.Boolean(nullable: false),
                        isNearByConvenientStore = c.Boolean(nullable: false),
                        isNearByPark = c.Boolean(nullable: false),
                        hasGarbageDisposal = c.Boolean(nullable: false),
                        hasWindowInBathroom = c.Boolean(nullable: false),
                        hasElevator = c.Boolean(nullable: false),
                        isNearMRT = c.Boolean(nullable: false),
                        kmAwayMRT = c.Int(),
                        isNearLRT = c.Boolean(nullable: false),
                        kmAwayLRT = c.Int(),
                        isNearBusStation = c.Boolean(nullable: false),
                        kmAwayBusStation = c.Int(),
                        isNearHSR = c.Boolean(nullable: false),
                        kmAwayHSR = c.Int(),
                        isNearTrainStation = c.Boolean(nullable: false),
                        kmAwayTrainStation = c.Int(),
                        hasAirConditioner = c.Boolean(nullable: false),
                        hasWashingMachine = c.Boolean(nullable: false),
                        hasRefrigerator = c.Boolean(nullable: false),
                        hasCloset = c.Boolean(nullable: false),
                        hasTableAndChair = c.Boolean(nullable: false),
                        hasWaterHeater = c.Boolean(nullable: false),
                        hasInternet = c.Boolean(nullable: false),
                        hasBed = c.Boolean(nullable: false),
                        hasTV = c.Boolean(nullable: false),
                        paymentMethodOfWaterBill = c.Int(nullable: false),
                        waterBillPerMonth = c.Int(),
                        electricBill = c.Int(nullable: false),
                        paymentMethodOfElectricBill = c.Int(nullable: false),
                        paymentMethodOfManagementFee = c.Int(nullable: false),
                        managementFeePerMonth = c.Int(),
                        rent = c.Int(),
                        securityDeposit = c.Int(nullable: false),
                        description = c.String(),
                        hasTenantRestrictions = c.Boolean(nullable: false),
                        genderRestriction = c.Int(nullable: false),
                        status = c.Int(nullable: false),
                        CreateAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.id)
                .ForeignKey("dbo.Users", t => t.userId, cascadeDelete: true)
                .Index(t => t.userId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Houses", "userId", "dbo.Users");
            DropIndex("dbo.Houses", new[] { "userId" });
            DropTable("dbo.Houses");
        }
    }
}
