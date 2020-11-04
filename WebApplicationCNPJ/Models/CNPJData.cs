using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplicationCNPJ.Models
{
    public class CNPJData
    {
        public int Id { get; set; }

        [RegularExpression( @"\d{2}\.\d{3}\.\d{3}\/\d{4}\-\d{2}", ErrorMessage = "CNPJ invalido." )]
        public string CNPJCode { get; set; }

        public string Nome { get; set; }

        public string Fantasia { get; set; }

        public string Logradouro { get; set; }

        public string Numero { get; set; }

        public string Complemento { get; set; }

        public string Cep { get; set; }

        public string Bairro { get; set; }

        public string Municipio { get; set; }

        public string UF { get; set; }

        public string Email { get; set; }

        public string Telefone { get; set; }

        public string EFR { get; set; }

        public string Situacao { get; set; }

    }
}
