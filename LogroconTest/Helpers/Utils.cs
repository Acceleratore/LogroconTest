using LogroconTest.Models;
using System;

namespace LogroconTest.Helpers
{
    public static class Utils
    {
        public static ModelResponse GetResponse(string session, string message = null)
        {
            return new ModelResponse() { Session = session, Message = message };
        }
        
        public static object NoNullValue(string value)
        {
            return string.IsNullOrEmpty(value) ? (object)DBNull.Value : (object)value;
        }
    }
}
