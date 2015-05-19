using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SchoolWebsite.Models
{
    public class SchoolDb : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public SchoolDb() : base("name=SchoolDb")
        {
        }

        public System.Data.Entity.DbSet<SchoolWebsite.Models.Poll> Polls { get; set; }

        public System.Data.Entity.DbSet<SchoolWebsite.Models.Vote> Votes { get; set; }

        public System.Data.Entity.DbSet<SchoolWebsite.Models.Config> Configs { get; set; }

        public System.Data.Entity.DbSet<SchoolWebsite.Models.HousePoint> HousePoints { get; set; }
    
    }
}
