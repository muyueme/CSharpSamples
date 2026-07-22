using Microsoft.Data.SqlClient;
using System.Data;

namespace Sample0401.DBHelper
{
    public class SqlHelper : IDisposable
    {
        private static readonly string ConnectionString = "Data Source=192.168.164.128,1433;Initial Catalog=SnowyPro;Persist Security Info=True;User ID=sa;Password=1234.com;Trust Server Certificate=True";

        private static readonly string DefaultSql = "SELECT 1";
        private static readonly string TimeSql = "SELECT getdate()";

        private SqlConnection? sqlConnection = null;
        private SqlTransaction? sqlTransaction = null;
        private SqlCommand? SqlCommand = null;
        public bool HasTransation { get; private set; } = false;

        public static SqlHelper CreateInstance(string? connectionString = null)
        {
            if (connectionString == null)
                return CreateInstanceInternal();
            else
                return CreateInstanceInternal(connectionString);
        }

        private static SqlHelper CreateInstanceInternal()
        {
            var helper = new SqlHelper();
            helper.sqlConnection = new SqlConnection(ConnectionString);
            try
            {
                helper.EnsureConnectionValid(true);
                helper.SqlCommand = new SqlCommand(DefaultSql, helper.sqlConnection);
                return helper;
            }
            catch (Exception ex)
            {
                throw new Exception($"SqlHelper.CreateInstanceInternal error: {ex.Message}");
            }
        }

        private static SqlHelper CreateInstanceInternal(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception($"SqlHelper.CreateInstanceInternal(string connectionString) error: connectionString is null or invalid.");
            }
            var helper = new SqlHelper();
            helper.sqlConnection = new SqlConnection(ConnectionString);
            try
            {
                helper.EnsureConnectionValid();
                helper.SqlCommand = new SqlCommand(DefaultSql, helper.sqlConnection);
                return helper;
            }
            catch (Exception ex)
            {
                throw new Exception($"SqlHelper.CreateInstanceInternal error: {ex.Message}");
            }
        }

        public bool TestConnect()
        {
            try
            {
                this.EnsureConnectionValid();
                this.SqlCommand?.CommandText = DefaultSql;
                return this.SqlCommand?.ExecuteScalar() != null;
            }
            finally
            {
                this.CloseConnection();
            }

        }

        public DateTime GetDbTime()
        {
            try
            {
                this.EnsureConnectionValid();
                this.SqlCommand?.CommandText = TimeSql;
                var data = this.SqlCommand?.ExecuteScalar();
                return Convert.ToDateTime(data);
            }
            finally
            {
                this.CloseConnection();
            }
        }

        /// <summary>
        /// 保证连接处于可用状态
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void EnsureConnectionValid(bool firstInit = false)
        {
            if (this.sqlConnection == null)
            {
                throw new Exception("sqlConnection is null, this instance is invalid please try CreateInstance()" +
                    "to create a new instance.");
            }
            if (!firstInit && this.SqlCommand == null)
            {
                throw new Exception("sqlCommand is null, this instance is invalid please try CreateInstance()" +
                    "to create a new instance.");
            }

            try
            {
                if (this.sqlConnection.State == ConnectionState.Broken)
                {
                    this.sqlConnection.Close(); // 必须先关闭
                    this.sqlConnection.Open();  // 再重新打开
                }
                else if (this.sqlConnection.State == ConnectionState.Closed)
                {
                    this.sqlConnection.Open();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 清理和释放资源
        /// </summary>
        public void CloseConnection()
        {
            try
            {
                this.SqlCommand?.Parameters.Clear();
                this.SqlCommand?.CommandText = string.Empty;
                // 当存在事务时，由于当前事务中的sql单元还未完毕，不能释放连接及相关资源，否则会导致先前的修改丢失
                // sqlConnection,SqlCommand复用技术
                if (!HasTransation)
                {
                    this.sqlTransaction = null;
                    this.SqlCommand?.Transaction = null;
                    this.sqlConnection?.Close();
                }
            }
            catch (Exception)
            {
            }

        }
        /// <summary>
        /// 开始事务
        /// </summary>
        /// <returns></returns>
        public void BeginTransation()
        {
            if (this.sqlTransaction != null)
            {
                return;
            }
            try
            {
                this.EnsureConnectionValid();
                this.sqlTransaction = this.sqlConnection?.BeginTransaction();
                this.SqlCommand?.Transaction = this.sqlTransaction;
                HasTransation = true;
            }
            catch (Exception e)
            {
                HasTransation = false;
                throw new Exception($"SqlHelper.BeginTransation error: {e.Message}");
            }

        }

        public void Commit()
        {
            try
            {
                if (this.sqlTransaction == null)
                {
                    throw new Exception("No transation in this connection.");
                }
                this.sqlTransaction.Commit();
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.CommitTransation error: {e.Message}");
            }
            finally
            {
                HasTransation = false;
                this.CloseConnection();
            }

        }

        public void Rollback()
        {
            try
            {
                if (this.sqlTransaction == null)
                {
                    throw new Exception("No transation in this connection.");
                }
                this.sqlTransaction.Rollback();
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.RollbackTransation error: {e.Message}");
            }
            finally
            {
                HasTransation = false;
                this.CloseConnection();
            }

        }

        /// <summary>
        /// 执行新增、删除、修改操作
        /// <list type="bullet">
        /// <item>若需要使用事务，请在执行此方法之前调用BeginTransation()</item>
        /// <item>所有事务单元操作执行完成后，请调用CommitTransation()或RollbackTransation()
        /// ，否则更改将会丢失</item>
        /// </list>
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public int ExcuteNonQuery(string sql, SqlParameter[]? parameters = null)
        {
            try
            {
                this.EnsureConnectionValid();
                SqlCommand sqlCommand = this.SqlCommand!;
                sqlCommand.CommandText = sql;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                return sqlCommand.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.ExcuteNonQuery error: {e.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }

        /// <summary>
        /// 获取第一行第一列的值
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public object QueryFirstRowColumn(string sql, SqlParameter[]? parameters = null)
        {

            try
            {
                this.EnsureConnectionValid();
                SqlCommand sqlCommand = this.SqlCommand!;
                sqlCommand.CommandText = sql;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                return sqlCommand.ExecuteScalar();
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.QueryFirstRowColumn error: {e.Message}");
            }
            finally
            {
                CloseConnection();
            }
        }

        public DataRow QueryRow(string sql, SqlParameter[]? parameters = null)
        {
            try
            {
                this.EnsureConnectionValid();
                SqlCommand sqlCommand = this.SqlCommand!;
                sqlCommand.CommandText = sql;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                var reader = sqlCommand.ExecuteReader();
                var dt = new DataTable();
                dt.Load(reader);
                reader.Close();
                if (dt.Rows.Count > 0)
                    return dt.Rows[0];
                return null;
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.QueryRow error: {e.Message}");
            }
            finally
            {
                this.CloseConnection();
            }
        }

        public DataTable QueryTable(string sql, SqlParameter[]? parameters = null)
        {
            try
            {
                this.EnsureConnectionValid();
                SqlCommand sqlCommand = this.SqlCommand!;
                sqlCommand.CommandText = sql;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                var reader = sqlCommand.ExecuteReader();
                var dt = new DataTable();
                dt.Load(reader);
                reader.Close();
                return dt;
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.QueryTable error: {e.Message}");
            }
            finally
            {
                this.CloseConnection();
            }
        }

        public DataSet QueryDataSet(string sql, SqlParameter[]? parameters = null)
        {
            try
            {
                this.EnsureConnectionValid();
                SqlCommand sqlCommand = this.SqlCommand!;
                sqlCommand.CommandText = sql;
                if (parameters != null)
                {
                    sqlCommand.Parameters.AddRange(parameters);
                }
                SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(sqlCommand);
                var ds = new DataSet();
                sqlDataAdapter.Fill(ds);
                return ds;
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.QueryDataSet error: {e.Message}");
            }
            finally
            {
                this.CloseConnection();
            }
        }

        public SqlDataReader GetReader(string sql, SqlParameter[]? parameters = null)
        {
            try
            {
                EnsureConnectionValid();
                this.SqlCommand!.CommandText = sql;
                if (parameters != null)
                {
                    this.SqlCommand.Parameters.AddRange(parameters);
                }
                return this.SqlCommand.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception e)
            {
                throw new Exception($"SqlHelper.GetReader error: {e.Message}");
            }
        }

        public void Dispose()
        {
            try
            {
                if (HasTransation)
                {
                    this.sqlTransaction?.Commit();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                HasTransation = false;
                CloseConnection();
                GC.SuppressFinalize(this);
            }

        }
    }
}

