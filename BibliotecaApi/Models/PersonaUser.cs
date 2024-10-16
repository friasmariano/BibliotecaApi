

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BibliotecaApi.Models
{
	public class PersonaUser
	{
		public int Id { get; set; }

		[ForeignKey("AspNetUser")]
		public string AspNetUserId { get; set; } = string.Empty;

		[ForeignKey("Persona")]
		public long PersonaId { get; set; }

		public virtual IdentityUser? AspNetUser { get; set; }
		public virtual Persona? Persona { get; set; }
	}
}
