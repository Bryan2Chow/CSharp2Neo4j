using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextPersonRelationship
{
    public partial class Form1 : Form
    {/// <summary>
    /// 调用Neo4jDao类
    /// </summary>
        Neo4jDao neo4jDao = new Neo4jDao(new Uri("bolt://localhost:7687"), "neo4j", "TestPersonRelatioships");//TestPersonRelatioships    Graph
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)//创建方法 添加节点
        {
            //两种表示方式
            //string name = name1.Text;
            //string g = gender.Text;
            //string a = age.Text;
            //Person p = new Person();
            //p.name = name;
            //p.gender = g;
            //p.age = a;
            Person p = new Person();
            p.name = name1.Text;
            p.age = age.Text;
            p.gender = gender.Text;

            neo4jDao.createPersonNode(p);//调用类中的 创建人节点的方法
            name1.Text = null;// 通过将输入框体内容的清空  方便知道操作执行情况
            //gender.Text = null;
            //age.Text = null;
            MessageBox.Show("添加成功!\n" + "name:" + p.name + "\n" + "gender:" + p.gender + "\n" + "age:" + p.age);// 通过执行 弹出显示添加成功及相应添加内容的信息
        }

        private void button3_Click(object sender, EventArgs e)//添加关系
        {
            string n1 = name1.Text;
            string n2 = name2.Text;
            //string r = relationship.Text;  用的是textBox 输入框
            string r = relationship1.Text;  //用的是comboBox 下拉选择框
            neo4jDao.createRelationship(n1, n2, r);
            MessageBox.Show("success!");
        }

        private void button2_Click(object sender, EventArgs e)//查找关系的最短路径
        {
            string n1 = name1.Text;
            string n2 = name2.Text;
            ArrayList nodes = neo4jDao.queryPath(n1, n2);
            ArrayList persons = (ArrayList)nodes[0];
            ArrayList relationships = (ArrayList)nodes[1];
            for(int i = 0; i< persons.Count; i++)
            {
                textBox1.AppendText(((Person)persons[i]).name + "--");
                if (i < relationships.Count)
                {
                    textBox1.AppendText(relationships[i] + "->");
                }
                
            }
            // textBox1.AppendText(((Person)persons[i]).name) = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string relationname1 = name1.Text;
            string relationname2 = name2.Text;
            neo4jDao.deleteRelationship(relationname1, relationname2);
            MessageBox.Show("delete success");
        }
    }
}
