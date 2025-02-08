using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerTools
{
    public enum MessegeType
    {
        None,
        Success,

        UserNotFound = 50,
        RegisterError = 51,
        Error = 52,
        DataBaseConnectionError = 53,
        UserAlreadyExist = 54,
    }

    public enum CommandIdx
    {
        None,
        GetUser,
        RegisterUser,

        ChangePointsRequest = 10,

        CloseConnection = 20,

        SendMessege = 50,
    }
}
