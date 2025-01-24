using MusicBase.Models;

namespace MusicBase.Database
{
	public class DbInitializer
	{
		public static void Initialize(DbConnection context)
		{
			context.Database.EnsureCreated();

			if (context.Musics.Any())
				return;

            List<Genre> genres = new()
            {
                new Genre {Name = "pop"},
                new Genre {Name = "disco"},
                new Genre {Name = "klasyczna"},
                new Genre {Name = "hiphop"},
                new Genre {Name = "rap"}
            };
            context.Genres.AddRange(genres);

            List<Music> musics = new()
			{
				new Music {Author = "Michael Jackson", Genre = genres.ElementAt(0), GenreId = 1, Length = new DateTime(1,1,1, 0,5,57), Name= "Thriller", PublishedDate = new DateTime(1983,11,1), Publisher="Epic Records"},
			};
			context.Musics.AddRange(musics);

			context.SaveChanges();
		}
	}
}