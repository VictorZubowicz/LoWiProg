using LoWiProg.FSControls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace LoWiProg
{
    public partial class Form1 : Form
    {
        //List<Block_Estado> ListEstados = new List<Block_Estado>();
        List<String> NombresDI = new List<string> { "IN1", "IN2", "IN3", "IN4", "IN5", "IN6" };
        List<String> NombresDO = new List<string> { "OUT1", "OUT2" };

        //const int LEN_ROWS_PROG = 16;
        //const int LEN_COLS_PROG = 16;
        //int[,] arrayProg = new int[LEN_COLS_PROG, LEN_ROWS_PROG];
        bool fg_EditandoNodo = false;

        Graph_Estado FS_EstadoGrafEnEdicion = null;
        int posYGraphEstados = 0;
        int posXGraphEstados = 0;

        TableLayoutPanelCellPosition position = new TableLayoutPanelCellPosition(0, 0);
        int indexListEstadoSel = 0;

        const int estadoDragDropTRUE = 1;
        const int estadoDragDropFALSE = 0;
        public int estadoDragDrop = -1;

        public Form1()
        {
            InitializeComponent();
            this.Size = new Size(1000, 1500);

            tableLayoutPanelUserProg.AllowDrop = true;
            this.tableLayoutPanelUserProg.DragDrop += new System.Windows.Forms.DragEventHandler(this.tableLayoutPanelUserProg_DragDrop);
            this.tableLayoutPanelUserProg.DragEnter += new System.Windows.Forms.DragEventHandler(this.tableLayoutPanelUserProg_DragEnter);



            matrizEstados.listNodes = JsonFiles.JsonFunctions.NodesRead();
            matrizEstados.cuentaEstados = matrizEstados.listNodes.Count();

            if (matrizEstados.cuentaEstados == 0) IniciarGrafEstados();

            ActualizaGrafTablaEstados();    //carga la tablaGrafica de estados con la Lista ListNodes

        }

        async static void GetRequest(string url)
        {
            using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
            {
                using (HttpResponseMessage response = await client.GetAsync(url))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();

                        Console.WriteLine(mycontent);
                    }
                }
            }

        }

        async static void PostRequest(string url)
        {

            IEnumerable<KeyValuePair<String, String>> queries = new List<KeyValuePair<String, String>>()
            {
                new KeyValuePair<String, String>("query1", "jamal"),
                new KeyValuePair<String, String>("query2", "hussain"),
            };

            HttpContent q = new FormUrlEncodedContent(queries);

            using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
            {
                using (HttpResponseMessage response = await client.PostAsync(url, q))
                {
                    using (HttpContent content = response.Content)
                    {
                        string mycontent = await content.ReadAsStringAsync();
                        HttpContentHeaders header = content.Headers;
                        Console.WriteLine(mycontent);
                    }
                }
            }

        }

        /// <summary>
        /// ////////////////////// DIGITAL IN////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxNameDI(object sender, EventArgs e)
        {
            TableLayoutPanelCellPosition position;
            TextBox txtBoxitem = (TextBox)sender;
            position = tableLayoutPanelDI.GetPositionFromControl(txtBoxitem.Parent);


            Graph_Estado item = (Graph_Estado)tableLayoutPanelDI.GetControlFromPosition(position.Column, 0);
            item.Text = txtBoxitem.Text;
            tableLayoutPanelDI.Controls.Add(item);

            NombresDI[position.Column] = txtBoxitem.Text;


        }
        /// <summary>
        /// ////////////////////// DIGITAL DO////////////////////////////////////////
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtBoxNameDO(object sender, EventArgs e)
        {
            TableLayoutPanelCellPosition position;
            TextBox txtBoxitem = (TextBox)sender;
            position = tableLayoutPanelDO.GetPositionFromControl(txtBoxitem.Parent);


            Graph_Estado item = (Graph_Estado)tableLayoutPanelDO.GetControlFromPosition(position.Column, 0);
            item.Text = txtBoxitem.Text;
            tableLayoutPanelDO.Controls.Add(item);

            NombresDO[position.Column] = txtBoxitem.Text;


        }

        private void butConfGuardar_Click(object sender, EventArgs e)
        {
            cboRegEdicAccion.Items.Clear();
            foreach (string sN in NombresDI) cboRegEdicAccion.Items.Add(sN);
            foreach (string sN in NombresDO) cboRegEdicAccion.Items.Add(sN);
        }




        #region PANEL_PROGRAMA

        /// <summary>
        /// ///////////////////////////// USER PROG /////////////////////////////////////////////////////////////////////////////////////////////////
        /// </summary>
        private void IniciarGrafEstados()
        {
            AgregarEstado(null, false);
            loadgEdit((object)tableLayoutPanelUserProg.GetControlFromPosition(0, 0));
        }


        /// <summary>
        /// AgregarEstado: Agrega nuevo estado en la lista, tomando el GraphParent y luego actualiza la Grafica, 
        /// devuelve: el nuevo estado
        /// </summary>
        Node_Estado AgregarEstado(Graph_Estado GraphParent, bool TypeJmp)
        {
            Node_Estado nodoNuevo;

            // si es null es porque es el 1ro
            if (GraphParent == null) { nodoNuevo = matrizEstados.AgregarNodolistaProg(null, TypeJmp); }
            else { nodoNuevo = matrizEstados.AgregarNodolistaProg(GraphParent.inode, TypeJmp); }
            ActualizaGrafTablaEstados();

            return nodoNuevo;
        }


        /// <summary>
        /// ActualizaGrafTablaEstados: Actualiza los Controles graficos
        /// TomarImagenSegunEstadoJmp: iEstado tiene propiedades jmpTrue/jmpFalse que definen tipo de grafico
        /// </summary>
        public void ActualizaGrafTablaEstados()
        {
            Graph_Estado grafEstadoCur = null;

            int x, y = 0;

            tableLayoutPanelUserProg.Controls.Clear();


            for (int i = 0; i < matrizEstados.listNodes.Count; i++)
            {
                x = matrizEstados.listNodes[i].pos.X;
                y = matrizEstados.listNodes[i].pos.Y;
                grafEstadoCur = (Graph_Estado)tableLayoutPanelUserProg.GetControlFromPosition(x, y);

                if (grafEstadoCur == null) // Agrego nuevo
                {
                    grafEstadoCur = new Graph_Estado();//global::LoWiProg.Properties.Resources.BackLineTrue);
                    grafEstadoCur.Image = TomarImagenSegunEstadoJmp(i);
                    grafEstadoCur.Text = matrizEstados.listNodes[i].Text;
                    grafEstadoCur.Click += new System.EventHandler(this.fS_Estado_Click);
                    grafEstadoCur.MouseClick += new System.Windows.Forms.MouseEventHandler(this.block_Estado_MouseClick);
                    grafEstadoCur.KeyDown += new System.Windows.Forms.KeyEventHandler(this.fS_Estado_KeyDown);
                    grafEstadoCur.MouseDown += graph_Estado_MouseDown;
                    grafEstadoCur.inode = matrizEstados.listNodes[i];

                    tableLayoutPanelUserProg.Controls.Add(grafEstadoCur, x, y);
                }
                else if (matrizEstados.listNodes[i] != grafEstadoCur.inode)
                {
                    tableLayoutPanelUserProg.Controls.Remove(grafEstadoCur);
                    grafEstadoCur.Image = TomarImagenSegunEstadoJmp(i);
                    grafEstadoCur.inode = matrizEstados.listNodes[i];
                    grafEstadoCur.Text = matrizEstados.listNodes[i].Text;

                    tableLayoutPanelUserProg.Controls.Add(grafEstadoCur, x, y);

                }

                grafEstadoCur.TabIndex = y * matrizEstados.LEN_COLS_PROG + x;
            }



        }
        private Image TomarImagenSegunEstadoJmp(int iList)
        {

            Image image; //global::LoWiProg.Properties.Resources.BackEstadoIni;

            Node_Estado iNodeEstado;

            //            int IndexList = matrizEstados.matrizProg[x, y];
            iNodeEstado = matrizEstados.listNodes[iList];

            if (iNodeEstado.EstadoJmpTrue != null)
            {
                if (iNodeEstado.EstadoJmpFalse != null)
                    image = global::LoWiProg.Properties.Resources.BackEstadoConNext;
                else
                    image = global::LoWiProg.Properties.Resources.BackEstadoComTrue;
            }
            else
            {
                if (iNodeEstado.EstadoJmpFalse != null)
                    image = global::LoWiProg.Properties.Resources.BackEstadoConFalse;
                else
                    image = global::LoWiProg.Properties.Resources.BackEstadoIni;
            }

            return image;
        }

        /// <summary>
        /// KEYDOWN DELETE
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fS_Estado_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                e.Handled = true;
                FSControls.Graph_Estado sEstado = (FSControls.Graph_Estado)sender;

                //limpiar_arrayProg();
                TableLayoutPanelCellPosition position = tableLayoutPanelUserProg.GetPositionFromControl(sEstado.Parent);

                matrizEstados.EliminarEstado(sEstado.inode);
                tableLayoutPanelUserProg.Controls.Remove(sEstado);
                ActualizaGrafTablaEstados();


                Graph_Estado item = (Graph_Estado)tableLayoutPanelUserProg.GetControlFromPosition(0, 0);
                loadgEdit(item);
            }

        }
        private void block_Estado_MouseClick(object sender, MouseEventArgs e)
        {
            loadgEdit(sender);
            if (e.X > 135 && e.Y > 35 && e.Y < 55)
            {
                //                AgregarEstado((Graph_Estado)sender, false);
            }
            if (e.Y > 70 && e.X > 85 && e.X < 110)
            {
                //                AgregarEstado((Graph_Estado)sender, true);
            }
        }

        private void tableLayoutPanelUserProg_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void tableLayoutPanelUserProg_DragDrop(object sender, DragEventArgs e)
        {
            // no anda Graph_Estado estado = (Graph_Estado)sender;
            //position = tableLayoutPanelDI.GetPositionFromControl(estado.Parent);

            var DragEstado = (Graph_Estado)e.Data.GetData(typeof(Graph_Estado));
            var DragEstadoNew = (Graph_Estado)new Graph_Estado();
            ///DragEstado.Location = g.PointToClient(new Point(e.X, e.Y));
            //DragEstado.Parent = g;
            //DragEstado.BringToFront();
            var tlp = (TableLayoutPanel)sender;
            DragEstadoNew.Location = tlp.PointToClient(new Point(e.X, e.Y));
            int despX = DragEstadoNew.Location.X - DragEstado.Location.X;
            int despY = DragEstadoNew.Location.Y - DragEstado.Location.Y;


            if (despX > 150 && despX < 400 && despY > 20 && despY < 75 && estadoDragDrop == estadoDragDropFALSE)
            {
                AgregarEstado(FS_EstadoGrafEnEdicion, false);

            }
            if (despX > 50 && despX < 200 && despY > 50 && despY < 250 && estadoDragDrop == estadoDragDropTRUE)
            {
                AgregarEstado(FS_EstadoGrafEnEdicion, true);
            }

        }

        private void graph_Estado_MouseDown(object sender, MouseEventArgs e)
        {

            loadgEdit(sender);

            // TRUE
            if (e.X > 135 && e.Y > 35 && e.Y < 55)
            {
                Graph_Estado g = (Graph_Estado)sender;
                g.DoDragDrop(g, DragDropEffects.Move);
                estadoDragDrop = estadoDragDropFALSE;
            }

            else if (e.Y > 70 && e.X > 90 && e.X < 110)
            {
                Graph_Estado g = (Graph_Estado)sender;
                g.DoDragDrop(g, DragDropEffects.Move);
                estadoDragDrop = estadoDragDropTRUE;
            }
            else estadoDragDrop = -1;

        }
        public void fS_Estado_Click(object sender, EventArgs e)
        {
            loadgEdit(sender);
        }

        private void lblStrCOS_TextChanged(object sender, EventArgs e)
        {
            FS_EstadoGrafEnEdicion.inode.COS = lblStrCOS.Text;
        }

        #endregion PANEL_PROGRAMA


        #region PANEL_EDICION
        /// <summary>
        /// ACCIONES DE EDICION
        /// </summary>  /////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Carga el sector de edicion, segun el EstadoGrafico Seleccionado
        /// </summary>
        /// <param name="sender"></param>
        private void loadgEdit(Object sender)
        {

            if (sender == null) return;


            FS_EstadoGrafEnEdicion = (Graph_Estado)sender;

            foreach (Graph_Estado g in tableLayoutPanelUserProg.Controls)
                if (FS_EstadoGrafEnEdicion == g) g.BackColor = Color.Aqua;
                else g.BackColor = Color.White;

            indexListEstadoSel = FS_EstadoGrafEnEdicion.inode.IndexListEstado;
            lblIndexEstado.Text = indexListEstadoSel.ToString();

            position = tableLayoutPanelUserProg.GetPositionFromControl(FS_EstadoGrafEnEdicion);
            lblPosition.Text = position.ToString();
            //public FS_EstadoEnEdicion
            posXGraphEstados = position.Column;
            posYGraphEstados = position.Row;
            lblPosition.Text = posXGraphEstados.ToString() + "," + posYGraphEstados.ToString();
            if (FS_EstadoGrafEnEdicion.inode.Parent != null) lblParent.Text = FS_EstadoGrafEnEdicion.inode.Parent.Text.ToString();
            else lblParent.Text = "Inicio";
            //gBoxEdicionEstado.Visible = true;
            txtBoxEditName.Text = FS_EstadoGrafEnEdicion.Text;

            lblStrCOS.Text = FS_EstadoGrafEnEdicion.inode.COS;
            cboCondCOS.Text = ""; cboRegEditCOS.Text = ""; txtBoxValCOS.Text = "";


            if (FS_EstadoGrafEnEdicion.inode == null) return;
            lblOffsetY.Text = FS_EstadoGrafEnEdicion.inode.offsetY.ToString();
            if (FS_EstadoGrafEnEdicion.inode.EstadoJmpTrue != null) cboBoxEditStCondTrue.Text = FS_EstadoGrafEnEdicion.inode.EstadoJmpTrue.Text.ToString();
            else cboBoxEditStCondTrue.Text = "0";
            if (FS_EstadoGrafEnEdicion.inode.EstadoJmpFalse != null) TxtEditStCondFalse.Text = FS_EstadoGrafEnEdicion.inode.EstadoJmpFalse.Text.ToString();
            else TxtEditStCondFalse.Text = "0";
            //ListEstados.ForEach(x => cboBoxEditStCondTrue.Items.Add(x.Text));

            butAgregaAccion.Enabled = true;
            listBoxAcciones.Items.Clear();
            if (FS_EstadoGrafEnEdicion.inode.Asignaciones.Count > 0)
            {
                butQuitarAccion.Enabled = true;
                foreach (string item in FS_EstadoGrafEnEdicion.inode.Asignaciones)
                {
                    listBoxAcciones.Items.Add(item);
                }
            }
            else butQuitarAccion.Enabled = false;

        }
        private void butAgregaAccion_Click(object sender, EventArgs e)
        {
            String Accion = "";
            if (FS_EstadoGrafEnEdicion == null) return;

            if (cboRegEdicAccion.SelectedIndex >= 0 && cboEdicValReg.SelectedIndex >= 0)
            {
                lblAction.Text = cboRegEdicAccion.Text + "=" + cboEdicValReg.Text;
                Accion = cboRegEdicAccion.SelectedIndex.ToString() + "=" + cboEdicValReg.SelectedIndex.ToString();
                listBoxAcciones.Items.Add(Accion);
                FS_EstadoGrafEnEdicion.inode.Asignaciones.Add(Accion);
            }
            else MessageBox.Show("Ingrese Accion Formato REGISTRO=ON/OFF", "ACCION");

        }
        private void butQuitarAccion_Click(object sender, EventArgs e)
        {
            if (listBoxAcciones.SelectedIndex >= 0)
            {
                FS_EstadoGrafEnEdicion.inode.Asignaciones.RemoveAt(listBoxAcciones.SelectedIndex);
                listBoxAcciones.Items.RemoveAt(listBoxAcciones.SelectedIndex);
            }
            else MessageBox.Show("Seleccione el Item a eliminar de la lista", "Quitar Accion");

            if (listBoxAcciones.Items.Count == 0) butQuitarAccion.Enabled = false;
        }
        private void cboBoxEditStCondTrue_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (cboBoxEditStCondTrue.Text == "NUEVO")
            {
                AgregarEstado((Graph_Estado)sender, true);
            }
        }
        private void cboEdicRegistros_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void butEditCondAddFalse_Click(object sender, EventArgs e)
        {
            // Nuevo item Jump False
            AgregarEstado((Graph_Estado)sender, false);
        }
        private void txtBoxEditName_TextChanged(object sender, EventArgs e)
        {
            FS_EstadoGrafEnEdicion.Text = txtBoxEditName.Text;
            tableLayoutPanelUserProg.Controls.Add(FS_EstadoGrafEnEdicion);
            matrizEstados.listNodes[FS_EstadoGrafEnEdicion.inode.IndexListEstado].Text = txtBoxEditName.Text;

        }
        private void ActualizaCOS(object sender, EventArgs e)
        {
            string compara = " ", cond;
            if (cboCondCOS.Text.Length < 3) return;
            cond = cboCondCOS.Text.Substring(0, 2);
            if (cond == "if")
                compara = cboCondCOS.Text.Substring(2);

            lblStrCOS.Text = cond + " " + cboRegEditCOS.Text + " " + compara + " " + txtBoxValCOS.Text;

        }

        private void butAceptCOS_Click(object sender, EventArgs e)
        {
            FS_EstadoGrafEnEdicion.inode.COS = lblStrCOS.Text;
        }

        #endregion PANEL_EDICION


        #region PANEL_WIFI

        public class JsonWiFi
        {
            public String iWiFi { get; set; }    // IMPORTANTE PONER get set
            public String NameWiFi { get; set; }    // IMPORTANTE PONER get set
        }

        private string Cliente_GET(string endpoint)
        {
            using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
            {
                string json = "";
                try
                {
                    var pathendpoint = new Uri("http://192.168.1.200/" + endpoint);
                    // GET
                    var result = client.GetAsync(pathendpoint).Result;
                    json = result.Content.ReadAsStringAsync().Result;
                }
                catch
                {
                    if (lblResponseHttp.Text == "") lblResponseHttp.Text = "Error Conexion";
                    else lblResponseHttp.Text = lblResponseHttp.Text + ".";
                }
                return json;
            }
        }

        /// <summary>
        /// POST
        //   var values = new Dictionary<string, string> {
        //      { "thing1", "hello" },
        //      { "thing2", "world" }   };
        //  var content = new FormUrlEncodedContent(values);
        //  var response = await client.PostAsync("http://www.example.com/recepticle.aspx", content);
        //  var responseString = await response.Content.ReadAsStringAsync();
        //  GET
        //  var responseString = await client.GetStringAsync("http://www.example.com/recepticle.aspx");
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private string Cliente_POST(string endpoint, IEnumerable<KeyValuePair<String, String>> queries)
        {
            using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
            {
                // POST
                var pathendpoint = new Uri("http://192.168.1.200/" + endpoint);
                HttpContent q = new FormUrlEncodedContent(queries);
                var result = client.PostAsync(pathendpoint, q).Result.Content.ReadAsStringAsync().Result;
                return (result);
            }
        }

        private void butScanWiFi_Click(object sender, EventArgs e)
        {
            using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
            {

                var endpoint = new Uri("http://192.168.1.200/escanear_wifi");
                // GET
                var result = client.GetAsync(endpoint).Result;
                var json = result.Content.ReadAsStringAsync().Result;

                lstBoxWiFi.Items.Clear();
                List<String> ListNameWiFi;
                ListNameWiFi = JsonFiles.JsonVZ.ObtenerListaValores(json);
                for (int i = 1; i < ListNameWiFi.Count; i++)
                {
                    lstBoxWiFi.Items.Add(ListNameWiFi[i]);
                }
            }
        }

        private void butConWiFi_Click(object sender, EventArgs e)
        {
            string selwifi = lstBoxWiFi.SelectedItem.ToString(); //Aca recibo el wifi que selecciono el cliente
            string passwifi = txtBoxPass.Text; //Aca recibo el wifi que selecciono el cliente

            using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
            {
                IEnumerable<KeyValuePair<String, String>> queries = new List<KeyValuePair<String, String>>()
                {
                   new KeyValuePair<String, String>("ssid", selwifi ), //"WiFI Victor 2.4GHz"),
                   new KeyValuePair<String, String>("pass", passwifi), //"232317717"),
                };

                txtWiFi.Text = Cliente_POST("guardar_wifi", queries);

            }

        }

        private void lstBoxWiFi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstBoxWiFi.SelectedIndex >= 0)
                butConWiFi.Enabled = true;
            else butConWiFi.Enabled = false;
        }

        private void butMAC_Click(object sender, EventArgs e)
        {
            string jsonGet = Cliente_GET("mac");
            string ValorStr = JsonFiles.JsonVZ.ObtenerValorStringIndex(jsonGet, 0);
            lblMAC.Text = ValorStr;
        }

        /// <summary>
        /// timerTestEstado hace un GET al micro del estado
        /// </summary>
        /*private void timerTestEstado_Tick(object sender, EventArgs e)
        {
            string jsonGet = Cliente_GET("status");
            string ValorStr = JsonFiles.JsonVZ.ObtenerValorStringIndex(jsonGet, 0);
            lblStatus.Text = ValorStr;
        }
        */

        /// <summary>
        ///  Envia el programa al micro mediante un POST 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void butPostProg_Click(object sender, EventArgs e)
        {

            string fileName = "EstadosProg.json";
            try
            {
                string type_prg = "prog";
                // txtBoxPassProg.Text; //Aca envio el password para que el programa se cargue

                string load_prg = File.ReadAllText(fileName);    ////Aca envio el programa 

                using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
                {
                    IEnumerable<KeyValuePair<String, String>> queries = new List<KeyValuePair<String, String>>()
                    {
                       new KeyValuePair<String, String>("type", type_prg),
                       new KeyValuePair<String, String>("load_prg", load_prg),
                    };

                    lblResponseHttp.Text = Cliente_POST("set_prog", queries);

                }

            }
            catch (Exception ex)
            {
                Exception excep = ex;
                lblResponseHttp.Text = ex.Message.ToString() + ". Verifique que esta en conectado al AP LoWiCom";
            }
        }
        private void butPostActions_Click(object sender, EventArgs e)
        {
            string type_prg = "asign";

            string fileName = "Asignations.json";
            try
            {
                string load_asign = File.ReadAllText(fileName);    ////Aca envio el programa 


                using (HttpClient client = new HttpClient())    //using es porque las respuestan son "dispose"
                {
                    IEnumerable<KeyValuePair<String, String>> queries = new List<KeyValuePair<String, String>>()
                    {
                       new KeyValuePair<String, String>("type", type_prg),
                       new KeyValuePair<String, String>("load_prg", load_asign),
                    };

                    lblResponseHttp.Text = Cliente_POST("set_prog", queries);

                }

            }
            catch (Exception ex)
            {
                Exception excep = ex;
                lblResponseHttp.Text = ex.Message.ToString() + ". Verifique que esta en conectado al AP LoWiCom";
            }

        }

        #endregion PANEL_WIFI

        #region JSON
        private void butShowJsonGraphFile_Click(object sender, EventArgs e)
        {
            //List<Node_Estado> list_Estados = new List<Node_Estado>();
            string rstr, beaytified;
            String fileName = "Estados.json";
            try
            {
                rstr = File.ReadAllText(fileName);
                JsonDocument document = JsonDocument.Parse(rstr);
                var stream = new MemoryStream();
                var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
                document.WriteTo(writer);
                writer.Flush();

                beaytified = Encoding.UTF8.GetString(stream.ToArray());
                txtBoxPrograma.Text = beaytified;


            }

            catch (Exception ex)
            {
                txtBoxPrograma.Text = ex.Message.ToString();
            }
        }
        private void butShowJsonFileProg_Click(object sender, EventArgs e)
        {
            //List<Node_Estado> list_Estados = new List<Node_Estado>();
            string rstr, beaytified;
            String fileName = "EstadosProg.json";
            try
            {
                rstr = File.ReadAllText(fileName);
                JsonDocument document = JsonDocument.Parse(rstr);
                var stream = new MemoryStream();
                var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
                document.WriteTo(writer);
                writer.Flush();

                beaytified = Encoding.UTF8.GetString(stream.ToArray());
                txtBoxPrograma.Text = beaytified;


            }

            catch (Exception ex)
            {
                txtBoxPrograma.Text = ex.Message.ToString();
            }
        }

        private void butShowJsonFileAsign_Click(object sender, EventArgs e)
        {
            //List<Node_Estado> list_Estados = new List<Node_Estado>();
            string rstr, beaytified;
            String fileName = "Asignations.json";
            try
            {
                rstr = File.ReadAllText(fileName);
                JsonDocument document = JsonDocument.Parse(rstr);
                var stream = new MemoryStream();
                var writer = new Utf8JsonWriter(stream, new JsonWriterOptions() { Indented = true });
                document.WriteTo(writer);
                writer.Flush();

                beaytified = Encoding.UTF8.GetString(stream.ToArray());
                txtBoxPrograma.Text = beaytified;


            }

            catch (Exception ex)
            {
                txtBoxPrograma.Text = ex.Message.ToString();
            }
        }


        #endregion JSON

        #region FORM
        private void butGuardarProg_Click(object sender, EventArgs e)
        {
            matrizEstados.GuardarMatrizEstados();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion FORM

    }
}
