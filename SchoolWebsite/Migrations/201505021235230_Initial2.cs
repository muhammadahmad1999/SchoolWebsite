namespace SchoolWebsite.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial2 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Configs", "Action", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Configs", "Action");
        }
    }
}
