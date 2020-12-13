using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace iCore_Administrator.Modules.SecurityAuthentication
{
    public class ActionAuthentication
    {
        public bool User_Authentication_Action(int ActionCode)
        {
            try
            {



                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}