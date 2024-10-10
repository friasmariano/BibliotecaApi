
using BibliotecaApi.Models;
using LinqKit;
using System.Linq.Expressions;

namespace BibliotecaApi.Predicates
{
    public class BuscarLibroPredicate
    {
        public Expression<Func<Libro, bool>> BuscarLibroFilter (SearchFilter searchFilter)
        {
            var predicate = PredicateBuilder.New<Libro>(true);

            if (!string.IsNullOrEmpty(searchFilter.Categoria))
            {
                predicate = predicate.And(p => p.Titulo.Contains(searchFilter.NombreLibro));
            }



            return predicate;
        }
    }

    public class SearchFilter {
        public DateTime FechaPublicacion { get; set; }
        public string NombreLibro {  get; set; } = string.Empty;
        public string Categoria {  get; set; } = string.Empty;
    }
}
