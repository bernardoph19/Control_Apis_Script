using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONTROL_APIS.Models
{
    public class Proyecto
    {
        public string nombre { get; set; }
        public List<ApisProyecto> apis { get; set; }
    }
}
