using BibliotecaApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi
{
    public class BibliotecaContext : IdentityDbContext<IdentityUser>
	{
        public BibliotecaContext(DbContextOptions<BibliotecaContext> options)
            : base(options)
        {

        }

        public DbSet<Autor> Autores { get; set; }
        public DbSet<AutorLibro> AutoresLibros { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<LibroCategoria> LibrosCategorias { get; set;}
        public DbSet<Persona> Personas { get; set; }
        public DbSet<PersonaUser> PersonasUser { get; set; }
    }
}
