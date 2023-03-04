using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Threading;



namespace MapMakerZ
{

   

    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
        }
        public string id;
        Node root = new Node();
        Node node = new Node();
        public int countdivide = 0;
        public int countsort = 0;
        public int temporalis = 0;
        public int temporalis2 = 0;
        public string path = "notnot";
        WayNode roots = new WayNode();
        WayNode wayNode = new WayNode();
        List<string[]> templist;



        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog c = new OpenFileDialog();
            if(c.ShowDialog()==DialogResult.OK)
            {
                label1.Text = c.FileName;
                open = true;
            }


        }

        
        

        bool open = false;
        int bar = 0;

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folder = new FolderBrowserDialog();
            if (folder.ShowDialog() == DialogResult.OK)
            {
                path = folder.SelectedPath;
            }
            backgroundWorker1.RunWorkerAsync();
        }


        private string[] search (string id)
        {

            Node node = new Node();
            node = root;
            for (int i = 0; i <id.Length; i++)
            {
                char c = char.Parse(id.Substring(i, 1));

                if (c - '0' < node.nextAll.Length && node.nextAll[c - '0'] != null)
                {
                    node = node.nextAll[c - '0'];
                }
                else { return null; }
                

            }
            if (node.wordEnd == true)
            {
                string[] ne = new string[] { node.lat, node.lon };
                return ne;
            }
            else 
            {
                MessageBox.Show(id + " not found");
                
            }

            return null;

        }

        private void button3_Click(object sender, EventArgs e)
        {
            string[] found = search(textBox1.Text);
            MessageBox.Show(found?[0] + " " + found?[1]);
        }


        //making maps
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            if (open == false)
            {
                MessageBox.Show("add file");
                return;
            }
            
            
            
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.Async = false;
            settings.IgnoreWhitespace = true;


            int counttrie = 0;
            int temp = 0;
            int countxml = 0;
            int countwrite = 0;

            var watch = System.Diagnostics.Stopwatch.StartNew();
            bool first = true;

            XmlReader read = XmlReader.Create(label1.Text, settings);

            //create trie
            while (read.Read())
            {

                if (read.Name == "node")
                {

                    node = root;
                    XmlReader r = read.ReadSubtree();
                    XElement el = XElement.Load(r);
                    string id = el.Attribute("id").Value;


                    for (int i = 0; i < id.Length; i++)
                    {

                        char c = char.Parse(id.Substring(i, 1));
                        if (node.nextAll[c - '0'] == null)
                        {
                            node.nextAll[c - '0'] = new Node();
                            node = node.nextAll[c - '0'];

                        }
                        else
                        {
                            node = node.nextAll[c - '0'];
                        }
                    }
                    if (node.wordEnd == false)
                    {
                        node.wordEnd = true;
                        node.lat = el.Attribute("lat").Value;
                        node.lon = el.Attribute("lon").Value;
                    }
                    counttrie++;
                    if (temp + 100000 < counttrie)
                    {
                        Thread.Sleep(1 / 1000);
                        backgroundWorker1.ReportProgress(counttrie);
                        temp = counttrie;
                    }
                }

                

            }
            backgroundWorker1.ReportProgress(counttrie);




            

            read.Close();
            XmlReader read2 = XmlReader.Create(label1.Text, settings);
            temp = 0;
            string D = "foo";
            bar++;


   
            //join and conquer
            while (read2.Read())
            {
                wayNode = roots;
                templist = new List<string[]>();

                if (read2.Name == "way" && read.NodeType!=XmlNodeType.EndElement)
                {
                    first = true;
                    string[] cord = { };
                    char l = 'c';
                   
                    XmlReader r = read2.ReadSubtree();
                    while (r.Read())
                    {


                        if (r.Name == "nd")
                        {
                            
                            cord = search(r.GetAttribute("ref"));

                            if (first == true)
                            {
                                D = (cord[0]+"00").Substring(0,4).Replace(".","")+(cord[1]+"00").Substring(0,4).Replace(".","");

                                templist.Add(cord);

                                first = false;
                            }
                            else
                            {
                                templist.Add(cord);
                            }
                         
                           
                            

                        }
                        else if (r.Name == "tag" && r.GetAttribute("k")=="highway")
                        {
                            string[] type = { "highway", r.GetAttribute("v") };
                            templist.Add( type );
                            
                        }
                        


                    }

                    for (int i = 0; i < D.Length; i++)
                    {
                       l=char.Parse( D.Substring(i, 1));
                       if(wayNode.wayNodes[l-'0']==null)
                        {
                            wayNode.wayNodes[l- '0'] = new WayNode();
                            
                            wayNode = wayNode.wayNodes[l - '0'];
                            
                        }
                        else
                        {
                            wayNode = wayNode.wayNodes[l - '0'];

                        }

                    }

                    if (wayNode.id == D)
                    {
                        wayNode.listoflists.Add(templist);
                    }
                    else
                    {
                        wayNode.pathed = path;
                        wayNode.id = D;
                        wayNode.listoflists.Add(templist);
                    }


                    countxml++;
                    if (temp + 10000 < countxml)
                    {
                       Thread.Sleep(1 / 1000);
                        backgroundWorker1.ReportProgress(countxml);
                        temp = countxml;
                    }
                }
            }
            bar++;





            for (int i = 0; i < 10; i++)
            {

                if (roots.wayNodes[i] != null)
                {
                    
                    roots.wayNodes[i].check();
                }
                temporalis += 10;
            }













            
            watch.Stop();
            read.Close();

            backgroundWorker1.ReportProgress(countwrite);
                
            MessageBox.Show("Map complete"+"\nElapsed time "+watch.ElapsedMilliseconds.ToString()); 
            
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (bar == 0) { label2.Text = e.ProgressPercentage.ToString(); }
            if (bar == 1) { label11.Text = e.ProgressPercentage.ToString(); }
            if (bar == 2) { label5.Text = e.ProgressPercentage.ToString()+"%"; }
                      
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }
    }

    public class Node
    {
        public bool wordEnd;
        public Node[] nextAll;
        public string lat;
        public string lon;

        public Node()
        {
            nextAll = new Node[10];
            wordEnd = false;
        }

    }

    

    public class WayNode
    {
        public string id;
        public string pathed;
        public string type;
        public WayNode[] wayNodes;
        public List<List<string[]>> listoflists;
        public WayNode()
        {
            wayNodes = new WayNode[10];
            id = "empty";
            listoflists = new List<List<string[]>>();
        }
        public void check()
        {
            for (int i = 0; i < 10; i++)
            {
                    
                if (wayNodes[i]?.listoflists.Count>0)
                {
                    XmlWriter write = XmlWriter.Create(wayNodes[i].pathed + @"\" + "X" + wayNodes[i].id + ".osm");
                    
                        write.WriteStartDocument();
                        write.WriteStartElement("coordinates");
                        for (int a = 0; a < wayNodes[i].listoflists.Count; a++)
                        {
                        write.WriteString("\n");
                        write.WriteStartElement("way");
                            for (int b = 0; b < wayNodes[i].listoflists[a].Count; b++)
                            {
                                write.WriteString("\n");
                            if (wayNodes[i].listoflists[a][b][0] == "highway")
                            {

                                
                                write.WriteStartElement("Z");
                                write.WriteAttributeString("highway", wayNodes[i].listoflists[a][b][1]);
                                write.WriteEndElement();
                            }
                            else
                            {
                                write.WriteStartElement("C" + b.ToString());
                                write.WriteAttributeString("lat", wayNodes[i].listoflists[a][b][0]);
                                write.WriteAttributeString("lon", wayNodes[i].listoflists[a][b][1]);
                                write.WriteEndElement();
                            }
                            }
                        write.WriteString("\n");
                        write.WriteEndElement();
                        }
                        write.WriteString("\n");
                        write.WriteEndElement();
                        write.WriteEndDocument();
                        write.Flush();
                        write.Close();
                    
                }
                
                wayNodes[i]?.check();
            }
           
        }

    }

}
