﻿

namespace BibliotecaApi.Models
{
    public class LibroCategoria
    {
        public long Id { get; set; }
        public long LibroId { get; set; }
        public long CategoriaId { get; set; }

        public required Categoria Categoria { get; set; }
    }
}
