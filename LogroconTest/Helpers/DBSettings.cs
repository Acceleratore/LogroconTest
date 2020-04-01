using System;

namespace LogroconTest.Helpers
{
    public class DBSettings
    {
        public string Server { get; set; }

        public string DBName { get; set; }

        public string Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }
        
        public string Schema { get; set; }

        public string ConnectionTimeout { get; set; }
        
        public string MaxConnections { get; set; }
        
        public int MaxCommandTime { get; set; }

        public string GetConnectionString()
        {
            return string.Format("Host={0};Port={1};DataBase={2};Username={3};Password={4};", Server, Port, string.IsNullOrWhiteSpace(DBName) ? UserName : DBName, UserName, Password);
        }
        
        public string GetSQLNamespace()
        {
            return String.IsNullOrEmpty(Schema) ? String.Empty : Schema + ".";
        }

        public DBSettings()
        {

        }

        public DBSettings(string _server, string _dBName, string _port, string _schema, string _userName, string _password)
        {
            Server   = _server;
            DBName   = _dBName;
            Port     = _port;
            Schema   = _schema;
            UserName = _userName;
            Password = _password;
        }

    }
}
