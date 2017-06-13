namespace EVE_Killboard_Analyser.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PositionToVictim : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Victims", "X", c => c.Double(nullable: true));
            AddColumn("dbo.Victims", "Y", c => c.Double(nullable: true));
            AddColumn("dbo.Victims", "Z", c => c.Double(nullable: true));
            DropColumn("dbo.Kills", "Position_X");
            DropColumn("dbo.Kills", "Position_Y");
            DropColumn("dbo.Kills", "Position_Z");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Kills", "Position_Z", c => c.Double(nullable: true));
            AddColumn("dbo.Kills", "Position_Y", c => c.Double(nullable: true));
            AddColumn("dbo.Kills", "Position_X", c => c.Double(nullable: true));
            DropColumn("dbo.Victims", "Z");
            DropColumn("dbo.Victims", "Y");
            DropColumn("dbo.Victims", "X");
        }
    }
}
