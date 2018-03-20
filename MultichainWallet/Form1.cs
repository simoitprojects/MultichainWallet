using SimoItProjects;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MultichainWallet
{
    public partial class Form1 : Form
    {
        private string path;
        private Process deamon = null;
        private Dictionary<string, string> rpc_creds;
        private SimplestJsonRpcClient rpc_client;
        private AppSettings settings;

        private void LOG(string msg)
        {
            File.AppendAllText(path + @"\app.log", msg + Environment.NewLine);
        }

        public class AppSettings
        {


            //private bool saveOnClose = true;
            private string settings_path = "";
            //private string multichaind_path = "";
            private string chain_name = "";
            private string chain_ip = "";
            private string chain_port = "";
            private string rpcuser = "";
            private string rpcpassword = "";

            public AppSettings(string path)
            {
                settings_path = path + @"\settings.ini";
            }

            public void Save()
            {
                string[] lines = {
                    //"multichaind_path=" + multichaind_path,
                    "chain_name=" + chain_name,
                    "chain_ip=" + chain_ip,
                    "chain_port=" + chain_port,
                    "rpcuser=" + rpcuser,
                    "rpcpassword=" + rpcpassword
                };
                System.IO.StreamWriter file = new System.IO.StreamWriter(settings_path);
                foreach (string line in lines)
                {
                    file.WriteLine(line);
                }
                file.Close();
            }

            public void Load()
            {
                Dictionary<string, string>  settings_dict = new Dictionary<string, string>();
                System.IO.StreamReader file = new System.IO.StreamReader(settings_path);
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    //LOG("chain.conf line=" + line);
                    string[] words = line.Split('=');
                    if (words.Length > 1)
                    {
                        //LOG("add conf chain[" + words[0] + "]=" + words[1]);
                        settings_dict.Add(words[0], words[1]);
                    }
                }
                file.Close();
                if(settings_dict.Count >= 4)
                {
                    //this.multichaind_path = settings_dict["multichaind_path"];
                    this.chain_name = settings_dict["chain_name"];
                    this.chain_ip = settings_dict["chain_ip"];
                    this.chain_port = settings_dict["chain_port"];
                    this.rpcuser = settings_dict["rpcuser"];
                    this.rpcpassword = settings_dict["rpcpassword"];
                }
            }

            public bool Validate()
            {
                if (chain_name.Length > 0 && chain_ip.Length > 0 && chain_port.Length > 0)
                    return true;
                else
                    return false;
            }

            //[DescriptionAttribute("Path to the multichaind.exe file (official multichain windows package) defaults to app path"),
            //CategoryAttribute("Global Settings")]
            //public string MultichaindPath
            //{
            //    get { return multichaind_path; }
            //    set { multichaind_path = value; this.Save(); }
            //}

            [DescriptionAttribute("Name of your blockchain (ex. test)"),
            CategoryAttribute("Multichain Settings")]
            public string ChainName
            {
                get { return chain_name; }
                set { chain_name = value; this.Save(); }
            }
            [DescriptionAttribute("Your blockchain node ip (ex. 192.168.1.1)"),
            CategoryAttribute("Multichain Settings")]
            public string ChainIp
            {
                get { return chain_ip; }
                set { chain_ip = value; this.Save(); }
            }
            [DescriptionAttribute("Your blockchain node port (ex. 4001)"),
            CategoryAttribute("Multichain Settings")]
            public string ChainPort
            {
                get { return chain_port; }
                set {chain_port = value; this.Save(); }
            }
            [DescriptionAttribute("Your rpcuser name to access node (from multichain.conf)"),
            CategoryAttribute("Multichain Settings")]
            public string RpcUser
            {
                get { return rpcuser; }
                set { rpcuser = value; this.Save(); }
            }
            [DescriptionAttribute("Rpcuser password (from multichain.conf)"),
            CategoryAttribute("Multichain Settings")]
            public string Rpcpassword
            {
                get { return rpcpassword; }
                set { rpcpassword = value; this.Save(); }
            }
        }

        public Form1()
        {
            InitializeComponent();
        }



        private void GetRpcCredentials()
        {
            rpc_creds = new Dictionary<string, string>();
            System.IO.StreamReader file = new System.IO.StreamReader(path + @"\" + settings.ChainName + @"\multichain.conf");
            string line;
            while ((line = file.ReadLine()) != null)
            {
                LOG("chain.conf line=" + line);
                string[] words = line.Split('=');
                if (words.Length > 1)
                {
                    LOG("add conf chain[" + words[0] + "]=" + words[1]);
                    rpc_creds.Add(words[0], words[1]);
                }
            }
            file.Close();
        }

        private void StartDeamon()
        {
            string deamon_command = path + @"\multichaind.exe";
            string deamon_args = settings.ChainName + "@" + settings.ChainIp + ":" + settings.ChainPort + " -daemon -datadir=" + path + @"\ -port=5001 -rpcport=5000 -debug=mchn -debug=mchnminor -debug=net";
            LOG("Starting deamon: " + deamon_command + " " + deamon_args);
            deamon = Process.Start(deamon_command, deamon_args);
        }

        private void StartApp()
        {
            //            dataGridAssets.Columns.Add("Name", "Name");
            //            dataGridAssets.Columns.Add("Balance", "Balance");
            //            dataGridAssets.Rows.Add("asset1", 100);
            //            dataGridAssets.Rows.Add("asset2", 50);
            //            dataGridAssets.ClearSelection();
            //StatusLabel.Text = "CONNECTING";
            //StartDeamon();
            //toolStripProgressBar1.Visible = true;
            //toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            //Thread.Sleep(5000);
            //tabControl1.Visible = true;
            //toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            //toolStripProgressBar1.Value = 100;
            //toolStatusLabel.Text = "CONNECTED";
            //toolStripButton1.Text = "DISCONNECT";
            //GetRpcCredentials();

            //rpc_client = new SimplestJsonRpcClient("http://127.0.0.1:5000", rpc_creds["rpcuser"], rpc_creds["rpcpassword"]);
            rpc_client = new SimplestJsonRpcClient("http://" + settings.ChainIp + ":" + settings.ChainPort, settings.RpcUser , settings.Rpcpassword);
            rpc_client.Debug(true, path);

            LOG("Getting wallet address");
            GetAssets();
            GetAddresses();

        }

        //CHAIN OPERATIONS

        private async Task GetAssets()
        {
            string request_json = "{ \"method\":\"gettotalbalances\",\"params\":[],\"id\":1,\"chain_name\":\"" + settings.ChainName + "\"}";
            LOG("Sending request = " + request_json);
            dynamic response = await rpc_client.Post(request_json);
            LOG("got response");
            dataGridAssets.Rows.Clear();
            //dataGridAssets.Rows.Add("asset1", 100);
            //dataGridAssets.Rows.Add("asset2", 50);
            foreach (var asset in response.result)
            { 
                //LOG(asset.ToString());
                dataGridAssets.Rows.Add(asset.name, asset.qty);
            }
            dataGridAssets.ClearSelection();
        }


        private async Task GetAddresses()
        {
            string request_json = "{\"method\":\"getaddresses\",\"params\":[],\"id\":1,\"chain_name\":\"" + settings.ChainName + "\"}";
            LOG("Sending request = " + request_json);
            dynamic response = await rpc_client.Post(request_json);
            LOG("got response");
            dataGridAddresses.Rows.Clear();
            foreach (string address in response.result)
                dataGridAddresses.Rows.Add(address);
            dataGridAddresses.ClearSelection();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            path = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            settings = new AppSettings(path);
            try
            {
                settings.Load();
            }
            catch (FileNotFoundException)
            {
                MessageBox.Show("No settings file please fill all settings");
                //settings.MultichaindPath = path + @"\multichaind.exe";
                tabControl1.SelectedIndex = 3;
            }

            StartApp();
            //propertyGrid1.SelectedObject = settings;

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
//            dataGridAssets.Columns.Add("Name", "Name");
//            dataGridAssets.Columns.Add("Balance", "Balance");
//            dataGridAssets.Rows.Add("asset1", 100);
//            dataGridAssets.Rows.Add("asset2", 50);
//            dataGridAssets.ClearSelection();
            StatusLabel.Text = "CONNECTING";
            StartDeamon();
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            Thread.Sleep(5000);
            tabControl1.Visible = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar1.Value = 100;
            StatusLabel.Text = "CONNECTED";
            toolStripButton1.Enabled = false;
            //toolStripButton1.Text = "DISCONNECT";
            GetRpcCredentials();

            //rpc_client = new MultiChainClient("127.0.0.1", 5000, false, "multichainrpc", "D3bC84HWzVcvmbgd8Wc7Q26onZ1nvkuuuEUJA2sDzzYr", chain["name"]);            

            rpc_client = new SimplestJsonRpcClient("http://127.0.0.1:5000", rpc_creds["rpcuser"], rpc_creds["rpcpassword"]);
            rpc_client.Debug(true, path);

            LOG("Getting wallet address");
            GetAssets();
            GetAddresses();
            tabControl1.SelectedIndex = 0;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (deamon != null)
                deamon.CloseMainWindow();
        }

        private void dataGridAssets_SelectionChanged(object sender, EventArgs e)
        {
            DataGridViewRow row = dataGridAssets.SelectedRows[0];
            labelAssetSend.Text = row.Cells[0].Value.ToString();
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            //GetAssets();
            //GetAddresses();
        }
    }
}
