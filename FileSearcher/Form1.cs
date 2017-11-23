using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace FileSearcher
{
    public partial class Form1 : Form
        
    {
        public SynchronizationContext uiContext;
        public static int numberFound = 0;
        public static Boolean cancelToken = false;
        public static bool working = false;
        public static Boolean contains = false;

        public Form1()
        {
            InitializeComponent();
            Type type = listView1.GetType();
            PropertyInfo propertyInfo = type.GetProperty("DoubleBuffered", BindingFlags.NonPublic | BindingFlags.Instance);
            propertyInfo.SetValue(listView1, true, null);
            uiContext = SynchronizationContext.Current;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.FillData();


        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            FolderBrowserDialog dlg = new FolderBrowserDialog();
         
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox3.Text = dlg.SelectedPath;
            }
        }


        // Filling the textbox with default text
      private void FillData()
        {
            this.checkBox1.Checked = true;
            this.textBox1.Text = "*.*";
            this.textBox2.Text = null;
            this.textBox3.Text = "C:\\";

        }

        private void StartButton_Click(object sender, EventArgs e)
        {
           
            cancelToken = false;
            textBox4.Clear(); 
            numberFound = 0;
            SearchParams userParams = new SearchParams(this.textBox1.Text, this.textBox3.Text, this.checkBox1.Checked, this.textBox2.Text);
            listView1.Items.Clear();
            DisableButtons();
            DirectoryInfo dirInfo = new DirectoryInfo(userParams.SearchPath);
            
            Thread th = new Thread(delegate()
                {
                    EnumerateDirs(dirInfo, userParams,userParams.SearchWord);
                });
            th.IsBackground = true;
            th.Start();
            if (cancelToken)
                th.Abort();

           
            
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            
                cancelToken = true;
                EnableButtons();
          
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            FillData();
            numberFound = 0;
            textBox4.Clear();
            listView1.Items.Clear();
            textBox4.Text = numberFound.ToString() + " files found";
        }


        public void DisableButtons()
        {
            this.label1.Enabled = false;
            this.label2.Enabled = false;
            this.label3.Enabled = false;
            this.textBox1.Enabled = false;
            this.textBox2.Enabled = false;
            this.textBox3.Enabled = false;
            this.textBox4.Enabled = false;
            this.StartButton.Enabled = false;
            this.ResetButton.Enabled = false;
            this.StopButton.Enabled = true;
            this.button4.Enabled = false;
        }

        public void EnableButtons()
        {
            this.label1.Enabled = true;
            this.label2.Enabled = true;
            this.label3.Enabled = true;
            this.textBox1.Enabled = true;
            this.textBox2.Enabled = true;
            this.textBox3.Enabled = true;
            this.textBox4.Enabled = true;
            this.StartButton.Enabled = true;
            this.ResetButton.Enabled = true;
            this.StopButton.Enabled = false;
            this.button4.Enabled = true;
        }





        private void EnumerateDirs(DirectoryInfo dirInfo, SearchParams pars,string findWord)
        
        {

            bool lookForWord = String.IsNullOrEmpty(findWord);
            working = true;
            if (!cancelToken)
            {
               
                try
                {
                    if ((pars.SearchPath.Length >= 3) && (Directory.Exists(pars.SearchPath)))
                    {
                        FileInfo[] infos = dirInfo.GetFiles(pars.FileName);
                        if (infos.Length > 0)

                        {
                            numberFound += infos.Length;
                            contains = true;

                            foreach (FileInfo info in infos)
                            {
                                if (cancelToken)
                                {
                                    working = false;
                                    Thread.CurrentThread.Abort();

                                }
                                else
                                {
                                    if (lookForWord)
                                    {
                                        byte[] byteStr = Encoding.Default.GetBytes(findWord);
                                        FileContainsBytes(dirInfo.FullName,byteStr);

                                    }
                                    uiContext.Send(d => CreateResultsListItem(info), null);
                                }
                            }
                        }

                        if (pars.SubDirChecked)


                        {
                            var dirs = dirInfo.GetDirectories();
                            foreach (var dir in dirs)
                            {
                                if (cancelToken)
                                {
                                    working = false;
                                    Thread.CurrentThread.Abort();

                                }
                                
                                else
                                {//recursion
                                  EnumerateDirs(dir, pars, findWord);
                                   
                                }


                            }

                        }
                    }
                    else
                    {

                       MessageBox.Show("The directory\r\n" + pars.SearchPath + "\r\ndoes not exist.");
                    }
                }
                
                catch (System.IO.DirectoryNotFoundException e)
                {
                    Console.WriteLine(e.Message);
                }

                catch (Exception exception)
                {

                }
            }
            working = false;
            if (!contains)
                MessageBox.Show("No files match the search criteria!");
            //uiContext.Send(d => EnableButtons(), null);
        }

        private static bool FileContainsBytes(String path, Byte[] compare)
        {
            Boolean contains = false;

            Int32 blockSize = 4096;
            if ((compare.Length >= 1) && (compare.Length <= blockSize))
            {
                Byte[] block = new Byte[compare.Length - 1 + blockSize];

                //try
                {
                    FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                    // Read the first bytes from the file into "block":
                    Int32 bytesRead = fs.Read(block, 0, block.Length);

                    do
                    {
                        // Search "block" for the sequence "compare":
                        Int32 endPos = bytesRead - compare.Length + 1;
                        for (Int32 i = 0; i < endPos; i++)
                        {
                            // Read "compare.Length" bytes at position "i" from the buffer,
                            // and compare them with "compare":
                            Int32 j;
                            for (j = 0; j < compare.Length; j++)
                            {
                                if (block[i + j] != compare[j])
                                {
                                    break;
                                }
                            }

                            if (j == compare.Length)
                            {
                                // "block" contains the sequence "compare":
                                contains = true;
                                break;
                            }
                        }

                        // Search completed?
                        if (contains || (fs.Position >= fs.Length))
                        {
                            return contains;
                        }
                        else
                        {
                            // Copy the last "compare.Length - 1" bytes to the beginning of "block":
                            for (Int32 i = 0; i < (compare.Length - 1); i++)
                            {
                                block[i] = block[blockSize + i];
                            }

                            // Read the next "blockSize" bytes into "block":
                            bytesRead = compare.Length - 1 + fs.Read(block, compare.Length - 1, blockSize);
                        }
                    }
                    while (!cancelToken);

                    fs.Close();
                    
                }
                /*catch (Exception)
                {
                    
                }*/
            }

            return contains;
        }

        private void CreateResultsListItem(FileInfo info)
        {
            // Create a new item and set its text:
            ListViewItem lvi = new ListViewItem();
            lvi.Text = info.Name;

            ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem();
            lvsi.Text = info.DirectoryName;
            lvi.SubItems.Add(lvsi);


            lvsi = new ListViewItem.ListViewSubItem();
            lvi.SubItems.Add(((info.Length / 1000).ToString()) + ' ' + "KB");

            lvsi = new ListViewItem.ListViewSubItem();
            lvsi.Text = info.LastWriteTime.ToShortDateString() + " " + info.LastWriteTime.ToShortTimeString();
            lvi.SubItems.Add(lvsi);

            // Set the text that is shown when the mouse cursor
            // rests over the item (The "ShowItemToolTips" property of the ListView
            // must be true to show the text.):
            lvi.ToolTipText = info.FullName;

            // Add the new item to the list:
           listView1.Items.Add(lvi);
            listView1.Update();
           textBox4.Text = numberFound.ToString() + " files found";
           textBox4.Update();
        }
    }
}
