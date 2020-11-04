using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplicationCNPJ.Models;

namespace WebApplicationCNPJ.Context
{
    public class CNPJDataContext : DbContext
    {
        public DbSet<CNPJData> CNPJData { get; set; }

        public CNPJDataContext( DbContextOptions<CNPJDataContext> options ) : base( options )
        {
        }        

        protected override void OnModelCreating( ModelBuilder builder )
        {
            builder.Entity<CNPJData>().HasKey( m => m.Id );
            base.OnModelCreating( builder );
        }
    }
}
