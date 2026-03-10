using Microsoft.EntityFrameworkCore;

namespace DSharpBot.R691;

public class R691Context : DbContext
{
	public DbSet<Banned> Banned { get; set; }

	public R691Context() : base() { }

	public R691Context(DbContextOptions<R691Context> options) : base(options) { }

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		=> optionsBuilder.UseSqlite("Data Source=R691.db"); 
}

public class Banned
{
	required public ulong Id { get; set; }
	required public DateTime Until { get; set; }
}