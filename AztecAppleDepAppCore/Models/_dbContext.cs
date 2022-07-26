using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace AztecAppleDepApp.Models
{
    [Table("DepCustomers")]
    public class _dbContext : DbContext
    {
        public _dbContext() : base(nameOrConnectionString: "name=AztecAppleDep") { }
        public DbSet<DepCustomer> DepCustomers { get; set; }
        public DbSet<DepService> DepServices { get; set; }
        public DbSet<RequestBodyEf> RequestBodiesEf { get; set; }
        public DbSet<RequestContextEf> RequestContextsEf { get; set; }
        public DbSet<OrderEf> OrdersEf { get; set; }
        public DbSet<DeliveryEf> DeliveriesEf { get; set; }
        public DbSet<DeviceEf> DevicesEf { get; set; }
        public DbSet<JsonLog> JsonLogs { get; set; }
        public DbSet<TransactionHistory> TransactionHistorys { get; set; }
    }
}