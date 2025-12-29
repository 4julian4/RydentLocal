using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ServicioRydentLocal.LogicaDelNegocio.Helpers
{
    internal class Automap
    {
        private static PropertyInfo BuscarPropiedad(object obj, string Nombre)
        {
            var retornar = obj.GetType().GetProperty(Nombre, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (retornar != null)
            {
                return retornar;
            }
            else
            {
                return null;
            }
        }


        public static T AutoMapearDesdeObjeto<T>(Object _ObjetoEntrada, bool ignorarId = false, string nombreId = "")
        {
            T retornar = Activator.CreateInstance<T>();
            foreach (PropertyInfo prop in _ObjetoEntrada.GetType().GetProperties())
            {
                var prop2 = BuscarPropiedad(retornar, prop.Name);
                if (ignorarId && prop.Name == nombreId)
                {

                }
                else
                {
                    if (prop2 != null)
                    {
                        if (prop2.CanWrite)
                        {
                            try
                            {
                                prop2.SetValue(retornar, prop.GetValue(_ObjetoEntrada, null), null);
                            }
                            catch (Exception)
                            {

                            }

                        }
                        else
                        {
                            string s = char.ToLower(prop.Name[0]) + prop.Name.Substring(1); ;
                            FieldInfo field = retornar.GetType().GetField(s, BindingFlags.Instance | BindingFlags.NonPublic);
                            if (field != null)
                            {
                                field.SetValue(retornar, prop.GetValue(_ObjetoEntrada, null));
                            }
                        }
                    }
                }

            }
            return retornar;
        }
        public static T AutoMapearObjeto<T>(Object _ObjetoEntrada, T retornar, bool ignorarId = false, string nombreId = "")
        {
            foreach (PropertyInfo prop in _ObjetoEntrada.GetType().GetProperties())
            {
                var prop2 = BuscarPropiedad(retornar, prop.Name);
                if (ignorarId && prop.Name == nombreId)
                {

                }
                else
                {
                    if (prop2 != null)
                    {
                        if (prop2.CanWrite)
                        {
                            try
                            {
                                prop2.SetValue(retornar, prop.GetValue(_ObjetoEntrada, null), null);
                            }
                            catch (Exception)
                            {

                            }

                        }
                        else
                        {
                            string s = char.ToLower(prop.Name[0]) + prop.Name.Substring(1); ;
                            FieldInfo field = retornar.GetType().GetField(s, BindingFlags.Instance | BindingFlags.NonPublic);
                            if (field != null)
                            {
                                field.SetValue(retornar, prop.GetValue(_ObjetoEntrada, null));
                            }
                        }
                    }
                }

            }
            return retornar;
        }

        public static DataTable ConvertToDataTable<T>(List<T> models)
        {
            // creating a data table instance and typed it as our incoming model   
            // as I make it generic, if you want, you can make it the model typed you want.  
            DataTable dataTable = new DataTable(typeof(T).Name);

            //Get all the properties of that model  
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Loop through all the properties              
            // Adding Column name to our datatable  
            foreach (PropertyInfo prop in Props)
            {
                //Setting column names as Property names    
                dataTable.Columns.Add(prop.Name);
            }
            // Adding Row and its value to our dataTable  
            foreach (T item in models)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows    
                    values[i] = Props[i].GetValue(item, null);
                }
                // Finally add value to datatable    
                dataTable.Rows.Add(values);
            }
            return dataTable;
        }

        public static List<string> NombresAtributosXObjeto<T>()
        {
            List<string> retornar = new List<string>();
            T obj = Activator.CreateInstance<T>();
            foreach (PropertyInfo p in obj.GetType().GetProperties())
            {
                retornar.Add(p.Name);
            }
            return retornar;
        }


        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static T CargarDe<T>(DataRow Row)
        {
            T retornar = Activator.CreateInstance<T>();
            if (Row != null)
            {
                foreach (PropertyInfo p in retornar.GetType().GetProperties())
                {

                    string name = p.Name;

                    if (Row.Table.Columns.Contains(name))
                    {//Row[name] != DBNull.Value &&
                        if (name.ToUpper() == p.Name.ToUpper())
                        {
                            object item = Row[name];
                            if (Row[name] != DBNull.Value && p.PropertyType != Row.Table.Columns[name].DataType)
                            {
                                var targetType = IsNullableType(p.PropertyType)
                                    ? Nullable.GetUnderlyingType(p.PropertyType)
                                    : p.PropertyType;

                                item = Convert.ChangeType(item, targetType);
                            }
                            if (Row[name] != DBNull.Value)
                            {
                                if (p.CanWrite)
                                {
                                    p.SetValue(retornar, item, null);
                                }
                                else
                                {
                                    string s = char.ToLower(p.Name[0]) + p.Name.Substring(1); ;
                                    FieldInfo field = retornar.GetType().GetField(s, BindingFlags.Instance | BindingFlags.NonPublic);
                                    field.SetValue(retornar, item);
                                }
                            }
                            else
                            {
                                if (p.CanWrite)
                                {
                                    if (p.PropertyType == typeof(string))
                                    {
                                        p.SetValue(retornar, "", null);
                                    }
                                    else
                                    {
                                        if (IsNullableType(p.PropertyType))
                                        {
                                            p.SetValue(retornar, null, null);
                                        }
                                        else if (p.PropertyType == typeof(Int32))
                                        {
                                            p.SetValue(retornar, -1, null);
                                        }
                                        else if (p.PropertyType == typeof(double))
                                        {
                                            p.SetValue(retornar, 0, null);
                                        }
                                        else if (p.PropertyType == typeof(DateTime))
                                        {
                                            p.SetValue(retornar, DateTime.MinValue, null);
                                        }
                                        //Jeisson Gómez 
                                        else if (p.PropertyType == typeof(TimeSpan))
                                        {
                                            p.SetValue(retornar, TimeSpan.MinValue, null);
                                        }
                                    }
                                }
                                else
                                {
                                    string s = char.ToLower(p.Name[0]) + p.Name.Substring(1); ;
                                    FieldInfo field = retornar.GetType().GetField(s, BindingFlags.Instance | BindingFlags.NonPublic);
                                    if (field.FieldType == typeof(string))
                                    {
                                        field.SetValue(retornar, "");
                                    }
                                    else
                                    {
                                        if (IsNullableType(field.FieldType))
                                        {
                                            field.SetValue(retornar, null);
                                        }
                                        else if (field.FieldType == typeof(Int32))
                                        {
                                            field.SetValue(retornar, 1);
                                        }
                                        else if (field.FieldType == typeof(double))
                                        {
                                            field.SetValue(retornar, 0);
                                        }
                                        else if (field.FieldType == typeof(DateTime))
                                        {
                                            field.SetValue(retornar, DateTime.MinValue);
                                        }
                                        //Jeisson Gómez 
                                        else if (field.FieldType == typeof(TimeSpan))
                                        {
                                            field.SetValue(retornar, TimeSpan.MinValue);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return retornar;
        }

        public static T CargarDe<T>(DataRow Row, List<string> mapearNombresXIndex)
        {
            T retornar = Activator.CreateInstance<T>();
            if (Row != null)
            {
                foreach (PropertyInfo p in retornar.GetType().GetProperties())
                {

                    string name = p.Name;
                    int indexObjeto = mapearNombresXIndex.IndexOf(name);
                    if (mapearNombresXIndex.Contains(name) && indexObjeto < Row.Table.Columns.Count)
                    {//Row[name] != DBNull.Value &&
                        if (name.ToUpper() == p.Name.ToUpper())
                        {
                            object item = Row[indexObjeto];
                            if (item != DBNull.Value && p.PropertyType != item.GetType())
                            {
                                var targetType = IsNullableType(p.PropertyType)
                                    ? Nullable.GetUnderlyingType(p.PropertyType)
                                    : p.PropertyType;

                                item = Convert.ChangeType(item, targetType);
                            }
                            if (item != DBNull.Value)
                            {
                                if (p.CanWrite)
                                {
                                    p.SetValue(retornar, item, null);
                                }
                                else
                                {
                                    string s = char.ToLower(p.Name[0]) + p.Name.Substring(1); ;
                                    FieldInfo field = retornar.GetType().GetField(s, BindingFlags.Instance | BindingFlags.NonPublic);
                                    field.SetValue(retornar, item);
                                }
                            }
                            else
                            {
                                if (p.CanWrite)
                                {
                                    if (p.PropertyType == typeof(string))
                                    {
                                        p.SetValue(retornar, "", null);
                                    }
                                    else
                                    {
                                        if (IsNullableType(p.PropertyType))
                                        {
                                            p.SetValue(retornar, null, null);
                                        }
                                        else if (p.PropertyType == typeof(Int32))
                                        {
                                            p.SetValue(retornar, -1, null);
                                        }
                                        else if (p.PropertyType == typeof(double))
                                        {
                                            p.SetValue(retornar, 0, null);
                                        }
                                        else if (p.PropertyType == typeof(DateTime))
                                        {
                                            p.SetValue(retornar, DateTime.MinValue, null);
                                        }
                                        //Jeisson Gómez 
                                        else if (p.PropertyType == typeof(TimeSpan))
                                        {
                                            p.SetValue(retornar, TimeSpan.MinValue, null);
                                        }
                                    }
                                }
                                else
                                {
                                    string s = char.ToLower(p.Name[0]) + p.Name.Substring(1); ;
                                    FieldInfo field = retornar.GetType().GetField(s, BindingFlags.Instance | BindingFlags.NonPublic);
                                    if (field.FieldType == typeof(string))
                                    {
                                        field.SetValue(retornar, "");
                                    }
                                    else
                                    {
                                        if (IsNullableType(field.FieldType))
                                        {
                                            field.SetValue(retornar, null);
                                        }
                                        else if (field.FieldType == typeof(Int32))
                                        {
                                            field.SetValue(retornar, 1);
                                        }
                                        else if (field.FieldType == typeof(double))
                                        {
                                            field.SetValue(retornar, 0);
                                        }
                                        else if (field.FieldType == typeof(DateTime))
                                        {
                                            field.SetValue(retornar, DateTime.MinValue);
                                        }
                                        //Jeisson Gómez 
                                        else if (field.FieldType == typeof(TimeSpan))
                                        {
                                            field.SetValue(retornar, TimeSpan.MinValue);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return retornar;
        }
    }
}
