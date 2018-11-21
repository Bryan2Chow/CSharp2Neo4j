using Neo4j.Driver.V1;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextPersonRelationship
{/// <summary>
/// 先配置连接neo4j 驱动
/// </summary>
    class Neo4jDao
    {
        private readonly IDriver _driver;
        public Neo4jDao()
        {

        }
        public Neo4jDao(Uri uri, string name, string password)
        {
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(name, password));
        }
       /// <summary>
       /// 创建人的关系节点
       /// </summary>
       /// <param name="person"></param>
        public void createPersonNode(Person person) // 创建一个createPersonNode方法
        {
            var session = _driver.Session(); //向驱动程序对象请求一个新的会话
            string statement = "create (a:Person{name:" + "\"" + person.name + "\",gender:\"" + person.gender +"\",age:\""+person.age+"\"})";//创建带有属性的人的节点
            session.WriteTransaction(tx =>    //请求会话对象创建（写入）事务
            {
                IStatementResult result = tx.Run(statement);
            });//运行创建的cypher语句

        }

        public void createRelationship(string name1, string name2,string relationship)
        {
            var session = _driver.Session();
            string statement = "match (a:Person{name:" + "\"" + name1 + "\"}),(b:Person{name:" + "\"" + name2+ "\"})"
                +"create (a)-[:"+relationship+"]->(b)";//创建两个人之间的关系
            session.WriteTransaction(tx =>
            {
                IStatementResult result = tx.Run(statement);
            });
        }

        public void deleteRelationship(string name1, string name2)
        {
            var session = _driver.Session();
            string statement = "match (a:Person{name:" + "\"" + name1 + "\"})-[r]-(b:Person{name:" + "\"" + name2 + "\"})"
                + "delete r";
            session.WriteTransaction(tx =>
            {
                IStatementResult result = tx.Run(statement);
            });
        }

        public ArrayList queryPath(string name1, string name2)//定义一个查询路径方法
        {
            ArrayList total = new ArrayList();//
            ArrayList persons = new ArrayList();//人的空数组列表
            ArrayList relationships = new ArrayList();//关系的空数组列表
            var session = _driver.Session();
            string statement = "match (a:Person{name:" + "\"" + name1 + "\"}),(b:Person{name:" + "\"" + name2 + "\"})"
                +"match path=shortestpath((a)-[*]-(b)) return path";//创建两个节点之间的最短路径  
            session.ReadTransaction(tx =>{
                IStatementResult result = tx.Run(statement);
                IEnumerator<IRecord> records = result.GetEnumerator();
                // 由于最短路径函数shortest() 只显示最短路径结果  路径之间的节点及相应关系的详细信息需自己提取
                // 因此需要考虑路径的起始节点、终止节点信息、 中间环节的相应的起始节点和下一个的终节点
                while (records.MoveNext())
                {
                    IRecord record = records.Current;
                    IReadOnlyDictionary<string, object> recordValues = record.Values;
                    foreach (var recordValue in recordValues)
                    {
                        IPath path = recordValue.Value.As<IPath>();
                        //IPath path = 
                        //INode node = recordValue.Value.As<INode>();
                        INode startNode = path.Start.As<INode>();
                        INode endNode = path.End.As<INode>();
                        IReadOnlyList<IRelationship> rss = path.Relationships;
                        IReadOnlyList<INode> ns = path.Nodes;

                        
                        foreach(INode node in ns)
                        {
                            IReadOnlyDictionary<string, object> properties = node.Properties;
                            Person p = new Person();
                            foreach (var keyValuePair in properties)
                            {

                                Console.WriteLine(keyValuePair.Key + ":" + keyValuePair.Value);
                                if (keyValuePair.Key.Equals("name"))
                                {
                                    p.name = keyValuePair.Value.ToString();
                                }
                                else if (keyValuePair.Key.Equals("gender"))
                                {
                                    p.gender = keyValuePair.Value.ToString();
                                }
                                else if (keyValuePair.Key.Equals("age"))
                                {
                                    p.age = keyValuePair.Value.ToString();
                                }
                            }
                            persons.Add(p);
                        }
                        foreach(IRelationship rs in rss)
                        {
                            string relationshipName = rs.Type;
                            relationships.Add(relationshipName);
                        }
                        
                    }
                }
            });
            total.Add(persons);
            total.Add(relationships);
            return total;
        }
        //public ArrayList queryRelation(string name)
        //{
        //    ArrayList list = new ArrayList();

        //    var session = _driver.Session();
        //    string statement = "match (m:Machine{name:\"" + name + "\"}) --> (q) return q";
        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //        IEnumerator<IRecord> records = result.GetEnumerator();
        //        while (records.MoveNext())
        //        {
        //            IRecord record = records.Current;
        //            IReadOnlyDictionary<string, object> recordValues = record.Values;
        //            foreach (var recordValue in recordValues)
        //            {
        //                INode node = recordValue.Value.As<INode>();
        //                IReadOnlyDictionary<string, object> properties = node.Properties;
        //                foreach (var keyValuePair in properties)
        //                {
        //                    if (keyValuePair.Key.Equals("name"))
        //                    {
        //                        list.Add(keyValuePair.Value);
        //                    }
        //                }
        //            }
        //        }
        //    });
        //    return list;
        //}
        ///// <summary>
        ///// 添加机器节点
        ///// </summary>
        ///// <param name="machine"></param>
        ///// <param name="database"></param>
        //public void createMachineNode(Machine machine, string database)
        //{
        //    Type type = machine.GetType();//  what dose the meaning
        //    //获取对话并声明创建节点语句
        //    var session = _driver.Session();
        //    string statement = "merge (m1:" + type.Name + "{name:\"" + machine.name + "\"," +
        //                       "position:\"" + machine.position + "\"," +
        //                       "currentType:\"" + machine.currentType + "\"," +
        //                       "database:\"" + database + "\"" +
        //                       "})";
        //    session.WriteTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);

        //    });

        //}
        //public void createMachineRelated(Machine machine, string machineName)
        //{
        //    Type type = machine.GetType();
        //    //获取对话并声明创建设备节点间语句
        //    var session = _driver.Session();
        //    string statement = "match (m2:Machine{name:\"" + machine.name +
        //                       "\"}),(m1:Machine{name:\"" + machineName + "\"}) " +
        //                       "create (m2)-[r1:NextTo]->(m1) return r1";
        //    session.WriteTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);

        //    });

        //}
        ///// <summary>
        ///// 通过设备信息，各种类型，各种名字构建关系
        ///// </summary>
        ///// <param name="machine"></param>
        ///// <param name="name"></param>
        ///// <param name="nameType"></param>
        //public void createRelationShip(Machine machine, string name, string nameType)
        //{
        //    var session = _driver.Session();
        //    string statement = "";
        //    if ("sharp".Equals(nameType))
        //    {
        //        statement = "match (m:Machine{name:\"" + machine.name +
        //                       "\"}),(s:Sharp{name:\"" + name + "\"}) " +
        //                       "create (m)-[r1:Can_Operate]->(s)-[r2:Canbe_OperatedBy]->(m) return r1,r2";
        //    }
        //    else if ("material".Equals(nameType))
        //    {
        //        statement = "match (m:Machine{name:\"" + machine.name +
        //                       "\"}),(s:Material{name:\"" + name + "\"}) " +
        //                       "create (m)-[r1:Can_Deal]->(s)-[r2:Can_DealBy]->(m) return r1,r2";
        //    }
        //    else if ("partCategory".Equals(nameType))
        //    {
        //        statement = "match (m:Machine{name:\"" + machine.name +
        //                       "\"}),(s:PartCategory{name:\"" + name + "\"}) " +
        //                       "create (m)-[r1:Can_Process]->(s)-[r2:Can_ProcessedBy]->(m) return r1,r2";
        //    }
        //    else if ("processingType".Equals(nameType))
        //    {
        //        statement = "match (m:Machine{name:\"" + machine.name +
        //                       "\"}),(s:ProcessingType{name:\"" + name + "\"}) " +
        //                       "create (m)-[r1:Doing]->(s)-[r2:Contains]->(m) return r1,r2";
        //    }
        //    session.WriteTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //    });
        //}

        //public ArrayList queryRelation(string name)
        //{
        //    ArrayList list = new ArrayList();

        //    var session = _driver.Session();
        //    string statement = "match (m:Machine{name:\"" + name + "\"}) --> (q) return q";
        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //        IEnumerator<IRecord> records = result.GetEnumerator();
        //        while (records.MoveNext())
        //        {
        //            IRecord record = records.Current;
        //            IReadOnlyDictionary<string, object> recordValues = record.Values;
        //            foreach (var recordValue in recordValues)
        //            {
        //                INode node = recordValue.Value.As<INode>();
        //                IReadOnlyDictionary<string, object> properties = node.Properties;
        //                foreach (var keyValuePair in properties)
        //                {
        //                    if (keyValuePair.Key.Equals("name"))
        //                    {
        //                        list.Add(keyValuePair.Value);
        //                    }
        //                }
        //            }
        //        }
        //    });
        //    return list;
        //}
        ///// <summary>
        ///// 判断机器名字是否存在
        ///// </summary>
        ///// <param name="name"></param>
        ///// <returns></returns>
        //public bool neededNameOperation(string name)
        //{
        //    var session = _driver.Session();
        //    string statement = "match (a:Machine{name:\"" + name + "\"" + "}) return a";
        //    IEnumerator<IRecord> records = null;
        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //        records = result.GetEnumerator();
        //    });
        //    if (records.MoveNext())
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }

        //}
        ///// <summary>
        ///// 判断机器位置是否存在
        ///// </summary>
        ///// <param name="position"></param>
        ///// <returns></returns>
        //public bool neededPositionOperation(string position)
        //{
        //    var session = _driver.Session();
        //    string statement = "match (a:Machine{position:\"" + position + "\"" + "}) return a";
        //    IEnumerator<IRecord> records = null;
        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //        records = result.GetEnumerator();
        //    });
        //    if (records.MoveNext())
        //    {
        //        return false;
        //    }
        //    else
        //    {
        //        return true;
        //    }
        //}

        //public string generateJsonFileOnNodes()
        //{
        //    var session = _driver.Session();

        //    StringBuilder nodes = new StringBuilder();
        //    nodes.Append("[\n");

        //    string statement = "match (m:Machine) return m";

        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(new Statement(statement));
        //        IEnumerator<IRecord> records = result.GetEnumerator();

        //        while (records.MoveNext())
        //        {
        //            IRecord record = records.Current;
        //            IReadOnlyDictionary<string, object> recordValues = record.Values;

        //            foreach (var recordValue in recordValues)
        //            {
        //                INode node = recordValue.Value.As<INode>();

        //                long id = node.Id;
        //                nodes.Append("{\"id\":\"" + id + "\"" + ",");

        //                IReadOnlyList<string> labels = node.Labels;
        //                nodes.Append("\"labels\":\"" + string.Join(",", labels.ToArray()) + "\"" + ",");

        //                IReadOnlyDictionary<string, object> properties = node.Properties;
        //                foreach (var property in properties)
        //                {
        //                    nodes.Append("\"" + property.Key + "\":\"" + property.Value + "\"" + ",");
        //                }
        //                nodes.Remove(nodes.Length - 1, 1);
        //                nodes.Append("},\n");
        //            }
        //        }
        //    });
        //    nodes.Remove(nodes.Length - 2, 1);
        //    nodes.Append("]");
        //    //Console.WriteLine(nodes);
        //    return nodes.ToString();
        //}


        //public string generateJsonFileOnNodeByPack(string currentType)
        //{
        //    var session = _driver.Session();

        //    StringBuilder nodes = new StringBuilder();
        //    nodes.Append("{\n");
        //    nodes.Append("\"currentType\":\"" + currentType + "\"," + "\n");
        //    nodes.Append("\"children\":" + "\n");
        //    nodes.Append("[\n");

        //    string statement = "match (m1:Machine{currentType:\"" + currentType + "\"}) return m1";

        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //        IEnumerator<IRecord> records = result.GetEnumerator();

        //        while (records.MoveNext())
        //        {

        //            IRecord record = records.Current;
        //            IReadOnlyDictionary<string, object> recordValues = record.Values;

        //            foreach (var recordValue in recordValues)
        //            {
        //                //Console.WriteLine(recordValue);
        //                INode node = recordValue.Value.As<INode>();

        //                long id = node.Id;
        //                nodes.Append("{\"id\":\"" + id + "\"" + ",");

        //                IReadOnlyList<string> labels = node.Labels;
        //                nodes.Append("\"labels\":\"" + string.Join(",", labels.ToArray()) + "\"" + ",");

        //                IReadOnlyDictionary<string, object> properties = node.Properties;
        //                foreach (var property in properties)
        //                {
        //                    nodes.Append("\"" + property.Key + "\":\"" + property.Value + "\"" + ",");
        //                }
        //                nodes.Remove(nodes.Length - 1, 1);
        //                nodes.Append("},\n");
        //            }
        //        }
        //    });
        //    nodes.Remove(nodes.Length - 2, 1);
        //    nodes.Append("]\n}");
        //    Console.WriteLine(nodes);
        //    return nodes.ToString();
        //}


        //public string generateJsonFileOnRelation()
        //{
        //    var session = _driver.Session();

        //    StringBuilder links = new StringBuilder();
        //    links.Append("[\n");
        //    string statement = "match p=((m1:Machine)-[r]->(m2:Machine)) return p";

        //    session.ReadTransaction(tx =>
        //    {
        //        IStatementResult result = tx.Run(statement);
        //        IEnumerator<IRecord> records = result.GetEnumerator();

        //        while (records.MoveNext())
        //        {
        //            IRecord record = records.Current;
        //            IReadOnlyDictionary<string, object> recordValues = record.Values;

        //            foreach (var recordValue in recordValues)
        //            {
        //                IPath path = recordValue.Value.As<IPath>();

        //                IReadOnlyList<IRelationship> relationships = path.Relationships;
        //                foreach (var relationship in relationships)
        //                {
        //                    links.Append("{\"source\":" + "\"" + relationship.StartNodeId + "\"," + "\"target\":" + "\"" + relationship.EndNodeId + "\"");
        //                }
        //                //links.Remove(links.Length - 1, 1);
        //                links.Append("},\n");
        //            }
        //        }
        //    });
        //    links.Remove(links.Length - 2, 1);
        //    links.Append("]\n");
        //    //Console.WriteLine(links);
        //    return links.ToString();
        //}
    }
}
