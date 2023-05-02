using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusicBase.Models
{
	public class Music
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int MusicId { get; set; }

		[DisplayName("Tytuł")]
		public string Name { get; set; }

		[DisplayName("Autor")]
		public string Author { get; set; }

		[DisplayName("Data wydania")]
		[BindProperty, DataType(DataType.Date)]
		public DateTime PublishedDate { get; set; }

		[DisplayName("Długość")]
		[BindProperty, DataType(DataType.Time)]
		public DateTime Length { get; set; }

		[DisplayName("Wydawca")]
		public string Publisher { get; set; }

		[DisplayName("Gatunek")]
		public Genre Genre { get; set; }
		public byte[]? Cover { get; set; }
		public byte[]? Track { get; set; }
	}
	public enum Genre
	{
		pop, disco, klasyczna, hiphop, rap
	}
}
