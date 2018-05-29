using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;

namespace FussballTippApp.Models
{
    public class TippSpielContext : DbContext
    {
        public TippSpielContext()
            : base("DataConnection")
        {
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<TippSpielContext>(null); 

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<TippMatchModel> TippMatchList { get; set; }
    }

    [Table("TippMatch")]
    public class TippMatchModel
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int MatchId { get; set; }
        public string User { get; set; }
        public double? MyOdds { get; set; }
        public int? MyTip { get; set; }
        public double? MyAmount { get; set; }
        public DateTime LastUpdated { get; set; }
    }

}