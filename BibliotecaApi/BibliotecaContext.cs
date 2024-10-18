using BibliotecaApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaApi
{
    public class BibliotecaContext : DbContext
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
        public DbSet<Rol> Roles { get; set; }
        public DbSet<UserToken> UserTokens { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Persona)
                .WithMany()
                .HasForeignKey(u => u.PersonaId);
        
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Rol)
                .WithMany()
                .HasForeignKey(u => u.RoldId);
        }
        
    }
}
