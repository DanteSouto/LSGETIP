using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSGETIP
{
    internal class AppStatus
    {
        private string _addr;
        private string _login;
        private string _name;
        private bool _status;

        public string Address { get{ return _addr;} }
        public string Login { get{ return _login;} }
        public string Name { get{ return _name;} }
        public bool Status { get{ return _status;} }

        public AppStatus(string name, string login, string addr, bool status) 
        {
            _name = name;
            _login = login;
            _addr = addr;
            _status = status;
        }

        public override string ToString()
        {
            return  _login + ((_name != "")? "/" + _name : "") + " " + _addr + ((_status) ? " connected" : " disconnected");
        }
    }
}
