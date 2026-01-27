using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Enums
{
    public enum CourseStatus
    {
        [Display(Name = "Em Breve")]
        EmBreve,
        [Display(Name = "Inscrições Abertas")]
        InscricoesAbertas,
        [Display(Name = "Disponivel")]
        Disponivel
    }
}
