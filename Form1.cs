using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using MySql.Data.MySqlClient;
using System.Text.RegularExpressions;
using System.IO;  //以文件流的方式打开文件
namespace 数据库framework版本
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string temp = @"Insert into EXPORT_TABLE(MSG_SEQ, MSG_ID, GATEWAY_TIME, PRIORITY, RCV_ADDRESS, SND_ADDRESS, BEP_TIME, SMI
              .BJSXCXA 030245
DFD
              FI CA4461 / AN B - 6047
              C0TCF05DCFD10070 /
              C106, 000, 1000, 45, X010, 0, 010X, 45, X /
              
');";
            if (Regex.IsMatch(temp, @"^\s+\n"))
            {
                temp = Regex.Replace(temp, @"^\s+\n", "");
                MessageBox.Show(temp);
            }


            string str = "server=localhost; User Id=root; password=qwer123456789; Database=acars;";
            MySqlConnection conn = new MySqlConnection(str);
            try
            {
                conn.Open();
                MessageBox.Show("连接成功");
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);

            }
            finally
            {
                conn.Close();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            //测试程序运行时间
            DateTime start = System.DateTime.Now;
            
            //记录打开文件的名字
            OpenFileDialog originFile = new OpenFileDialog();
            originFile.ShowDialog();
            var originFilePath = originFile.FileName;
            var changed = Regex.Replace(originFilePath, @"\w+.txt", "temp2.txt");     //将原来的文件重命名为temp2.txt

            //开始读源文件
            try
            {

                StreamReader sr = new StreamReader(originFilePath);
                //string a = sr.ReadToEnd();    //全部读完

                string a = sr.ReadLine();       //一次读一行

                while (null != a)
                {
                    bool month;
                    /*原文件格式为：,to_date('09-6ÔÂ -19','DD-MON-RR')
                     * 需要改成： str_to_date('09-6-19','%d-%m-%y')
                    
                     */
                    //if (month = Regex.IsMatch(a, @"(to_date)(\(\'\d{2}-\d)(\D+)(-)(\d{2}\',\')(DD-MON-RR)"))
                    //{
                    //    a = Regex.Replace(a, @"(to_date)(\(\'\d{2}-\d)(\D+)(-)(\d{2}\',\')(DD-MON-RR)", "str_to_date$2 $4$5%d-%m-%y");
                    //}

                    //a = Regex.Replace(a, @"^\s+\n$", "");
                    //MessageBox.Show(a);
                    if (a.Length != 0)
                    {
                        //正则表达式搞不定 ，只好使用这个方法 ，已知空行的Length=0 这样只有长度不为0的时候才进行写入
                        a = Regex.Replace(a, @"(to_date)(\(\'\d{2}-\d)(\D+)(-)(\d{2}\',\')(DD-MON-RR)", "str_to_date$2 $4$5%d-%m-%y");

                        //向新文件末尾进行添加，如果新文件不存在 则新建 
                        StreamWriter readTxt = File.AppendText(changed);
                        readTxt.WriteLine(a);           //这里不能写成Write() 写成了Write就没有空格了 对于回车的会出现错误
                        readTxt.Close();
                        
                    }
                    a = sr.ReadLine();
                }
                sr.Close();     //读取结束 关闭原来的文件
            }
            catch (Exception e1)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e1.Message);
            }
            
            //输出运行时间
            DateTime end = DateTime.Now;
            TimeSpan last = end - start;
            String showmessage = "转换一共" + "执行了:" + last.Hours + "小时" + last.Minutes + "分" + last.Seconds + "秒";
            MessageBox.Show(showmessage);

        }

        private void button3_Click(object sender, EventArgs e)
        {

            DateTime start = System.DateTime.Now;

            //连接数据库
            string str = "server=localhost; User Id=root; password=qwer123456789; Database=acars;";

            try
            {
                //记录打开的文件的名字
                OpenFileDialog originFile = new OpenFileDialog();
                originFile.ShowDialog();
                var originFilePath = originFile.FileName;
                StreamReader sr = new StreamReader(originFilePath);


                //从文本文件中读入数据

                string a=sr.ReadLine();
                /*先找到第一个Insert Into 语句开始的地方 
                 * 通过观察发现 每个insert into 都是在新的一行
                 * 每个'); 也是在新的一行
                 
                 */
                 //找到第一个Insert into语句开始的地方
                while (!Regex.IsMatch(a, @"Insert into EXPORT_TABLE"))
                {
                    a = sr.ReadLine();
                }
                string temp3 = "";
                while (null != a)
                {
                    bool month;
                    /*
                     * 刚进去肯定能匹配 找到一个insert 
                     */
                    while (month = Regex.IsMatch(a, @"Insert into EXPORT_TABLE.+$"))   //匹配 以Insert 开头一直到这一行的结束
                    {
                        string temp4 = Regex.Match(a, @"Insert into EXPORT_TABLE.+$").Value;   //把这个存储在 temp4
                        temp3 = temp3 + temp4;      //连接起来
                        a = sr.ReadLine();         //再读一行 一直到读到结束标志为止。

                        //读到结束标志的上一行 把这些全部都连接在 temp3 后面
                        while (a != null && (Regex.IsMatch(a, @"\'\)\;")==false))    //匹配结尾的 ');标志
                        {
                            temp3 += a;
                            a = sr.ReadLine();
                        }
                        //跳出循环肯定是匹配到结尾了 因为这个文件的末尾一定有一个'); 先是碰到 这个 再遇到结束符的
                        if (Regex.IsMatch(a, @"\'\);"))  
                        {
                            temp3 += a;     //现在已经是完整的一段了，可以进行数据库的插入操作了。


                            //去掉多余的空格这些 便于数据库的存储.这里显示是有问题 但是输出到文本框没有问题
                            temp3 = Regex.Replace(temp3, @"\s+", " ");    //多个空格 以一个空格进行代替

                            //把这一段导入数据库里面
                            MySqlConnection conn = new MySqlConnection(str);
                            try
                            {
                                conn.Open();
                                MySqlCommand cmd = new MySqlCommand(temp3, conn);
                                cmd.ExecuteNonQuery();
                                conn.Close();
                                //MessageBox.Show("数据插入成功");
                                
                            }
                            catch (Exception e4)
                            {
                                //MessageBox.Show("数据插入失败：" + e4.Message);  
                            }
                            //temp3还需要接受缓存新的数据 所以需要清空
                            temp3 = "";
                        }
                    }
                    a = sr.ReadLine();  //新读入一行数据
                }
                sr.Close();   //文件全部读取完毕 关闭文件
       

            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message);

            }

            DateTime end = DateTime.Now;
            TimeSpan last = end - start;
            //Console.WriteLine("执行了:{0}小时{1}分{2}秒", last.Hours, last.Minutes, last.Seconds);
            String showmessage = "转换一共" + "执行了:" + last.Hours + "小时" + last.Minutes + "分" + last.Seconds + "秒";
            MessageBox.Show(showmessage);

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "txt文件导入mysql";
            //this.BackColor = Color.Red; //设置背景颜色
            this.BackgroundImage = Image.FromFile(@"./123.jpg");//相对路径为bin/Debug
        }
    }
}
