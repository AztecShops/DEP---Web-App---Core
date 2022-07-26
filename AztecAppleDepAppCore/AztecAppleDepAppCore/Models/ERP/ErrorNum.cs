using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore;

namespace AztecAppleDepApp.Models.ERP
{
    public enum ErrorEnum
    {
        OK = 0,
        No_uv_access = 1000,
        Uv_Exception = 1100,
        Uv_SUBR_Error = 1200,

        Generic = 10000,
        Uknown = 11000
    }
}