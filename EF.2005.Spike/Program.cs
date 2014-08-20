using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EF._2005.Spike
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new DropCreate2008());
            using (var context = new SpikeContext())
            {
                context.RateCards.Add(new RateCard {Name = "Test1", CreatedAt = DateTime.Now});
                context.SaveChanges();
                Console.WriteLine("Changes saved");
            }
        }
    }

    public class SpikeContext:DbContext
    {
        public DbSet<RateCard> RateCards { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Properties<DateTime>()
                .Configure(c => c.HasColumnType("datetime2"));
        }
    }

    public class RateCard
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    class DropCreate2008 : DropCreateDatabaseAlways<SpikeContext>
    {
        public override void InitializeDatabase(SpikeContext context)
        {
            var sqlConnectionString = context.Database.Connection.ConnectionString;
            var databaseName = context.Database.Connection.Database;

            base.InitializeDatabase(context);
 
            // The EF context connection can't be used to ALTER the new database because it 
            // operates inside a transaction which will cause an SQL error.
            // Instead we will create a new SQL connection.
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                // Execute the alter database command and enable allow snapshot isolation.
                string sqlCommandText = string.Format("ALTER DATABASE {0} SET COMPATIBILITY_LEVEL = 100", databaseName);
                var sqlCommand = new SqlCommand(sqlCommandText, connection);
                sqlCommand.ExecuteNonQuery();
            }
        }
    }

}
