using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monoponly
{
    class Class1
    {


        public class Rootobject
        {
            public string docType { get; set; }
            public int operatorId { get; set; }
            public string userGroupGuid { get; set; }
            public Groupmeta groupMeta { get; set; }
            public Groupproperties groupProperties { get; set; }
            public Tag[] tags { get; set; }
        }

        public class Groupmeta
        {
            public string name { get; set; }
            public string description { get; set; }
            public bool isDeleted { get; set; }
            public bool isVirtual { get; set; }
        }

        public class Groupproperties
        {
            public DateTime expiryDate { get; set; }
            public Cleanupschedule cleanupSchedule { get; set; }
        }

        public class Cleanupschedule
        {
            public DateTime nextRun { get; set; }
            public string recurrenceTypeId { get; set; }
            public int recurrenceInterval { get; set; }
        }

        public class Tag
        {
            public string tag { get; set; }
            public bool isSystem { get; set; }
        }

    }
}
