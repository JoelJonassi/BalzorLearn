using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BaseLibrary.Entities{
    public class ApplicationUser {

        //[Column("id_utilizador")]
        public int Id {get; set; }

        //[Column("nome_utilizador")]
        public string? FullName {get; set; }
        public string? Email {get; set; }
        public string? Password {get; set; }

       /* [Column("abv_nome")]
        public string ShortName { get; set; }

        [Column("id_area")]
        public int IdArea { get; set; }

        [Required]
        [Column("ativo")]
        public bool Active { get; set; }*/
    }
}