﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Domain
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class WorklogDBEntities : DbContext
    {
        public WorklogDBEntities()
            : base("name=WorklogDBEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Holiday> Holidays { get; set; }
        public virtual DbSet<LogEntry> LogEntries { get; set; }
        public virtual DbSet<Leave> Leaves { get; set; }
    }
}
