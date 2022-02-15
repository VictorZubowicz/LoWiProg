using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

namespace LoWiProg.FSControls
{
    public class Graph_Estado : Button
    {
        public int offset = 0;
        public Node_Estado inode = null;

        public Graph_Estado()
        {
            this.MinimumSize = new Size(45, 22);
            this.Dock = DockStyle.Fill;
            this.Text = "->";
            this.BackColor = Color.White;
            this.TextAlign = ContentAlignment.TopLeft;
            this.FlatStyle = FlatStyle.Flat;
            this.FlatAppearance.BorderColor = Color.White;
            //if (image!=null) this.Image = image;
        }
    }

    public class Node_Estado
    {

        public String Text;

        private List<String> asignaciones;
        private Node_Estado estadoJmpTrue;
        private Node_Estado estadoJmpFalse;
        private int indexListEstado;
        private string cos;
        public int IndexListEstado { get => indexListEstado; set => indexListEstado = value; }
        public List<string> Asignaciones { get => asignaciones; set => asignaciones = value; }
        public Node_Estado EstadoJmpTrue { get => estadoJmpTrue; set => estadoJmpTrue = value; }
        public Node_Estado EstadoJmpFalse { get => estadoJmpFalse; set => estadoJmpFalse = value; }

        public String COS { get => cos; set => cos = value; } // String que continene la condicion de cambio de estado.



        public Node_Estado Parent;
        public Point pos;
        public int offsetY;   //usado para graficar cuadricula en forma ordenada 

        public Node_Estado()
        {
            this.Text = "->";
            this.EstadoJmpTrue = null;
            this.EstadoJmpFalse = null;
            this.offsetY = 0;
            Asignaciones = new List<String>();
            this.COS = "";

        }
        public List<String> FS_Estado_TomarListaAsignaciones()
        {
            return Asignaciones;
        }

        public void FS_Estado_LlenarListaAsignaciones(List<String> lstAcciones)
        {
            Asignaciones.Clear();
            foreach (String accion in lstAcciones)
            {
                Asignaciones.Add(accion);
            }
        }



    }
}
