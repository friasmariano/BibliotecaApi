
using BibliotecaApi.Models;
using LinqKit;
using System.Linq.Expressions;

namespace BibliotecaApi.Predicates
{
    public class BuscarLibroPredicate
    {
        public Expression<Func<Libro, bool>> BuscarLibroFilter (SearchFilter filter)
        {
            var predicate = PredicateBuilder.New<Libro>(true);

            if (!string.IsNullOrEmpty(filter.Titulo))
            {
                predicate = predicate.And(p => p.Titulo.Contains(filter.Titulo));
            }



            return predicate;
        }
    }

    public class SearchFilter {
        public string Titulo {  get; set; } = string.Empty;
        public string Categoria {  get; set; } = string.Empty;
        public string FechaPublicacion { get; set; } = string.Empty;
    }
}
