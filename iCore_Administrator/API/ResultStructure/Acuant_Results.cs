using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iCore_Administrator.API.ResultStructure
{
    public class Acuant_CLS_Async
    {
        public Transaction_CLS Transaction { get; set; }
    }

    public class Acuant_CLS_OCR_Sync
    {
        public Transaction_CLS Transaction { get; set; }
        public object Data { get; set; }
        public Transaction_File Files { get; set; }
    }

    public class Acuant_CLS_Validation_Sync
    {
        public Transaction_CLS Transaction { get; set; }
        public object Data { get; set; }
        public object Alert { get; set; }
        public object Authentication { get; set; }
        public Transaction_File Files { get; set; }
    }
}