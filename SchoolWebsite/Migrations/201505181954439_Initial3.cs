namespace SchoolWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.HousePoints",
                c => new
                    {
                        HousePointID = c.Int(nullable: false, identity: true),
                        DateAwarded = c.DateTime(nullable: false),
                        AwardingTeacher = c.String(),
                        Reason = c.String(),
                    })
                .PrimaryKey(t => t.HousePointID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.HousePoints");
        }
    }
}
