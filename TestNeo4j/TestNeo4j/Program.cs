using Neo4j.Driver.V1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestNeo4j
{
    class Program : IDisposable
    {
        private readonly IDriver _driver;

        public Program(string uri, string user, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        public void PrintGreeting(string message)
        {
            using (var session = _driver.Session())
            {
                var greeting = session.WriteTransaction(tx =>
                {
                    var result = tx.Run("CREATE (a:Greeting) " +
                                        "SET a.message = $message " +
                                        "RETURN a.message + ', from node ' + id(a)",
                        new { message });
                    return result.Single()[0].As<string>();
                });
                Console.WriteLine(greeting);
            }
        }

        public void Dispose()
        {
            _driver?.Dispose();
        }
        static void Main(string[] args)
        {
            using (var greeter = new Program("bolt://localhost:7687", "neo4j", "root"))
            {
                greeter.PrintGreeting("hello, world");
            }
            Console.Read();
        }
    }
}
