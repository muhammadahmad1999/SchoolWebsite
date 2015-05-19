namespace SchoolWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Configs",
                c => new
                    {
                        ConfigID = c.Int(nullable: false, identity: true),
                        SystemID = c.String(),
                        RolesAllowed = c.String(),
                    })
                .PrimaryKey(t => t.ConfigID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Configs");
        }
    }
}
