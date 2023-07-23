using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServerCore
{
    internal class Program
    {
        static object _obj = new object();
        static void Main(string[] args)
        {
            lock (_obj)
            {

            }
            
        }
    }
}