using Microsoft.EntityFrameworkCore;
using MusicBase.Models;

namespace MusicBase.Database
{
	public class DbConnection : DbContext
	{
		public DbConnection(DbContextOptions<DbConnection> options) : base(options)
		{ }
		public DbSet<Music> Musics { get; set; }
	}
}
