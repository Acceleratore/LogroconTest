using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using LogroconTest;

namespace LogroconTest.Helpers
{
    public class Settings
    {
        public DBSettings MainDBConnection { get; set; }

        public bool ChachedEmployee { get; set; }

        public bool ChachedPosts { get; set; }

        public Settings()
        {

        }

    }
}
