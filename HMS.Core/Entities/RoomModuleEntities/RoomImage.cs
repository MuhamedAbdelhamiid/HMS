using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HMS.Core.Entities.RoomModuleEntities
{
    public class RoomImage : BaseEntity<int>
    {
        public string ImageUrl { get; set; } = default!;
        public int RoomId { get; set; }
    }
}
