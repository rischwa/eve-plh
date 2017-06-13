using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using EVE_Killboard_Analyser.Controllers;
using EVE_Killboard_Analyser.Models;
using PLHLib;

namespace EVE_Killboard_Analyser.Helper
{
    public class BlockedIp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Ip { get; set; }
    }

    public class DatabaseContext : DbContext 
    {
        public ObjectContext ObjectContext{get { return ((IObjectContextAdapter) this).ObjectContext; }}
        public DatabaseContext():base("kb_analysis"){}
        public DbSet<Kill> Kills { get; set; }
        public DbSet<Victim> Victims { get; set; }
        public DbSet<Attacker> Attackers { get; set; }
        public DbSet<Item> Items { get; set; }
        public DbSet<CharacterTag> SpecialTags { get; set; }

        //public DbSet<KillboardRequest> Requests { get; set; }
        //public DbSet<CharacterV1DataEntry> AnalysisResults { get; set; }

        public DbSet<MessageOfTheDay> MotDs { get; set; }

        public DbSet<BlockedIp> BlockedIps { get; set; }

        public IList<T> ExecuteSqlQuery<T>(string command, params SqlParameter[] parameters)
        {
             using (var result = ObjectContext.ExecuteStoreQuery<T>(command, parameters))
             {
                 return result.ToList();
             }
        }

        public void ExecuteSqlCommand(string command, params SqlParameter[] parameters)
        {
            ObjectContext.ExecuteStoreCommand(command, parameters);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<CharacterStatistics>().HasMany(c => c.Groups).WithRequired().WillCascadeOnDelete(true);
            //modelBuilder.Entity<CharacterV1DataEntry>().HasRequired(c => c.Statistics).WithRequiredPrincipal().WillCascadeOnDelete(true);
            //modelBuilder.Entity<CharacterV1DataEntry>().HasMany(c=>c.Tags).WithRequired().WillCascadeOnDelete(true);
            //modelBuilder.Entity<CharacterV1DataEntry>().HasMany(c => c.FavouriteShips).WithRequired().WillCascadeOnDelete(true);
            
            //modelBuilder.Entity<Kill>().HasMany(k=>k.Attackers).WithRequired().WillCascadeOnDelete(true);
            //modelBuilder.Entity<Kill>().HasRequired(k=>k.Victim).WithRequiredPrincipal().WillCascadeOnDelete(true);
            //modelBuilder.Entity<Kill>().HasMany(k => k.Items).WithRequired().WillCascadeOnDelete(true);
            //modelBuilder.Entity<Victim>().HasKey(v => v.Id);
            //modelBuilder.Entity<Attacker>().HasKey(v => v.Id);
            //modelBuilder.Entity<Item>().HasKey(v => v.Id);
        }
    }
}