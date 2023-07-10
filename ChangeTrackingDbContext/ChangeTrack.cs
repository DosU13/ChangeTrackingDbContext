using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace ChangeTrackingDbContext
{
    [Table("ChangeTrack")]
    public class ChangeTrack
    {
        [Key]
        public int Id { get; set; }
        [Column("table_name")]
        public string TableName { get; set; }
        [Column("user_id")]
        public string UserId { get; set; }
        [Column("ipaddress")]
        public string IpAddress { get; set; }
        [Column("operation")]
        public OperationType Operation { get; set; }
        [Column("change_date")]
        public DateTime ChangeDate { get; set; }
        [Column("old_values")]
        public string OldValuesJson { get; set; }
        [Column("new_values")]
        public string NewValuesJson { get; set; }

        [NotMapped]
        public Dictionary<string, string> OldValues
        {
            get => OldValuesJson != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(OldValuesJson ?? "") : null;
            set
            {
                if (value != null)
                {
                    OldValuesJson = JsonConvert.SerializeObject(value);
                }
            }
        }

        [NotMapped]
        public Dictionary<string, string> NewValues
        {
            get => NewValuesJson != null ? JsonConvert.DeserializeObject<Dictionary<string, string>>(NewValuesJson ?? "") : null;
            set
            {
                if (value != null)
                {
                    NewValuesJson = JsonConvert.SerializeObject(value);
                }
            }
        }
    }
}
