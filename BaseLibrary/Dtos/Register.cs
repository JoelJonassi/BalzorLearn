using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BaseLibrary.Dtos
{
    public class Register : AccountBase
    {

        [Required]
        [MinLength(5)]
        [MaxLength(100)]
        [Column("nome_utilizador")]
        public string FullName { get; set; }

        [Column("abv_nome")]
        public string ShortName { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        [Required]
        public string ConfirmPassword { get; set; }

        [Required]
        [Column("id_area")]
        public int IdArea { get; set; }

        [Required]
        [Column("ativo")]
        public bool Active { get; set; }


    }
}
