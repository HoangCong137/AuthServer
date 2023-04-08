using System;
using System.Collections.Generic;
using System.Text;

namespace AuthServer.Domain.Entities
{
    public class Permissions
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid IdUser { get; set; }

        public string Permission { get; set; }

        public int? RoomLock { get; set; }

        public bool IsDeleted { get; set; }
    }
}
