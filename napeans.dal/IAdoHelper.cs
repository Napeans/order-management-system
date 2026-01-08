using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace napeans.dal
{
   public interface IAdoHelper
    {

        void UpdateWithDatatable(string strProc, DataTable dt, string dtParamName, Dictionary<string, string> param = null);
        int ExecNonQuery(string qry, Dictionary<string, string> args);
        int ExecNonQuery(string qry);
        int ExecNonQueryProc(string proc, Dictionary<string, string> args);


        object ExecScalar(string qry, Dictionary<string, string> args);
        object ExecScalarProc(string qry, Dictionary<string, string> args);

         DataSet ExecDataSet(string qry, Dictionary<string, string> args);
        DataSet ExecDataSet(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null);
        DataSet ExecDataSetProc(string qry, Dictionary<string, string> args);
        DataSet ExecDataSetProc(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null);
         
        List<T> ExecDataSet<T>(string qry, Dictionary<string, string> args);
        List<T> ExecDataSet<T>(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null);
        List<T> ExecDataSetProc<T>(string qry, Dictionary<string, string> args);
        List<T> ExecDataSetProc<T>(string qry, List<SqlParameter> param = null, Dictionary<string, string> args = null);
        //object ExecDataSet(string sql, Dictionary<string, string> parameters);
    }
}
