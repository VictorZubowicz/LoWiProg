
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LoWiProg.FSControls;
using LoWiProg.JsonFiles;

namespace LoWiProg
{
    class matrizEstados
    {
        public const int LEN_ROWS_PROG = 16;
        public const int LEN_COLS_PROG = 16;
        public static Graph_Estado GRAFeSTADOlINE = new Graph_Estado();//global::LoWiProg.Properties.Resources.BackLineTrue);
        //public static List<itemListEstado> ListEstados = new List<itemListEstado>();

        public static int cuentaEstados = 0;

        /// <summary>
        /// listNodes es una lista de Nodos con propiedades para ser cargadas a los estados graficos
        /// la idea de tener una lista de Nodos es para no tener una lista de Graph_Estados, ya que la primera es mas liviana.
        /// </summary>
        public static List<Node_Estado> listNodes = new List<Node_Estado>();
        public class Point
        {
            public int XLocation { get; set; }
            public int YLocation { get; set; }
        }

        private static Point ObtenerPosicion(Node_Estado estado)
        {
            Point posChild = new Point();

            posChild.XLocation = 0;
            posChild.YLocation = 0;

            while (estado != null)
            {
                if (estado.Parent != null)
                {
                    if (estado.Parent.EstadoJmpTrue == estado) posChild.YLocation++;
                    if (estado.Parent.EstadoJmpFalse == estado) posChild.XLocation++;
                }
                estado = estado.Parent;
            }
            return posChild;
        }

        internal static Node_Estado AgregarNodolistaProg(Node_Estado parent, bool jmpTrue)
        {
            //Point posParent = new Point();
            Node_Estado nwNodo = new Node_Estado();
            nwNodo.Text = "Estado" + cuentaEstados++;
            nwNodo.IndexListEstado = listNodes.Count;

            listNodes.Add(nwNodo);
            if (parent == null)
            {
                return nwNodo;
            }


            //ObtenerOffsetVertical(posM.XLocation, posM.YLocation);
            if (jmpTrue == true)
            {
                if (parent.EstadoJmpTrue != null)
                {   //si ya tenia jmpTrue estoy intercalando
                    nwNodo.EstadoJmpTrue = parent.EstadoJmpTrue;
                    //nuevo parent
                    parent.EstadoJmpTrue.Parent = nwNodo;
                }

                parent.EstadoJmpTrue = nwNodo;
                nwNodo.Parent = parent;
                procShiftChildren(nwNodo, 1);        //Actualiza Aguas Abajo
                procOffsetParents(nwNodo, 1);          //SetOffsetMatrizJmpTrueArriba(nwNodo);  //set Offset de aguas Arriba

            }
            else
            {
                if (parent.EstadoJmpFalse != null)
                {   //si ya tenia estoy intercalando
                    nwNodo.EstadoJmpFalse = parent.EstadoJmpFalse;
                    parent.EstadoJmpFalse.Parent = nwNodo;
                }

                parent.EstadoJmpFalse = nwNodo;

                nwNodo.Parent = parent;
                procDesplazarXPorJmpFalse(nwNodo);        //Actualiza Aguas Abajo
            }

            construirMatriz();

            return nwNodo;
        }
        internal static void EliminarEstado(Node_Estado estado)
        {

            if (estado.Parent == null)
            {
                MessageBox.Show("No se puede eliminar el Estado 0 ", "Elimnar Estado");
                return;
            }

            if (estado.EstadoJmpTrue != null && estado.EstadoJmpFalse != null)
            {
                MessageBox.Show("No se puede eliminar el Estado con doble salto condicional", "Elimnar Estado");
                return;
            }

            //Si viene de un jmpTrue
            if (estado.Parent.EstadoJmpTrue == estado)
            {
                procShiftChildren(estado, -1);     //Actualiza Aguas Abajo
                procOffsetParents(estado, -1);     //SetOffsetMatrizJmpTrueArriba(nwNodo);  //set Offset de aguas Arriba


                //si ya tenia jmpTrue estaba intercalando
                if (estado.EstadoJmpTrue != null)
                {
                    estado.Parent.EstadoJmpTrue = estado.EstadoJmpTrue;
                    //nuevo parent
                    estado.EstadoJmpTrue.Parent = estado.Parent;
                }
                else
                {
                    estado.Parent.EstadoJmpTrue = null;
                }
            }


            //Si viene de un jmpFalse
            if (estado.Parent.EstadoJmpFalse == estado)
            {
                if (estado.EstadoJmpFalse != null)
                {   //si ya tenia estaba intercalando
                    procDesplazarXPorJmpFalse(estado.Parent);        //Actualiza Aguas Abajo

                    estado.Parent.EstadoJmpFalse = estado.EstadoJmpFalse;
                    //nuevo Parent
                    estado.EstadoJmpFalse.Parent = estado.Parent;
                }
                else
                {
                    estado.Parent.EstadoJmpFalse = null;
                }
            }

            listNodes.Remove(estado);
            //Corrijo valor de los indexList
            for (int i = 0; i < listNodes.Count; i++) listNodes[i].IndexListEstado = i;

            construirMatriz();

        }

        /// <summary>
        /// Va construyendo la matriz agregando los nuevos elementos
        /// </summary>
        public static void construirMatriz()
        {
            //IniciarMatrizProg();

            for (int i = 0; i < listNodes.Count; i++)
            {
                Point posP = ObtenerPosicion(listNodes[i]);

                //Offset Vertical se obtiene del Parent
                if (listNodes[i].Parent == null)
                {
                    listNodes[i].pos.X = 0;
                    listNodes[i].pos.Y = 0;
                    continue;
                }

                if (listNodes[i].Parent.EstadoJmpTrue == listNodes[i])
                {
                    listNodes[i].pos.X = posP.XLocation;
                    listNodes[i].pos.Y = posP.YLocation + GetOffsetFromLineJmpTrue(listNodes[i]);
                }
                if (listNodes[i].Parent.EstadoJmpFalse == listNodes[i])
                {
                    listNodes[i].pos.X = posP.XLocation;
                    listNodes[i].pos.Y = listNodes[i].Parent.pos.Y;

                }

            }
        }

        /// <summary>
        /// Toma el valor Parent jmpTrue "Master", solo 
        /// </summary>
        /// <param name="iEstado"></param>
        /// <returns></returns>
        private static int GetOffsetFromLineJmpTrue(Node_Estado iEstado)
        {
            int offset = 0;
            while (iEstado.Parent != null && iEstado.Parent.EstadoJmpTrue == iEstado)
            {
                iEstado = iEstado.Parent;
                offset = offset + iEstado.offsetY;
            }
            return (offset);
        }

        /// <summary>
        /// procOffsetParents> Consiste en buscar hacia arriba hasta encontrar el Parent EstadojmpFalse.
        /// esto seria el offset que tiene que tener un elemento ubicado a la izquierda. 
        /// Para esto, se le deja el valor "offsetY + 1" en ese Parent 
        /// La idea es que los elementos abajo del Parent usen ese valor para saber el deplazamiento del primer jmptrue.
        /// </summary>
        /// <param name="refEstado"></param>
        private static void procOffsetParents(Node_Estado refEstado, int shift)
        {
            // Busco el primer parent.jmpFalse aguas arriba
            while (refEstado != null)
            {
                if (refEstado.Parent != null)
                {
                    if (refEstado.Parent.EstadoJmpFalse == refEstado)
                    {
                        if (refEstado.Parent.offsetY + shift >= 0) refEstado.Parent.offsetY += shift;
                    }
                }
                refEstado = refEstado.Parent;
            }

        }


        /// <summary>
        /// Funcion Recusiva, Recorre en direccion X e Y ajustando las posiciones PosY y copiando , 
        /// cada vez que encuentra un jmpTrue o jmpFalse, se llama a si misma
        /// </summary>
        /// <param name="refEstado"></param>
        private static void procShiftChildren(Node_Estado refEstado, int shift)
        {

            if (refEstado.EstadoJmpFalse != null)
            {
                //refEstado.EstadoJmpFalse.offsetY = refEstado.offsetY;
                refEstado.EstadoJmpFalse.pos.Y = refEstado.pos.Y;

                refEstado = refEstado.EstadoJmpFalse;
                procShiftChildren(refEstado, shift);
            }
            if (refEstado.EstadoJmpTrue != null)
            {
                //refEstado.EstadoJmpTrue.offsetY = refEstado.offsetY;
                refEstado.EstadoJmpTrue.pos.Y = refEstado.EstadoJmpTrue.pos.Y + shift;

                refEstado = refEstado.EstadoJmpTrue;
                procShiftChildren(refEstado, shift);
            }

        }

        private static void recurOffsetJmpTruePorInsertFalse(Node_Estado parent)
        {
            if (parent.EstadoJmpTrue != null)
            {
                parent.EstadoJmpTrue.pos.X = parent.pos.X;
                parent = parent.EstadoJmpTrue;
                recurOffsetJmpTruePorInsertFalse(parent);
            }
        }

        /// <summary>
        /// Al agregar (intercalar) un item false todos los demas deben desplazarse 1
        /// </summary>
        /// <param name="refEstado"></param>
        private static void procDesplazarXPorJmpFalse(Node_Estado refEstado)
        {

            while (refEstado.EstadoJmpFalse != null)
            {
                refEstado.EstadoJmpFalse.pos.X += 1;
                refEstado = refEstado.EstadoJmpFalse;
                recurOffsetJmpTruePorInsertFalse(refEstado);
            }


        }

        internal static void GuardarMatrizEstados()
        {
            // NodesGraphWrite guarda todo: Estados, relaciones y Asignaciones
            JsonFunctions.NodesGraphWrite(listNodes);

            // Para el Proyecto final se tienen separados los archivos Nodes y Relaciones    
            JsonFunctions.NodesProgWrite(listNodes);
            JsonFunctions.AsignationWrite(listNodes);

        }

        /// <summary>
        /// Va construyendo la Lista agregando los nuevos elementos desde la Matriz
        /// </summary>
        /*        public static void construirListaProg()
                {
                    listaProg.Clear();
                    string spos ="";

                    for (int x = 0; x < LEN_COLS_PROG; x++)
                        for (int y = 0; y < LEN_ROWS_PROG; y++)
                            if (matrizProg[x, y] != null)
                            {
                                listaProg.Add(matrizProg[x, y]);
                            }
                }
        */
        /// <summary>
        /// Inicializa la matriz de programacion
        /// </summary>
        /*internal static void IniciarMatrizProg()
        {
            for (int i = 0; i < LEN_ROWS_PROG; i++)
                for (int j = 0; j < LEN_COLS_PROG; j++)
                {
                    matrizProg[j, i] = null;
                }
        }
        */



    }

}
