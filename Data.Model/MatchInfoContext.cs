using System.Data.Entity;

namespace Data.SqlClient
{
    public class MatchInfoContext : DbContext
    {
        static MatchInfoContext()
        {
            Database.SetInitializer(new NullDatabaseInitializer<MatchInfoContext>());
        }

        public MatchInfoContext()
            : base("DataConnection")
        {
        }

        public IDbSet<MatchInfoItem> Items { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new MatchInfoItemMap());

            base.OnModelCreating(modelBuilder);
        }

    }
}
