using System.IO;
using Microsoft.Data.SqlClient;

namespace ADOMaker
{
    public partial class Form1 : Form
    {
        private List<Tabla> Tablas { get; set; }
        public Form1()
        {
            InitializeComponent();
            Tablas = new List<Tabla>();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Tablas = new List<Tabla>();
            treeView1.Nodes.Clear();
            List<Tuple<string, string>> identities = new List<Tuple<string, string>>();
            using (SqlConnection con = new SqlConnection(toolStripTextBox1.Text))
            {

                using (SqlCommand cm = new SqlCommand("SELECT sys.objects.name AS table_name, sys.columns.name AS column_name FROM sys.columns JOIN sys.objects ON sys.columns.object_id=sys.objects.object_id WHERE sys.columns.is_identity=1 AND sys.objects.type in (N'U')", con))
                {
                    cm.Connection.Open();
                    var reader = cm.ExecuteReader();
                    while (reader.Read())                    
                        identities.Add(new Tuple<string, string>(reader[0].ToString() + "", reader[1].ToString() + ""));                    
                    reader.Close();
                    cm.Connection.Close();
                }

                using (SqlCommand cmd = new SqlCommand("SELECT name FROM sysobjects WHERE xtype = 'U'", con))
                {
                    cmd.Connection.Open();
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        Tablas.Add(new Tabla((string)reader[0], toolStripTextBox2.Text));
                        treeView1.Nodes.Add((string)reader[0]);
                    }
                    reader.Close();
                    cmd.Connection.Close();
                }

                foreach (var tabla in Tablas)
                {
                    using (SqlCommand com = new SqlCommand("SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + tabla.SqlNombre + "'", con))
                    {
                        com.Connection.Open();
                        var reader = com.ExecuteReader();
                        while (reader.Read())
                        {
                            Campo campo = new Campo((string)reader[0], (string)reader[1]);
                            campo.Nombre = tabla.Nombre == campo.Nombre ? "@" + campo.Nombre : campo.Nombre;
                            campo.Nuleable = ((string)reader[2]).ToUpper() != "NO";
                            campo.IsIdentity = identities.Exists(x => x.Item1 == tabla.Nombre && x.Item2 == campo.Nombre);
                            tabla.Campos.Add(campo);
                        }
                        com.Connection.Close();
                        reader.Close();
                    }
                }
            }

            string dir = Directory.GetCurrentDirectory() + "\\" + toolStripTextBox2.Text;
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                Directory.CreateDirectory(dir + "\\Modelos");
                Directory.CreateDirectory(dir + "\\Controladores");
            }

            using (StreamWriter sw = new StreamWriter(dir + "\\Controladores\\BaseDeDatos.cs", false))
            {
                sw.WriteLine("using System;\r\n");
                sw.WriteLine("using System.Data.SqlClient;\r\n");
                sw.WriteLine("using System.Collections.Generic;\r\n");
                sw.WriteLine("using System.Threading.Tasks;\r\n");
                sw.WriteLine("\r\n");
                sw.WriteLine("namespace " + toolStripTextBox2.Text + ".Controladores\r\n");
                sw.WriteLine("{\r\n");
                sw.WriteLine("    public class BaseDeDatos\r\n");
                sw.WriteLine("    {\r\n");
                sw.WriteLine("        public static string Conexion =@\"" + toolStripTextBox1.Text + "\r\n");
                sw.WriteLine("        public static int Comando(string sql)\r\n");
                sw.WriteLine("        {\r\n");
                sw.WriteLine("            int n = 0;\r\n");
                sw.WriteLine("            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n");
                sw.WriteLine("            {\r\n");
                sw.WriteLine("                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n");
                sw.WriteLine("                {\r\n");
                sw.WriteLine("                    cmd.Connection.Open();\r\n");
                sw.WriteLine("                    n = cmd.ExecuteNonQuery();\r\n");
                sw.WriteLine("                    cmd.Connection.Close();\r\n");
                sw.WriteLine("                }\r\n");
                sw.WriteLine("            return n;\r\n");
                sw.WriteLine("            }\r\n");
                sw.WriteLine("        }\r\n");
                sw.WriteLine("\r\n");
                sw.WriteLine("        public static Task<int> Comando(string sql)\r\n");
                sw.WriteLine("        {\r\n");
                sw.WriteLine("            return Task.Run(() => {\r\n");
                sw.WriteLine("            int n = 0;\r\n");
                sw.WriteLine("            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n");
                sw.WriteLine("            {\r\n");
                sw.WriteLine("                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n");
                sw.WriteLine("                {\r\n");
                sw.WriteLine("                    cmd.Connection.Open();\r\n");
                sw.WriteLine("                    n = cmd.ExecuteNonQuery();\r\n");
                sw.WriteLine("                    cmd.Connection.Close();\r\n");
                sw.WriteLine("                }\r\n");
                sw.WriteLine("            return n;});\r\n");
                sw.WriteLine("            }\r\n");
                sw.WriteLine("        }\r\n");
                sw.WriteLine("\r\n");
                sw.WriteLine("        public static List<List<Tuple<string,object>>> Consulta(string sql");
                sw.WriteLine("        {\r\n");
                sw.WriteLine("            List<List<Tuple<string,object>>> l = new List<List<Tuple<string,object>>>();\r\n");
                sw.WriteLine("            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n");
                sw.WriteLine("            {\r\n");
                sw.WriteLine("                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n");
                sw.WriteLine("                {\r\n");
                sw.WriteLine("                    cmd.Connection.Open();\r\n");
                sw.WriteLine("                    reader = cmd.ExecuteReader();\r\n");
                sw.WriteLine("                    while(reader.Read())\r\n");
                sw.WriteLine("                    {\r\n");
                sw.WriteLine("                        var c = new List<Tuple<string,object>>();\r\n");
                sw.WriteLine("                        for(int n=0; n < reader.FieldCount; n++)\r\n");
                sw.WriteLine("                            c.Add(new Tuple<string,object>(reader.GetName(n), reader[n]));\r\n");
                sw.WriteLine("                        l.Add(c);\r\n");
                sw.WriteLine("                    }\r\n");
                sw.WriteLine("                    cmd.Connection.Close();\r\n");
                sw.WriteLine("                }\r\n");
                sw.WriteLine("            }\r\n");
                sw.WriteLine("            return l;\r\n");
                sw.WriteLine("        }\r\n");
                sw.WriteLine("\r\n");
                sw.WriteLine("        public static Task<List<List<Tuple<string,object>>>> Consulta(string sql");
                sw.WriteLine("        {\r\n");
                sw.WriteLine("            return Task.Run(() => {\r\n");
                sw.WriteLine("            List<List<Tuple<string,object>>> l = new List<List<Tuple<string,object>>>();\r\n");
                sw.WriteLine("            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n");
                sw.WriteLine("            {\r\n");
                sw.WriteLine("                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n");
                sw.WriteLine("                {\r\n");
                sw.WriteLine("                    cmd.Connection.Open();\r\n");
                sw.WriteLine("                    reader = cmd.ExecuteReader();\r\n");
                sw.WriteLine("                    while(reader.Read())\r\n");
                sw.WriteLine("                    {\r\n");
                sw.WriteLine("                        var c = new List<Tuple<string,object>>();\r\n");
                sw.WriteLine("                        for(int n=0; n < reader.FieldCount; n++)\r\n");
                sw.WriteLine("                            c.Add(new Tuple<string,object>(reader.GetName(n), reader[n]));\r\n");
                sw.WriteLine("                        l.Add(c);\r\n");
                sw.WriteLine("                    }\r\n");
                sw.WriteLine("                    cmd.Connection.Close();\r\n");
                sw.WriteLine("                }\r\n");
                sw.WriteLine("            }\r\n");
                sw.WriteLine("            return l;});\r\n");
                sw.WriteLine("        }\r\n");
                sw.WriteLine("    }\r\n");
                sw.WriteLine("}\r\n");
            }

            foreach (var tabla in Tablas)
            {
                var node = treeView1.Nodes.Add(tabla.Nombre);

                using (StreamWriter sw = new StreamWriter(dir + "\\Modelos\\" + tabla.Nombre + ".cs", false))                
                    sw.Write(tabla.Modelo());                

                using (StreamWriter sw = new StreamWriter(dir + "\\Controladores\\" + tabla.Nombre + ".cs", false))                
                    sw.Write(tabla.Controlador());                

                foreach (var campo in tabla.Campos)
                    node.Nodes.Add(campo.Nombre + " - " + campo.Tipo.Sql);
            }
        }
        
        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            int n = treeView1.SelectedNode.Index;
            if(n >= 0)
            {
                SetTabla(Tablas[n]);
                modelo.Text = Tablas[n].Modelo();
                controlador.Text = Tablas[n].Controlador();
            }
        }

        void SetTabla(Tabla t)
        {
            tabla.Rows.Clear();
            for (int c = 0; c < t.Campos.Count; c++)
                tabla.Rows.Add(t.Campos[c].SqlNombre, t.Campos[c].Tipo.Sql, t.Campos[c].Nuleable, t.Campos[c].IsIdentity);
        }
    }

    public class Tabla
    {
        public string Nombre { get; set; }
        public string SqlNombre { get; set; }
        public List<Campo> Campos { get; set; }
        public string NameSpace { get; set; }

        public Tabla(string nombre, string @namespace)
        {
            Nombre = nombre;
            SqlNombre = nombre;
            Campos = new List<Campo>();
            NameSpace = @namespace;            
        }

        public string Modelo()
        {
            string s = "";
            s += "using System;\r\n";
            s += "\r\n";
            s += "namespace " + NameSpace + ".Modelos\r\n";
            s += "{\r\n";
            s += "    public class " + Nombre + "\r\n";
            s += "    {\r\n";
            for(int c=0; c < Campos.Count; c++)
            s += "        public " + Campos[c].Tipo.CS + " " + Campos[c].Nombre + " { get; set; }\r\n"; ;
            s += "    }\r\n";
            s += "}\r\n";
            return s;
        }

        public string Controlador()
        {
            string s = "";
            s += "using System;\r\n";
            s += "using System.Data;\r\n";
            s += "using System.Data.SqlClient;\r\n";
            s += "using System.Collections.Generic;\r\n";
            s += "using System.Threading.Tasks;\r\n";
            s += "\r\n";
            s += "namespace " + NameSpace + ".Controladores\r\n";
            s += "{\r\n";
            s += "    public class " + Nombre + "\r\n";
            s += "    {\r\n";
            s += Leer() + "\r\n";
            s += Crear() + "\r\n";
            s += Modificar() + "\r\n";
            s += Eliminar() + "\r\n";
            s += "    }";
            s += "}";
            return s;
        }

        private string Leer()
        {
            string s = "\r\n";
            s += "        public static List<Modelos." + Nombre + "> Leer(string query, bool fullquery=false)\r\n";
            s += "        {\r\n";
            s += "            List<Modelos." + Nombre + "> lista = new List<Modelos." + Nombre + ">();\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = fullquery? query : \"SELECT* FROM " + Nombre + " \" + (BaseDeDatos.Void(query)? \"\" : \" WHERE \" + query);\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += "                    using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                    {\r\n";
            s += "                        cmd.Connection.Open();\r\n";
            s += "                        var reader = cmd.ExecuteReader();\r\n";
            s += "                        while(reader.Read)\r\n";
            s += "                        {\r\n";
            s += "                            Modelos." + Nombre + " m = new Modelos." + Nombre + "();\r\n";
            for (int n = 0; n < Campos.Count; n++)
            {
                var c = Campos[n];
                if(c.Tipo.CS == "string")
                    s += String.Format("                            m.{1} = DBNull.Value.Equals(reader[{0}])? \"\" : ({2})reader[{0}];\r\n", n, c.Nombre, c.Tipo.CS);
                else if (c.Tipo.CS == "bool")
                    s += String.Format("                            m.{1} = DBNull.Value.Equals(reader[{0}])? false : ({2})reader[{0}];\r\n", n, c.Nombre, c.Tipo.CS);
                else
                    s += String.Format("                            m.{1} = DBNull.Value.Equals(reader[{0}])? null : ({2})reader[{0}];\r\n", n, c.Nombre, c.Tipo.CS);
            }
            s += "                            lista.Add(m);\r\n";
            s += "                        }\r\n";
            s += "                        cmd.Connection.Close();\r\n";
            s += "                    }\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return lista;\r\n";
            s += "        }\r\n";
            s += "\r\n";
            s += "        public static Task<List<Modelos." + Nombre + ">> LeerAsync(string query, bool fullquery=false)\r\n";
            s += "        {\r\n";
            s += "            return Task.Run(() => {\r\n";
            s += "            List<Modelos." + Nombre + "> lista = new List<Modelos." + Nombre + ">();\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = fullquery? query : \"SELECT* FROM " + Nombre + " \" + (BaseDeDatos.Void(query)? \"\" : \" WHERE \" + query);\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += "                    using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                    {\r\n";
            s += "                        cmd.Connection.Open();\r\n";
            s += "                        var reader = cmd.ExecuteReader();\r\n";
            s += "                        while(reader.Read)\r\n";
            s += "                        {\r\n";
            s += "                            Modelos." + Nombre + " m = new Modelos." + Nombre + "();\r\n";
            for (int n = 0; n < Campos.Count; n++)
            {
                var c = Campos[n];
                if (c.Tipo.CS == "string")
                    s += String.Format("                            m.{1} = DBNull.Value.Equals(reader[{0}])? \"\" : ({2})reader[{0}];\r\n", n, c.Nombre, c.Tipo.CS);
                else if (c.Tipo.CS == "bool")
                    s += String.Format("                            m.{1} = DBNull.Value.Equals(reader[{0}])? false : ({2})reader[{0}];\r\n", n, c.Nombre, c.Tipo.CS);
                else
                    s += String.Format("                            m.{1} = DBNull.Value.Equals(reader[{0}])? null : ({2})reader[{0}];\r\n", n, c.Nombre, c.Tipo.CS);
            }
            s += "                            lista.Add(m);\r\n";
            s += "                        }\r\n";
            s += "                        cmd.Connection.Close();\r\n";
            s += "                    }\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return lista;});\r\n";
            s += "        }\r\n";

            return s;
        }

        private string Crear()
        {
            string insert = "", values="", parats="";
            for (int c = 0; c < Campos.Count; c++)
            {
                string name = Campos[c].SqlNombre.ToLower();
                insert += (c == 0 ? "" : ", ") + Campos[c].SqlNombre;
                values += (c == 0 ? "@" : ", @") + name;
                parats += String.Format("                    SqlParameter {0} = cmd.Parameters.Add(\"@{0}\", SqlDbType.{1});\r\n", name, Campos[c].Tipo.SqlDbType);
                parats += String.Format("                    {0}.Value = m.{1};\r\n", name, Campos[c].Nombre);
            }
            string s = "\r\n";
            s += "        public static int Crear(Modelos." + Nombre + " m)\r\n";
            s += "        {\r\n";
            s += "            int n=0;\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = \"INSERT INTO " + Nombre +"(" + insert + ") VALUES(" + values +")\";\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += parats;
            s += "                    cmd.Connection.Open();\r\n";
            s += "                    n = cmd.ExecuteNonQuery();\r\n";
            s += "                    cmd.Connection.Close();\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return n;\r\n";
            s += "        }\r\n";
            s += "\r\n";
            s += "        public static Task<int> CrearAsync(Modelos." + Nombre + " m)\r\n";
            s += "        {\r\n";
            s += "            return Task.Run(() => {\r\n";
            s += "            int n=0;\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = \"INSERT INTO " + Nombre + "(" + insert + ") VALUES(" + values + ")\";\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += parats;
            s += "                    cmd.Connection.Open();\r\n";
            s += "                    n = cmd.ExecuteNonQuery();\r\n";
            s += "                    cmd.Connection.Close();\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return n;});\r\n";
            s += "        }\r\n";

            return s;
        }

        private string Modificar()
        {
            string parats = "", colums="";
            for (int c = 0; c < Campos.Count; c++)
            {
                colums += (c == 0? "" : ", ") + String.Format("{0} = @{1}", Campos[c].SqlNombre, Campos[c].SqlNombre.ToLower());
                parats += String.Format("                    SqlParameter {0} = cmd.Parameters.Add(\"@{0}\", SqlDbType.{1});\r\n", Campos[c].SqlNombre.ToLower(), Campos[c].Tipo.SqlDbType);
                parats += String.Format("                    {0}.Value = m.{1};\r\n", Campos[c].SqlNombre.ToLower(), Campos[c].Nombre);
            }
            string s = "\r\n";
            s += "        public static int Modificar(Modelos." + Nombre + " m, string query=\"\")\r\n";
            s += "        {\r\n";
            s += "            int n;\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = \"UPDATE " + Nombre + " SET " + colums + "\" + (BaseDeDatos.Void(query)? \"\" : \" WHERE \" + query);\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += parats;
            s += "                    cmd.Connection.Open();\r\n";
            s += "                    n = cmd.ExecuteNonQuery();\r\n";
            s += "                    cmd.Connection.Close();\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return n;\r\n";
            s += "        }\r\n";
            s += "\r\n";
            s += "        public static Task<int> Modificar(Modelos." + Nombre + " m, string query=\"\")\r\n";
            s += "        {\r\n";
            s += "            return Task.Run(() => {\r\n";
            s += "            int n;\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = \"UPDATE " + Nombre + " SET " + colums + "\" + (BaseDeDatos.Void(query)? \"\" : \" WHERE \" + condicion);\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += parats;
            s += "                    cmd.Connection.Open();\r\n";
            s += "                    n = cmd.ExecuteNonQuery();\r\n";
            s += "                    cmd.Connection.Close();\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return n;});\r\n";
            s += "        }\r\n";

            return s;
        }

        private string Eliminar()
        {
            string parats = "";
            for (int c = 0; c < Campos.Count; c++)
            {
                parats += String.Format("                    SqlParameter {0} = cmd.Parameters.Add(\"@{0}\", SqlDbType.{1});\r\n", Campos[c].SqlNombre.ToLower(), Campos[c].Tipo.SqlDbType);
                parats += String.Format("                    {0}.Value = m.{1};\r\n", Campos[c].SqlNombre.ToLower(), Campos[c].Nombre);
            }
            string s = "\r\n";
            s += "        public static int Eliminar(string query, Modelos." + Nombre + " m = null)\r\n";
            s += "        {\r\n";
            s += "            int n;\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = \"DELETE FROM "+ Nombre + "\" + (BaseDeDatos.Void(query)? \"\" : \" WHERE \" + query); \"\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += parats;
            s += "                    cmd.Connection.Open();\r\n";
            s += "                    n = cmd.ExecuteNonQuery();\r\n";
            s += "                    cmd.Connection.Close();\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return n;\r\n";
            s += "        }\r\n";
            s += "\r\n";
            s += "        public static int Eliminar(string query, Modelos." + Nombre + " m = null)\r\n";
            s += "        {\r\n";
            s += "            return Task.Run(() => {\r\n";
            s += "            int n;\r\n";
            s += "            using (SqlConnection con = new SqlConnection(BaseDeDatos.Conexion))\r\n";
            s += "            {\r\n";
            s += "                string sql = \"DELETE FROM " + Nombre + "\" + (BaseDeDatos.Void(query)? \"\" : \" WHERE \" + query); \"\r\n";
            s += "                using (SqlCommand cmd = new SqlCommand(sql, con))\r\n";
            s += "                {\r\n";
            s += parats;
            s += "                    cmd.Connection.Open();\r\n";
            s += "                    n = cmd.ExecuteNonQuery();\r\n";
            s += "                    cmd.Connection.Close();\r\n";
            s += "                }\r\n";
            s += "            }\r\n";
            s += "            return n;});\r\n";
            s += "        }\r\n";
            return s;
        }
    }

    public class Campo
    {
        public string Nombre { get; set; }
        public string SqlNombre { get; set; }
        public Tipo Tipo { get; set; }
        public bool Nuleable { get; set; }
        public bool IsIdentity { get; set; }
        public Campo(string nombre, string tipo, bool nuleable=false, bool isidentity=false)
        {
            Nombre = nombre;
            Tipo = new Tipo(tipo);
            SqlNombre = nombre;
            IsIdentity = isidentity;
            Nuleable = nuleable;
        }
    }

    public class Tipo
    {        
        public string Sql { get; }
        private string sqldbtype;
        public string SqlDbType { get { return sqldbtype; } }
        private string cs;
        public string CS { get { return cs; } }
        public Tipo(string sql)
        {
            Sql = sql;
            switch (sql.ToLower())
            {
                case "bigint":
                    sqldbtype = "BigInt";
                    cs = "long?";
                    break;

                case "binary":
                    sqldbtype = "Binary";
                    cs = "byte[]";
                    break;

                case "bit":
                    sqldbtype = "Bit";
                    cs = "bool";
                    break;

                case "char":
                    sqldbtype = "Char";
                    cs = "string";
                    break;

                case "date":
                    sqldbtype = "Date";
                    cs = "DateTime?";
                    break;

                case "datetime":
                    sqldbtype = "DateTime";
                    cs = "DateTime?";
                    break;

                case "datetime2":
                    sqldbtype = "DateTime2";
                    cs = "DateTime?";
                    break;

                case "datetimeoffset":
                    sqldbtype = "DateTimeOffSet";
                    cs = "DateTimeOffset?";
                    break;

                case "decimal":
                    sqldbtype = "Decimal";
                    cs = "decimal?";
                    break;

                case "float":
                    sqldbtype = "Float";
                    cs = "float?";
                    break;

                case "image":
                    sqldbtype = "Image";
                    cs = "byte[]";
                    break;

                case "money":
                    sqldbtype = "Money";
                    cs = "decimal?";
                    break;

                case "int":
                    sqldbtype = "Int";
                    cs = "int?";
                    break;

                case "nchar":
                    sqldbtype = "NChar";
                    cs = "string";
                    break;

                case "ntext":
                    sqldbtype = "NChar";
                    cs = "string";
                    break;

                case "numeric":
                    sqldbtype = "Decimal";
                    cs = "decimal?";
                    break;

                case "nvarchar":
                    sqldbtype = "NVarChar";
                    cs = "string";
                    break;

                case "real":
                    sqldbtype = "Real";
                    cs = "float?";
                    break;

                case "smalldatetime":
                    sqldbtype = "Date";
                    cs = "DateTime?";
                    break;

                case "smallint":
                    sqldbtype = "SmallInt";
                    cs = "short?";
                    break;

                case "smallmoney":
                    sqldbtype = "SmallMoney";
                    cs = "decimal?";
                    break;

                case "text":
                    sqldbtype = "Text";
                    cs = "string";
                    break;

                case "time":
                    sqldbtype = "Time";
                    cs = "TimeSpan?";
                    break;

                case "timestamp":
                    sqldbtype = "Timestamp";
                    cs = "byte[]";
                    break;

                case "tinyint":
                    sqldbtype = "TinyInt";
                    cs = "byte?";
                    break;

                case "uniqueIdentifier":
                    sqldbtype = "UniqueIdentifier";
                    cs = "Guid?";
                    break;

                case "varbinary":
                    sqldbtype = "VarBinary";
                    cs = "byte[]";
                    break;

                case "varchar":
                    sqldbtype = "VarChar";
                    cs = "string";
                    break;

                case "xml":
                    sqldbtype = "Xml";
                    cs = "string";
                    break;

                default:
                    sqldbtype = "VarBinary";
                    cs = "object";
                    break;
            }
        }
    }
}