namespace SchoolWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.HousePoints", "AwardedTo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.HousePoints", "AwardedTo");
        }
    }
}
