using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace napeans.dal
{
    public class AdoHelper : IDisposable, IAdoHelper
    {
        // Internal members
        protected string _connString = null;
        protected SqlConnection _conn = null;
        protected SqlTransaction _trans = null;
        protected bool _disposed = false;

        /// <summary>
        /// Sets or returns the connection string use by all instances of this class.
        /// </summary>

        /// <summary>
        /// Returns the current SqlTransaction object or null if no transaction
        /// is in effect.
        /// </summary>
        public SqlTransaction Transaction { get { return _trans; } }

        /// <summary>
        /// Constructor using global connection string.
        /// </summary>


        /// <summary>
        /// Constructure using connection string override
        /// </summary>
        /// <param name="connString">Connection string for this instance</param>
        public AdoHelper()
        {
            _connString = ConfigurationManager.ConnectionStrings["connString"].ConnectionString;
            Connect();
        }

        // Creates a SqlConnection using the current connection string
        protected void Connect()
        {
            _conn = new SqlConnection(_connString);
            _conn.Open();
        }


        public void UpdateWithDatatable(string strProc, DataTable dt, string dtParamName, Dictionary<string, string> param = null)
        {


            using (SqlConnection conn = new SqlConnection(_connString))
            {
                SqlCommand cmd = new SqlCommand(strProc, conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue(dtParamName, dt);
                if (param != null)
                {
                    foreach (var item in param)
                    {
                        cmd.Parameters.AddWithValue(item.Key, item.Value);
                    }
                }

                //SqlDataAdapter sda = new SqlDataAdapter(cmd);
                //DataSet ds = new DataSet();
                //sda.Fill(ds);

                conn.Open();
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }
        /// <summary>
        /// Constructs a SqlCommand with the given parameters. This method is normally called
        /// from the other methods and not called directly. But here it is if you need access
        /// to it.
        /// </summary>
        /// <param name="qry">SQL query or stored procedure name</param>
        /// <param name="type">Type of SQL command</param>
        /// <param name="args">Query arguments. Arguments should be in pairs where one is the
        /// name of the parameter and the second is the value. The very last argument can
        /// optionally be a SqlParameter object for specifying a custom argument type</param>
        /// <returns></returns>
        protected SqlCommand CreateCommand(string qry, CommandType type, Dictionary<string, string> args = null)
        {
            SqlCommand cmd = new SqlCommand(qry, _conn);

            // Associate with current transaction, if any
            if (_trans != null)
                cmd.Transaction = _trans;

            cmd.CommandType = type;

            if (args != null)
            {
                foreach (var item in args)
                {
                    var value = string.IsNullOrWhiteSpace(item.Value) ? DBNull.Value : (object)item.Value;
                    cmd.Parameters.AddWithValue(item.Key, value);
                }
            }

            return cmd;
        }


        protected SqlCommand CreateCommand(string qry, CommandType type, List<SqlParameter> param = null, Dictionary<string, string> args = null)
        {
            SqlCommand cmd = new SqlCommand(qry, _conn);

            // Associate with current transaction, if any
            if (_trans != null)
                cmd.Transaction = _trans;

            // Set command type
            cmd.CommandType = type;
            if (param != null)
            {
                foreach (var item in param)
                {
                    cmd.Parameters.Add(item);
                }
            }

            if (args != null)
            {
                foreach (var item in args)
                {
                    cmd.Parameters.AddWithValue(item.Key, item.Value);
                }
            }
            return cmd;
        }

        #region Exec Members

        /// <summary>
        /// Executes a query that returns no results
        /// </summary>
        /// <param name="qry">Query text</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>The number of rows affected</returns>
        public int ExecNonQuery(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
            {
                return cmd.ExecuteNonQuery();
            }
        }
        public int ExecNonQuery(string qry)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, null))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Executes a stored procedure that returns no results
        /// </summary>
        /// <param name="proc">Name of stored proceduret</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>The number of rows affected</returns>
        public int ExecNonQueryProc(string proc, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(proc, CommandType.StoredProcedure, args))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        public int ExecNonQuery(string qry, SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(qry, _conn))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;

                cmd.Parameters.AddRange(parameters);
                return cmd.ExecuteNonQuery();
            }
        }



        public int ExecNonQuery1(string qry, SqlParameter[] parameters)
        {
            using (SqlCommand cmd = new SqlCommand(qry, _conn))
            {
                if (_trans != null)
                    cmd.Transaction = _trans;

                if (parameters != null)
                    cmd.Parameters.AddRange(parameters);

                return cmd.ExecuteNonQuery();
            }
        }


        /// <summary>
        /// Executes a query that returns a single value
        /// </summary>
        /// <param name="qry">Query text</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Value of first column and first row of the results</returns>
        public object ExecScalar(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
            {
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes a query that returns a single value
        /// </summary>
        /// <param name="proc">Name of stored proceduret</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Value of first column and first row of the results</returns>
        public object ExecScalarProc(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
            {
                return cmd.ExecuteScalar();
            }
        }

        /// <summary>
        /// Executes a query and returns the results as a SqlDataReader
        /// </summary>
        /// <param name="qry">Query text</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Results as a SqlDataReader</returns>
        public SqlDataReader ExecDataReader(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
            {
                return cmd.ExecuteReader();
            }
        }

        /// <summary>
        /// Executes a stored procedure and returns the results as a SqlDataReader
        /// </summary>
        /// <param name="proc">Name of stored proceduret</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Results as a SqlDataReader</returns>
        public SqlDataReader ExecDataReaderProc(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
            {
                return cmd.ExecuteReader();
            }
        }

        /// <summary>
        /// Executes a query and returns the results as a DataSet
        /// </summary>
        /// <param name="qry">Query text</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Results as a DataSet</returns>
        public DataSet ExecDataSet(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
            {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                return ds; // ✅ return the filled dataset
            }
        }

        public DataSet ExecDataSet(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, param, args))
            {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                return ds;
            }
        }
        /// <summary>
        /// Executes a stored procedure and returns the results as a Data Set
        /// </summary>
        /// <param name="proc">Name of stored proceduret</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Results as a DataSet</returns>
        public DataSet ExecDataSetProc(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
            {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                return ds;
            }
        }

        public DataSet ExecDataSetProc(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, param, args))
            {
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                DataSet ds = new DataSet();
                adapt.Fill(ds);
                return ds;
            }
        }






        public List<T> ExecDataSet<T>(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, args))
            {
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapt.Fill(dt);
                        return CommonMethods.ConvertDataTable<T>(dt);
                    }


                }

            }
        }
        public List<T> ExecDataSet<T>(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.Text, param, args))
            {
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapt.Fill(dt);
                        return CommonMethods.ConvertDataTable<T>(dt);
                    }


                }
            }
        }
        /// <summary>
        /// Executes a stored procedure and returns the results as a Data Set
        /// </summary><T>
        /// <param name="proc">Name of stored proceduret</param>
        /// <param name="args">Any number of parameter name/value pairs and/or SQLParameter arguments</param>
        /// <returns>Results as a DataSet</returns>
        public List<T> ExecDataSetProc<T>(string qry, Dictionary<string, string> args)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, args))
            {
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapt.Fill(dt);
                        return CommonMethods.ConvertDataTable<T>(dt);
                    }


                }
            }
        }

        public List<T> ExecDataSetProc<T>(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null)
        {
            using (SqlCommand cmd = CreateCommand(qry, CommandType.StoredProcedure, param, args))
            {
                using (SqlDataAdapter adapt = new SqlDataAdapter(cmd))
                {
                    using (DataTable dt = new DataTable())
                    {
                        adapt.Fill(dt);
                        return CommonMethods.ConvertDataTable<T>(dt);
                    }


                }
            }
        }



        #endregion

        #region Transaction Members

        /// <summary>
        /// Begins a transaction
        /// </summary>
        /// <returns>The new SqlTransaction object</returns>
        public SqlTransaction BeginTransaction()
        {
            Rollback();
            _trans = _conn.BeginTransaction();
            return Transaction;
        }

        /// <summary>
        /// Commits any transaction in effect.
        /// </summary>
        public void Commit()
        {
            if (_trans != null)
            {
                _trans.Commit();
                _trans = null;
            }
        }

        /// <summary>
        /// Rolls back any transaction in effect.
        /// </summary>
        public void Rollback()
        {
            if (_trans != null)
            {
                _trans.Rollback();
                _trans = null;
            }
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                // Need to dispose managed resources if being called manually
                if (disposing)
                {
                    if (_conn != null)
                    {
                        Rollback();
                        _conn.Dispose();
                        _conn = null;
                    }
                }
                _disposed = true;
            }
        }

        
        #endregion
    }
}
