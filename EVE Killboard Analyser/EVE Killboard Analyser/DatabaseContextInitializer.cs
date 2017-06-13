using System.Data.Entity;
using System.Data.Entity.Migrations;
using EVE_Killboard_Analyser.Helper;
using EVE_Killboard_Analyser.Migrations;

namespace EVE_Killboard_Analyser
{

    internal class DatabaseContextInitializer : IDatabaseInitializer<DatabaseContext>
    {
        //private readonly DropCreateDatabaseAlways<DatabaseContext> _create = new DropCreateDatabaseAlways<DatabaseContext>();
        private readonly CreateDatabaseIfNotExists<DatabaseContext> _create = new CreateDatabaseIfNotExists<DatabaseContext>();

        private readonly MigrateDatabaseToLatestVersion<DatabaseContext, Configuration> _migrate =
            new MigrateDatabaseToLatestVersion<DatabaseContext, Configuration>();
        
        protected  void Seed(DatabaseContext context)
        {
         //   context.ObjectContext.ExecuteStoreCommand("create nonclustered index nonclustered_killtime on dbo.Kills (KillTime)");
        }

        public void InitializeDatabase(DatabaseContext context)
        {
          //_create.InitializeDatabase(context);// WARNUNG DROP DATABASE AKTIV
            
           // Seed(context);
            //_migrate.InitializeDatabase(context);
        }
    }
}