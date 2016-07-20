namespace AliseaMiddleService.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Migrations.Builders;

    public partial class Inizio : DbMigration
    {
        public override void Up()
        {

            CreateTable(

                "dbo.Categories",
                c => new
                {
                  
                    Name = c.String(maxLength:100)
                }).PrimaryKey(c => c.Name);


            CreateTable(
           "dbo.Films",
           c => new
           {
               ID = c.Int(identity:true),
               Title = c.String(nullable:false, maxLength:100),
               Director = c.String(nullable:false),
               Length = c.Short(nullable:false),
               ReleaseDate = c.DateTime(nullable:false),
               InsertDate = c.DateTime(nullable:false),
               Actors = c.String(nullable:false, maxLength:500),
               Category = c.String(maxLength:100, nullable:false),
               FilmDescription = c.String(nullable:false),
               ImagePath = c.String(nullable:false),
               ThemePath = c.String(nullable: false),
               TorrentLink = c.String(nullable: false),
               Thumbs = c.Short(defaultValue:0),
               WeeklyThumbs = c.Short(defaultValue:0),
               Visualizations = c.Short(defaultValue:0),


           })
           .PrimaryKey(t => t.ID);
        }



        public override void Down()
        {
        }
    }
}
