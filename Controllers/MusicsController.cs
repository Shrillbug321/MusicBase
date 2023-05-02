using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MusicBase.Database;
using MusicBase.Models;
using static MusicBase.Controllers.BlobController;

namespace MusicBase.Controllers
{
	public class MusicsController : Controller
	{
		private readonly DbConnection _context;
		BlobServiceClient blobServiceClient;
		BlobClient blobClient;
		BlobContainerClient coversClient, tracksClient;
		private const string magazineAccountName = "projektkm";
		private const string connectionKey = "g7zeQcFGB1Msh96/m2dwdqOkpFSXjal8rOaTnHx8UomLN1nGxxzLHeZ/LZDJ5Qn+QVYbO/m0kNII+ASt+rTN9A==";

		public MusicsController(DbConnection context)
		{
			_context = context;
			GetClients();
		}

		// GET: Musics
		public async Task<IActionResult> Index(Dictionary<string, string> search)
		{
			List<Music> tracks = _context.Musics.ToList();

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
						tracks = tracks.Where(m => m.Length.TimeOfDay.TotalSeconds >= date.TimeOfDay.TotalSeconds).ToList();
						break;
					case "lengthMax":
						date = DateTime.Parse(pair.Value);
						tracks = tracks.Where(m => m.Length.TimeOfDay.TotalSeconds <= date.TimeOfDay.TotalSeconds).ToList();
						break;
					case "genre":
						tracks = tracks.Where(m => m.Genre.ToString() == pair.Value).ToList();
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
			ViewBag.Genre = new SelectList(Enum.GetNames(typeof(Genre)));
			DbContextOptions<DbConnection> options = new();

			options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
			DbConnection db = new(options);
				return View(tracks);
		}

		// GET: Musics/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null || _context.Musics == null)
			{
				return NotFound();
			}

			var music = await _context.Musics.FirstOrDefaultAsync(m => m.MusicId == id);
			if (music == null)
			{
				return NotFound();
			}
			await TryGetCoverAndTrack(id.Value);
			return View(music);
		}

		// GET: Musics/Create
		public IActionResult Create()
		{
			ViewBag.Genre = new SelectList(Enum.GetNames(typeof(Genre)));
			return View();
		}

		// POST: Musics/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("MusicId,Name,Author,PublishedDate,Length,Publisher,Genre,Cover")] Music music)
		{
			if (ModelState.IsValid)
			{
				_context.Musics.Add(music);
				await _context.SaveChangesAsync();
				int id = music.MusicId;
				await ProcessCoverAndTrackForm(id, true);
				return RedirectToAction(nameof(Details), new { id });
			}
			return View(music);
		}

		// GET: Musics/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null || _context.Musics == null)
			{
				return NotFound();
			}

			var music = await _context.Musics.FindAsync(id);
			if (music == null)
			{
				return NotFound();
			}

			ViewBag.Genre = new SelectList(Enum.GetNames(typeof(Genre)));

			await TryGetCoverAndTrack(id.Value);

			return View(music);
		}

		// POST: Musics/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("MusicId,Name,Author,PublishedDate,Length,Publisher,Genre")] Music music)
		{
			if (id != music.MusicId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				await ProcessCoverAndTrackForm(id);
				try
				{
					_context.Update(music);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!MusicExists(music.MusicId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Details), new { id });
			}
			return View(music);
		}

		// GET: Musics/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null || _context.Musics == null)
			{
				return NotFound();
			}

			var music = await _context.Musics
				.FirstOrDefaultAsync(m => m.MusicId == id);
			if (music == null)
			{
				return NotFound();
			}

			return View(music);
		}

		// POST: Musics/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			if (_context.Musics == null)
			{
				return Problem("Entity set 'DbConnection.Musics'  is null.");
			}
			var music = await _context.Musics.FindAsync(id);
			if (music != null)
			{
				_context.Musics.Remove(music);
			}

			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool MusicExists(int id)
		{
			return _context.Musics.Any(e => e.MusicId == id);
		}

		private async void GetClients()
		{
			GetBlobServiceClient(ref blobServiceClient, magazineAccountName, connectionKey);
			GetBlobClient(ref blobClient, magazineAccountName, connectionKey);
			coversClient = blobServiceClient.GetBlobContainerClient("covers");
			tracksClient = blobServiceClient.GetBlobContainerClient("tracks");
		}

		private async Task ProcessCoverAndTrackForm(int id, bool create = false)
		{
			IFormFile? cover = null;
			IFormFile? track = null;
			foreach (IFormFile file in Request.Form.Files)
			{
				if (file.ContentType.Contains("image"))
					cover = file;
				if (file.ContentType.Contains("audio"))
					track = file;
			}

			string path = "temp/upload/" + id;

			if (create) Directory.CreateDirectory(path);

			if (cover != null)
			{
				using (var stream = System.IO.File.Open($"{path}/cover_{id}.png", FileMode.Create))
					cover.CopyTo(stream);
				await UploadFile(coversClient, $"temp/upload/{id}/cover_{id}.png");
			}

			if (track != null)
			{
				using (var stream = System.IO.File.Open($"{path}/track_{id}.mp3", FileMode.Create))
					track.CopyTo(stream);
				await UploadFile(tracksClient, $"temp/upload/{id}/track_{id}.mp3");
			}
		}

		private async Task TryGetCoverAndTrack(int id)
		{
			string base64;
			try
			{
				base64 = Convert.ToBase64String(await DownloadFileContent(coversClient, $"cover_{id}.png"));
				ViewData["Cover"] = "data:image / jpeg; base64," + base64;
			}
			catch (Exception) { };
			try
			{
				base64 = Convert.ToBase64String(await DownloadFileContent(tracksClient, $"track_{id}.mp3"));
				ViewData["Track"] = "data:audio / mp3; base64," + base64;
			}
			catch (Exception) { };
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
	}
}
//282
//232
//286