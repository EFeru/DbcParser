using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DbcParserLib;

/* 
 * ------------------------------------
 * Author:  Emanuel Feru
 * Year:    2022
 * 
 * Copyright (C) Emanuel Feru
 * ------------------------------------
 */

namespace Demo
{
    public partial class Form1 : Form
    {
        public DbcParser dbc;
        public DataTable dtMessages = new DataTable();
        public DataTable dtSignals = new DataTable();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Initial dbc file
            string pathInit = Path.GetDirectoryName(Application.ExecutablePath);
            pathInit = Path.GetFullPath(Path.Combine(pathInit, @"..\..\..\..\"));   // go up 4 levels in folder
            pathInit += "tesla_can.dbc";
            if (File.Exists(pathInit))
            {
                LoadDbc(pathInit);
            }

            // Example for packing/unpacking a signal: 14 bits, Min: -61.92, Max: 101.91
            //Signal sig = new Signal();
            //sig.Length = 14;
            //sig.StartBit = 2;
            //sig.IsSigned = 1;
            //sig.ByteOrder = 1;
            //sig.Factor = 0.01F;
            //sig.Offset = 20F;
            //ulong TxMsg = dbc.TxSignalPack(-34.3, sig);
            //double val = dbc.RxSignalUnpack(TxMsg, sig);
        }

        private void buttonLoadDbc_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Title = "Open dbc file";
            openFileDialog.FileName = "";
            openFileDialog.Filter = "DBC File|*.dbc";
            openFileDialog.ShowDialog();

            if (openFileDialog.FileName != "")
            {
                LoadDbc(openFileDialog.FileName);
            }
        }

        public void LoadDbc(string filePath)
        {
            try
            {
                dbc = new DbcParser();
                dbc.ReadFromFile(filePath);
                textBoxPath.Text = filePath;
                PopulateView();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read dbc file!\n\n{ex}", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        public void PopulateView()
        {
            // Clear controls
            textBoxNodes.Text = "";
            dtMessages.Rows.Clear();
            dtMessages.Columns.Clear();
            dtSignals.Rows.Clear();
            dtSignals.Columns.Clear();

            // Assign DataSource
            dataGridViewMsg.DataSource = dtMessages;
            dataGridViewSig.DataSource = dtSignals;
            SetDoubleBuffer(dataGridViewMsg, true);
            SetDoubleBuffer(dataGridViewSig, true);

            // Nodes
            foreach (var node in dbc.Nodes)
            {
                textBoxNodes.Text += node + " ";
            }

            // Messages
            dtMessages.Columns.Add("ID");
            dtMessages.Columns.Add("Name");
            dtMessages.Columns.Add("DLC");
            dtMessages.Columns.Add("Transmitter");
            dtMessages.Columns.Add("CycleTime");
            foreach (var msg in dbc.Messages)
            {
                dtMessages.Rows.Add("0x" + msg.ID.ToString("X"), msg.Name, msg.DLC, msg.Transmitter, msg.CycleTime);
            }

            // Signals
            dtSignals.Columns.Add("ID");
            dtSignals.Columns.Add("Name");
            dtSignals.Columns.Add("StartBit");
            dtSignals.Columns.Add("Length");
            dtSignals.Columns.Add("ByteOrder");
            dtSignals.Columns.Add("IsSigned");
            dtSignals.Columns.Add("InitialValue");
            dtSignals.Columns.Add("Factor");
            dtSignals.Columns.Add("Offset");
            dtSignals.Columns.Add("Minimum");
            dtSignals.Columns.Add("Maximum");
            dtSignals.Columns.Add("Unit");
            dtSignals.Columns.Add("ValueTable");
            dtSignals.Columns.Add("Comment");
            foreach (var node in dbc.Nodes)
            {
                dtSignals.Columns.Add(node);
            }
            foreach (var msg in dbc.Messages)
            {
                foreach (var sig in msg.Signals)
                {
                    dtSignals.Rows.Add("0x" + sig.ID.ToString("X"), sig.Name, sig.StartBit, sig.Length, sig.ByteOrder, sig.IsSigned, sig.InitialValue, sig.Factor, sig.Offset, sig.Minimum, sig.Maximum, sig.Unit, sig.ValueTable, sig.Comment);

                    int rowIdx = dtSignals.Rows.Count - 1;
                    int colIdx = dtSignals.Columns.IndexOf(msg.Transmitter);
                    if (colIdx > -1)
                    {
                        dtSignals.Rows[rowIdx][colIdx] = "Tx";
                        dataGridViewSig.Rows[rowIdx].Cells[colIdx].Style.BackColor = Color.Yellow;
                    }
                    foreach (var rx in sig.Receiver)
                    {
                        colIdx = dtSignals.Columns.IndexOf(rx);
                        if (colIdx > -1)
                        {
                            dtSignals.Rows[rowIdx][colIdx] = "Rx";
                            dataGridViewSig.Rows[rowIdx].Cells[colIdx].Style.BackColor = Color.LimeGreen;
                        }
                    }
                }
            }
        }

        static void SetDoubleBuffer(Control dgv, bool DoubleBuffered)
        {
            typeof(Control).InvokeMember("DoubleBuffered", 
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetProperty, 
                null, dgv, new object[] { DoubleBuffered });
        }

    }
}
