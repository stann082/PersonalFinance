using Microsoft.EntityFrameworkCore;
using PersonalFinance.Domain.Misc;

namespace PersonalFinance.Application.Models;

public class TransactionContext(DbContextOptions<TransactionContext> options) : DbContext(options)
{

    public DbSet<TransactionTable> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var connectionString = "Server=localhost;Port=5432;User Id=postgres;Password=gr3B%zPeFtkeD8bpdau^N4j5;Database=finances;";
        optionsBuilder.UseNpgsql(connectionString);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TransactionTable>(entity =>
        {
            entity.ToTable(Constants.TransactionsTableName);
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .HasColumnName(Constants.TransactionsTableIdColumn)
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.Amount).IsRequired().HasColumnName(Constants.TransactionsTableAmountColumn);
            entity.Property(e => e.Bank).IsRequired().HasColumnName(Constants.TransactionsTableBankColumn);
            entity.Property(e => e.Category).HasColumnName(Constants.TransactionsTableCategoryColumn);
            entity.Property(e => e.Date).IsRequired().HasColumnName(Constants.TransactionsTableDateColumn);
            entity.Property(e => e.Description).IsRequired().HasColumnName(Constants.TransactionsTableDescriptionColumn);
            entity.Property(e => e.Type).IsRequired().HasColumnName(Constants.TransactionsTableTypeColumn);
        });
    }
}
