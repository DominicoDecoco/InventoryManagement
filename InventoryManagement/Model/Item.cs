using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Model
{
    public class Item
    {
        public int IDItem { get; set; }
        public string NameItem { get; set; } = string.Empty;
        public int TypeIt { get; set; }
        public byte[] ImageQuestion { get; set; }
        public virtual TypeItem TypeItem { get; set; }
    }
}
