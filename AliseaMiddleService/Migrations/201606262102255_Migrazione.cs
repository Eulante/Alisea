namespace AliseaMiddleService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Migrazione : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Films", "Thumbs", c => c.Short(nullable: false));
            AlterColumn("dbo.Films", "WeeklyThumbs", c => c.Short(nullable: false));
            AlterColumn("dbo.Films", "Visualizations", c => c.Short(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Films", "Visualizations", c => c.Int(nullable: false));
            AlterColumn("dbo.Films", "WeeklyThumbs", c => c.Int(nullable: false));
            AlterColumn("dbo.Films", "Thumbs", c => c.Int(nullable: false));
        }
    }
}
