

using Microsoft.AspNetCore.Identity;

namespace BibliotecaApi.Responses
{
	public class UserCreationResult
	{
		public bool Success { get; set; }
		public string? Message { get; set; }
		public IdentityUser? User { get; set; }
	}
}
