﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PerkinElmer.COE.Registration.Server.Code
{
    public class RegistrationException : Exception
    {
        public RegistrationException(String message) : base(message)
        {
        }
    }
}