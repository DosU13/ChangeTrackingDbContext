using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ChangeTrackingDbContext
{
    public class ChangeTrackingDbContext : DbContext
    {
        public ChangeTrackingDbContext(string nameOrConnectionString) : base(nameOrConnectionString) { }

        public virtual DbSet<ChangeTrack> ChangeTracks { get; set; }

        public override int SaveChanges()
        {
            var changedEntries = ChangeTracker.Entries().
                Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Deleted ||
                           e.State == EntityState.Modified);
            foreach (var entity in changedEntries)
            {
                if (IsChangeTrackable(entity.Entity))
                {
                    throw new Exception("Changed Table is marked ChangeTrackable. Use SaveChanges(username,idAddress) instead");
                }
            }
            return base.SaveChanges();
        }

        private bool firstTime = true;
        public int SaveChanges(string username, string ipAddress)
        {
            if (firstTime)
            {
                CreateTrackerTableIfNotExists();
                firstTime = false;
            }

            var changedEntries = ChangeTracker.Entries().
                Where(e => e.State == EntityState.Added ||
                           e.State == EntityState.Deleted ||
                           e.State == EntityState.Modified);
            foreach (var entity in changedEntries)
            {
                if (!IsChangeTrackable(entity.Entity)) continue;

                Dictionary<string, string> oldValues = null;
                if (entity.State != EntityState.Added)
                {
                    oldValues = new Dictionary<string, string>();
                    foreach (var propertyName in entity.OriginalValues.PropertyNames)
                    {
                        var value = entity.OriginalValues.GetValue<object>(propertyName)?.ToString() ?? null;
                        oldValues.Add(propertyName, value);
                    }
                }
                Dictionary<string, string> newValues = null;
                if (entity.State != EntityState.Deleted)
                {
                    newValues = new Dictionary<string, string>();
                    foreach (var propertyName in entity.CurrentValues.PropertyNames)
                    {
                        var value = entity.CurrentValues.GetValue<object>(propertyName)?.ToString() ?? null;
                        newValues.Add(propertyName, value);
                    }
                }

                var insertTrack = new ChangeTrack()
                {
                    TableName = GetTableName(entity.Entity),
                    Username = username,
                    IpAddress = ipAddress,
                    ChangeDate = DateTime.Now,
                    Operation = (entity.State == EntityState.Added) ? OperationType.Insert :
                                (entity.State == EntityState.Deleted) ? OperationType.Delete :
                                OperationType.Update,
                    OldValues = oldValues,
                    NewValues = newValues
                };

                ChangeTracks.Add(insertTrack);
            }

            return base.SaveChanges();
        }

        private void CreateTrackerTableIfNotExists()
        {
            Database.ExecuteSqlCommand(@"
            IF OBJECT_ID(N'dbo.ChangeTrack', N'U') IS NULL
            CREATE TABLE [dbo].[ChangeTrack](
                [Id] [int] IDENTITY(1,1) NOT NULL,
                [table_name] [nvarchar](255) NOT NULL,
                [username] [nvarchar](255) NOT NULL,
                [ipaddress] [nvarchar](255) NOT NULL,
                [change_date] [datetime] NOT NULL,
                [operation] [int] NOT NULL,
                [old_values] [nvarchar](max) NULL,
                [new_values] [nvarchar](max) NULL
            )");
        }

        private bool IsChangeTrackable(object _object)
        {
            return Attribute.IsDefined(_object.GetType(), typeof(ChangeTrackableAttribute));
        }

        private string GetTableName(object _object)
        {
            Type type = _object.GetType();
            if (Attribute.IsDefined(type, typeof(TableAttribute)))
            {
                var attr = (TableAttribute)Attribute.GetCustomAttribute(type, typeof(TableAttribute));
                return attr.Name;
            }
            else return type.Name;
        }
    }
}
