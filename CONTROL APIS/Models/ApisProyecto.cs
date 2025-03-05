using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONTROL_APIS.Models
{
    public class ApisProyecto
    {
        public string endPoint { get; set; } // Solo para APIs
        public string estado { get; set; }
        public int puerto { get; set; } // Solo para APIs
        public string RutaLocal { get; set; }
        public string Tecnologia { get; set; }
        public string comandoInicio { get; set; }
        public bool EsScript { get; set; } // Para distinguir entre APIs y scripts
        public string Nombre { get; set; } // Nombre del script (opcional)
    }

}
