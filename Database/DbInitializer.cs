using MusicBase.Models;

namespace MusicBase.Database
{
	public class DbInitializer
	{
		public static void Initialize(DbConnection context)
		{
			/*context.Database.EnsureDeleted();*/
			context.Database.EnsureCreated();

			if (context.Musics.Any())
				return;

			List<Music> musics = new()
			{
				new Music {Author = "Michael Jackson", Genre = Genre.pop, Length = new DateTime(1,1,1, 0,5,57), Name= "Thriller", PublishedDate = new DateTime(1983,11,1), Publisher="Epic Records"},
			};
			
			context.Musics.AddRange(musics);
			context.SaveChanges();
		}
	}
}