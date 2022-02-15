using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using LoWiProg.FSControls;
using System.Linq;

namespace LoWiProg.JsonFiles
{
    public class JsonAsignaciones
    {
        public int Id { get; set; }    // Es el estado al que pertenece las asignacion
        public int iReg { get; set; }    // Registro al que se le asiganara un valor
        public int ValReg { get; set; }     // Valor a de la asigacion 
                                            //(si es mayor a 10000 es el valor de un registro)
    }


    public class JsonGraphDataEstado
    {
        //public DateTimeOffset Date { get; set; }
        public String Text { get; set; }    // IMPORTANTE PONER get set
        public int IdEstado { get; set; }
        //public List<string> Acciones { get; set; }
        public List<JsonAsignaciones> Asignaciones { get; set; }
        public string COS { get; set; }
        public int posX { get; set; }
        public int posY { get; set; }
        public int parent { get; set; }
        public int jmpTrue { get; set; }
        public int jmpFalse { get; set; }
        public int offsetGraphY { get; set; }
    }

    public class JsonProgDataEstado
    {
        public int Id { get; set; }
        public int jT { get; set; }
        public int jF { get; set; }

        //public List<string> FsEs_Acciones { get; set; }
        //public List<JsonAsignaciones> Asignaciones { get; set; }
        //public string CS { get; set; }
        public int reg { get; set; }        // registro a compara
        public int cond { get; set; }       // condicion de comparacion
        public int valr { get; set; }       // valor/reg de comparacion
    }

    public class JsonRelations
    {
        public Boolean iRelationType { get; set; }    // IMPORTANTE PONER get set
        public int iRelationOrigin { get; set; }    // IMPORTANTE PONER get set
        public int iRelationEnd { get; set; }    // IMPORTANTE PONER get set
    }

    public class JsonVZ
    {
        internal static List<String> ObtenerListaValores(string json)
        {
            //List<JsonWiFi> listJsonWiFi = JsonSerializer.Deserialize<List<JsonWiFi>>(json);
            string[] parametros;
            List<String> rp = new List<String>();

            String rstr = json;
            rstr = rstr.Replace("{", "");
            rstr = rstr.Replace("}", "");
            rstr = rstr.Replace("\n", "");

            parametros = rstr.Split(',').ToArray();

            string[] param_Par;

            for (int i = 0; i < parametros.Length; i++)
            {
                string[] separador = { "\":" }; // separa si encuentra un (":)
                param_Par = parametros[i].Split(separador, StringSplitOptions.RemoveEmptyEntries).ToArray();
                if (param_Par.Length == 2) rp.Add(param_Par[1].Replace("\"", "")); //por si tiene comillas 
            }
            return rp;
        }

        internal static string ObtenerValorStringIndex(string json, int index)
        {
            List<String> rp = ObtenerListaValores(json);
            if (index < rp.Count)
                return (rp[index]);
            else return ("Err Index");
        }
    }
    public class JsonFunctions
    {
        public static List<Node_Estado> NodesRead()
        {
            List<JsonGraphDataEstado> listJsonEstados;// = new List<JsonDataEstado>();
            List<Node_Estado> list_Estados = new List<Node_Estado>();

            String fileName = "Estados.json";
            try
            {
                String jsonString = File.ReadAllText(fileName);

                listJsonEstados = JsonSerializer.Deserialize<List<JsonGraphDataEstado>>(jsonString);


                for (int i = 0; i < listJsonEstados.Count; i++)
                {
                    Node_Estado node_Estado = new Node_Estado();
                    node_Estado.Text = listJsonEstados[i].Text;
                    if (listJsonEstados[i].Asignaciones.Count != 0)
                    {
                        //Asignaciones no se lee en formato json ;
                        for (int j = 0; j < listJsonEstados[i].Asignaciones.Count; j++)
                        {   // Carga lista de asignaciones en NodoEstado
                            node_Estado.Asignaciones.Add(
                            listJsonEstados[i].Asignaciones[j].iReg.ToString() + " = " +
                            listJsonEstados[i].Asignaciones[j].ValReg.ToString());
                        }
                    }

                    node_Estado.COS = listJsonEstados[i].COS;
                    node_Estado.IndexListEstado = i;
                    node_Estado.pos.X = listJsonEstados[i].posX;
                    node_Estado.pos.Y = listJsonEstados[i].posY;
                    node_Estado.offsetY = listJsonEstados[i].offsetGraphY;
                    list_Estados.Add(node_Estado);
                }

                // ASIGNACION DE JMPtrue/False ESTADOS segun valor Index ya que en json guardo index pero programa trabaja con nodos directamente
                for (int i = 0; i < list_Estados.Count; i++)
                {
                    if (listJsonEstados[i].parent >= 0)
                        list_Estados[i].Parent = list_Estados[listJsonEstados[i].parent];

                    if (listJsonEstados[i].jmpTrue >= 0)
                        list_Estados[i].EstadoJmpTrue = list_Estados[listJsonEstados[i].jmpTrue];

                    if (listJsonEstados[i].jmpFalse >= 0)
                        list_Estados[i].EstadoJmpFalse = list_Estados[listJsonEstados[i].jmpFalse];
                }

                // SOLO PARA CHECKEAR FUNCION
                //elationNodesRead(listJsonEstados);



            }

            catch (Exception e)
            {
                Exception excep = e;
                throw;
            }


            return (list_Estados);
        }


        /// <summary>
        /// Esta funcion es solo para probar la carga de los jmpTrue y jmpFalse a partir del archivo Relation.json
        /// No se usa ya que los campos nombrados se cargan directamente de Estados.json
        /// </summary>
        /// <param name="listEstados"></param>
        /// <returns></returns>
        public static List<JsonGraphDataEstado> RelationNodesRead(List<JsonGraphDataEstado> listEstados)
        {
            List<JsonRelations> listJsonRelations = new List<JsonRelations>();

            String fileName = "Relations.json";
            try
            {
                // Lo que sigue toma el archivo y lo carga en registros json.
                String jsonString = File.ReadAllText(fileName);
                listJsonRelations = JsonSerializer.Deserialize<List<JsonRelations>>(jsonString);

                // for anidado>  
                // por cada "Relacion" (en for exterior) recorre los estados (for interior)
                // hasta encontrar al estado que pertenece.
                for (int i = 0; i < listJsonRelations.Count; i++)  // recorre las relaciones
                    for (int j = 0; j < listEstados.Count; j++)     //busca en los estados
                    {
                        // si a mi relacionOrigen le encontre el estado
                        if (listJsonRelations[i].iRelationOrigin == listEstados[j].IdEstado)
                        {   // si mi relacionOrigen es tipo true, entonces el destino es jmpTrue
                            if (listJsonRelations[i].iRelationType == true)
                                listEstados[j].jmpTrue = listJsonRelations[i].iRelationEnd;
                            else // es listJsonRelations[i].iRelationType == false
                                listEstados[j].jmpFalse = listJsonRelations[i].iRelationEnd;

                            break; // si ya encontro el estado origen no hace falta que siga
                        }
                    }
            }

            catch (Exception e)
            {
                Exception excep = e;
                throw;
            }
            return (listEstados);
        }


        /// <summary>
        /// Escribe en archivo formato json la lista de estados graficos. La lista de estados graficos se diferencia de la
        /// lista de estados nodos, en que tiene mas info relacionadas a las posiciones dentro de editor
        /// </summary>
        /// <param name="listEstados"></param>
        public static void NodesGraphWrite(List<Node_Estado> listEstados)
        {

            List<JsonGraphDataEstado> listNodes = new List<JsonGraphDataEstado>();


            for (int i = 0; i < listEstados.Count; i++)
            {

                int auxJmpFalse = -1, auxJmpTrue = -1, auxParent = -1;

                if (listEstados[i].EstadoJmpFalse != null) auxJmpFalse = listEstados[i].EstadoJmpFalse.IndexListEstado;
                if (listEstados[i].EstadoJmpTrue != null) auxJmpTrue = listEstados[i].EstadoJmpTrue.IndexListEstado;
                if (listEstados[i].Parent != null) auxParent = listEstados[i].Parent.IndexListEstado;


                // Proceso de toma de asignaciones
                List<JsonAsignaciones> ListjAsig = new List<JsonAsignaciones>();
                for (int j = 0; j < listEstados[i].Asignaciones.Count; j++)
                {
                    string asignaciones = listEstados[i].Asignaciones[j].ToString();
                    if (asignaciones.Length >= 3)
                    {
                        string[] parametros;
                        parametros = asignaciones.Split('=').ToArray();
                        if (parametros.Length == 2)
                        {
                            JsonAsignaciones jAsig = new JsonAsignaciones();
                            jAsig.iReg = int.Parse(parametros[0]);
                            jAsig.ValReg = int.Parse(parametros[1]);
                            ListjAsig.Add(jAsig);
                        }

                    }

                }


                var DataEstado = new JsonGraphDataEstado
                {
                    IdEstado = i,
                    Text = listEstados[i].Text,
                    COS = listEstados[i].COS,
                    posX = listEstados[i].pos.X,
                    posY = listEstados[i].pos.Y,
                    parent = auxParent,
                    jmpFalse = auxJmpFalse,
                    jmpTrue = auxJmpTrue,
                    offsetGraphY = listEstados[i].offsetY,
                    Asignaciones = ListjAsig,
                };

                listNodes.Add(DataEstado);
            }

            String fileName = "Estados.json";
            String jsonString = JsonSerializer.Serialize(listNodes);
            File.WriteAllText(fileName, jsonString);

            //Console.WriteLine(File.ReadAllText(fileName));
        }

        /// <summary>
        /// Usado para generar un archivo EstadosProg.json a partir de las lista de estados.
        /// Esta archivo es el que se envia al LoWiCom para cargar los estados en su eeprom
        /// </summary>
        /// <param name="listEstados"></param>
        public static void NodesProgWrite(List<Node_Estado> listEstados)
        {
            List<JsonProgDataEstado> listNodes = new List<JsonProgDataEstado>();
            List<JsonAsignaciones> ListjAsig = new List<JsonAsignaciones>();

            for (int i = 0; i < listEstados.Count; i++)
            {

                for (int j = 0; j < listEstados[i].Asignaciones.Count; j++)
                {
                    string FsEs_Acciones = listEstados[i].Asignaciones[j].ToString();
                    if (FsEs_Acciones.Length >= 3)
                    {
                        string[] parametros;
                        parametros = FsEs_Acciones.Split('=').ToArray();
                        if (parametros.Length == 2)
                        {
                            JsonAsignaciones jAsig = new JsonAsignaciones();
                            jAsig.iReg = int.Parse(parametros[0]);
                            jAsig.ValReg = int.Parse(parametros[1]);
                            ListjAsig.Add(jAsig);
                        }

                    }

                }
                int auxJmpFalse = 0, auxJmpTrue = 0;

                if (listEstados[i].EstadoJmpFalse != null) auxJmpFalse = listEstados[i].EstadoJmpFalse.IndexListEstado;
                if (listEstados[i].EstadoJmpTrue != null) auxJmpTrue = listEstados[i].EstadoJmpTrue.IndexListEstado;
                //if (listEstados[i].Parent != null) auxParent = listEstados[i].Parent.IndexListEstado;


                var DataEstado = new JsonProgDataEstado
                {
                    Id = i,
                    //CS = listEstados[i].COS,
                    jT = auxJmpTrue,
                    jF = auxJmpFalse
                };

                listNodes.Add(DataEstado);
            }

            String fileName = "EstadosProg.json";
            String jsonString = JsonSerializer.Serialize(listNodes);
            File.WriteAllText(fileName, jsonString);

            //Console.WriteLine(File.ReadAllText(fileName));
        }
        public static void RelationWrite(List<Node_Estado> listEstados)
        {
            List<JsonRelations> listJsonRelations = new List<JsonRelations>();

            for (int i = 0; i < listEstados.Count; i++)
            {

                int auxJmpFalse = -1, auxJmpTrue = -1;

                if (listEstados[i].EstadoJmpFalse != null)
                {
                    auxJmpFalse = listEstados[i].EstadoJmpFalse.IndexListEstado;

                    var DataRelations = new JsonRelations
                    {
                        iRelationType = false,
                        iRelationOrigin = listEstados[i].IndexListEstado,
                        iRelationEnd = auxJmpFalse,
                    };
                    listJsonRelations.Add(DataRelations);
                }

                if (listEstados[i].EstadoJmpTrue != null)
                {
                    auxJmpTrue = listEstados[i].EstadoJmpTrue.IndexListEstado;

                    var DataRelations = new JsonRelations
                    {
                        iRelationType = true,
                        iRelationOrigin = listEstados[i].IndexListEstado,
                        iRelationEnd = auxJmpTrue,
                    };
                    listJsonRelations.Add(DataRelations);
                }


            }

            String fileName = "Relations.json";
            String jsonString = JsonSerializer.Serialize(listJsonRelations);
            File.WriteAllText(fileName, jsonString);

        }

        /// <summary>
        /// Escribe un archivo con un lista de Asignaciones. 
        /// Recorre una lista de estados: listEstados ( recibe como argumento en la funcion) para ver si tiene
        /// el campo Asignaciones con valores, si es asi, los va cargando en otra lista llamada Asignaciones.
        /// </summary>
        /// <param name="listEstados"></param> recibe una lista de estados, uno de los campos es Asignaciones
        internal static void AsignationWrite(List<Node_Estado> listEstados)
        {
            // Prepara una lista de asignaciones nueva
            List<JsonAsignaciones> listJsonAsignations = new List<JsonAsignaciones>();

            //recorro la lista estados, para ver si este tiene "Asignaciones"
            for (int i = 0; i < listEstados.Count; i++)
            {
                if (listEstados[i].Asignaciones != null && listEstados[i].Asignaciones.Count > 0)
                {  // encontro un estado con asignaciones
                    for (int j = 0; j < listEstados[i].Asignaciones.Count; j++)
                    {
                        // preparo un nuevo elemento para la lista Asignaciones a guardar
                        var DataAsignacion = new JsonAsignaciones();
                        DataAsignacion.Id = listEstados[i].IndexListEstado;
                        string strAsig = listEstados[i].Asignaciones[j];
                        string[] parametros = strAsig.Split('=');
                        try { DataAsignacion.iReg = int.Parse(parametros[0]); }
                        catch { DataAsignacion.iReg = -1; }
                        try { DataAsignacion.ValReg = int.Parse(parametros[1]); }
                        catch { DataAsignacion.iReg = -1; }
                        listJsonAsignations.Add(DataAsignacion);
                    }
                }

            }

            String fileName = "Asignations.json";
            String jsonString = JsonSerializer.Serialize(listJsonAsignations);
            File.WriteAllText(fileName, jsonString);


        }
    }
}
// output:
//{"Date":"2019-08-01T00:00:00-07:00","TemperatureCelsius":25,"Summary":"Hot"}