using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MusicBase.Database;
using MusicBase.Models;

namespace MusicBase.Controllers
{
	public class MusicsController : Controller
	{
		private readonly DbConnection _context;

		public MusicsController(DbConnection context)
		{
			_context = context;
		}

		// GET: Musics
		public async Task<IActionResult> Index(Dictionary<string, string> search)
		{
			List<Music> tracks = _context.Musics.Include(music => music.Genre).ToList();

			foreach (KeyValuePair<string, string> pair in search)
			{
				DateTime date;
				if (pair.Value == null) continue;
				switch (pair.Key)
				{
					case "dataMin":
						date = DateTime.Parse(pair.Value);
						tracks = tracks.Where(m => m.PublishedDate >= date).ToList();
						break;
					case "dataMax":
						date = DateTime.Parse(pair.Value);
						tracks = tracks.Where(m => m.PublishedDate <= date).ToList();
						break;
					case "lengthMin":
						date = DateTime.Parse(pair.Value);
						tracks = tracks.Where(m => m.Length.TimeOfDay.TotalSeconds >= date.TimeOfDay.TotalSeconds)
							.ToList();
						break;
					case "lengthMax":
						date = DateTime.Parse(pair.Value);
						tracks = tracks.Where(m => m.Length.TimeOfDay.TotalSeconds <= date.TimeOfDay.TotalSeconds)
							.ToList();
						break;
					case "genre":
						tracks = tracks.Where(m => m.GenreId.ToString() == pair.Value).ToList();
						break;
					case "title":
						tracks = tracks.Where(m => m.Name.Contains(pair.Value)).ToList();
						break;
					case "author":
						tracks = tracks.Where(m => m.Author.Contains(pair.Value)).ToList();
						break;
					case "publisher":
						tracks = tracks.Where(m => m.Publisher.Contains(pair.Value)).ToList();
						break;
				}
			}

			ViewBag.Genre = CreateGenresOptions();
			return View(tracks);
		}

		// GET: Musics/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Musics == null)
				return NotFound();

			Music music = await _context.Musics.FirstOrDefaultAsync(m => m.MusicId == id);
			if (music == null)
				return NotFound();

			music.Genre = _context.Genres.ToList().Find(g => g.GenreId == music.GenreId);
			
			ConvertTrackAndCover(id.Value);
			return View(music);
		}

		// GET: Musics/Create
		public IActionResult Create()
		{
			ViewBag.Genre = CreateGenresOptions();
			return View();
		}

		// POST: Musics/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(
			[Bind("MusicId,Name,Author,PublishedDate,Length,Publisher,GenreId,Cover")]
			Music music)
		{
			music.Genre = _context.Genres.ToList().Find(g => g.GenreId == music.GenreId);
			
			if (!ModelState.IsValid)
			{
				if (ModelState.ContainsKey("Genre") && ModelState.ErrorCount == 1)
					ModelState.Clear();
				else
					return View(music);
			}
			
			_context.Musics.Add(music);
			await _context.SaveChangesAsync();
			int id = music.MusicId;
			await ProcessCoverAndTrackForm(id, true);
			return RedirectToAction(nameof(Details), new { id });
		}

		// GET: Musics/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Musics == null)
				return NotFound();

			Music music = await _context.Musics.FindAsync(id);
			if (music == null)
				return NotFound();

			ViewBag.Genre = CreateGenresOptions();
			ConvertTrackAndCover(id.Value);

			return View(music);
		}

		// POST: Musics/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id,
			[Bind("MusicId,Name,Author,PublishedDate,Length,Publisher,GenreId")]
			Music music)
		{
			if (id != music.MusicId)
				return NotFound();
			music.Genre = _context.Genres.ToList().Find(g => g.GenreId == music.GenreId);
			
			if (!ModelState.IsValid)
			{
				if (ModelState.ContainsKey("Genre") && ModelState.ErrorCount == 1)
					ModelState.Clear();
				else
					return View(music);
			}
			
			await ProcessCoverAndTrackForm(id);
			try
			{
				_context.Update(music);
				await _context.SaveChangesAsync();
			}
			catch (DbUpdateConcurrencyException)
			{
				if (!MusicExists(music.MusicId))
					return NotFound();
				throw;
			}

			return RedirectToAction(nameof(Details), new { id });
		}

		// GET: Musics/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Musics == null)
				return NotFound();

			Music music = await _context.Musics.FirstOrDefaultAsync(m => m.MusicId == id);
			music.Genre = _context.Genres.ToList().Find(g => g.GenreId == music.GenreId);

			if (music == null)
				return NotFound();
			return View(music);
		}

		// POST: Musics/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Musics == null)
				return Problem("Entity set 'DbConnection.Musics'  is null.");

			Music music = await _context.Musics.FindAsync(id);
			if (music != null)
				_context.Musics.Remove(music);

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool MusicExists(int id)
		{
			return _context.Musics.Any(e => e.MusicId == id);
		}

		private async Task ProcessCoverAndTrackForm(int id, bool create = false)
		{
			Dictionary<string, IFormFile?> files = new();
			Dictionary<string, string?> extensions = new()
			{
				["cover"] = "png",
				["track"] = "mp3",
			};

			IFormFile? cover = null;
			IFormFile? track = null;
			foreach (IFormFile file in Request.Form.Files)
			{
				if (file.ContentType.Contains("image"))
					files.Add("cover", file);
				if (file.ContentType.Contains("audio"))
					files.Add("track", file);
			}

			string path = "temp/upload/" + id;

			if (create) Directory.CreateDirectory(path);

			foreach (KeyValuePair<string, IFormFile> file in files)
			{
				await using var stream =
					System.IO.File.Open($"{path}/{file.Key}_{id}.{extensions[file.Key]}", FileMode.Create);
				await file.Value.CopyToAsync(stream);
			}
		}

		private void ConvertTrackAndCover(int id)
		{
			try
			{
				byte[] filebytes =
					System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() +
					                            $"\\temp\\upload\\{id}\\cover_{id}.png");
				string base64 = Convert.ToBase64String(filebytes);
				ViewData["Cover"] = "data:image / jpeg; base64," + base64;

				filebytes = System.IO.File.ReadAllBytes(Directory.GetCurrentDirectory() +
				                                        $"\\temp\\upload\\{id}\\track_{id}.mp3");
				base64 = Convert.ToBase64String(filebytes);
				ViewData["Track"] = "data:audio / mp3; base64," + base64;
			}
			catch (Exception)
			{
			}
		}

		[HttpGet]
		public int GetRandomTrack()
		{
			Random random = new();
			int max = _context.Musics.Max(m => m.MusicId);
			Music music = null;
			int id = -1;
			while (music == null)
			{
				id = random.Next(1, max + 1);
				music = _context.Musics.FirstOrDefault(m => m.MusicId == id);
			}

			return id;
		}

		private List<SelectListItem> CreateGenresOptions()
		{
			return new List<SelectListItem>(_context.Genres.Select(g => new SelectListItem
			{
				Value = g.GenreId.ToString(),
				Text = g.Name
			}).ToList());
		}
	}
}