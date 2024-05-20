using Microsoft.AspNetCore.Hosting.Infrastructure;
using Microsoft.Extensions.Configuration;
using Npgsql;
using NpgsqlTypes;
using System.Data;
using System.Text;



namespace ServerLibrary.Helpers
{

    public enum DBCommandType
    {
        Procedure,
        Function
    }

    public  class PostgreSQLTools
    {

        #region ATTRIBUTES

        private readonly string connectionString = string.Empty;
        private string returnedMsg=string.Empty;
        private int paramCount = 0;
        private string commandString = string.Empty;
        #endregion


        #region CONSTRUCTOR

        public PostgreSQLTools() { }


        public PostgreSQLTools(string connectionString)
        {
            this.connectionString = connectionString;
        }

        #endregion

        #region PROPERTIES

        /// <summary>
        /// Messages returned from the database
        /// </summary>
        public string ReturnedMsg
        {
            get { return returnedMsg; }
        }

        #endregion



        #region METHODS

        /// <summary>
        /// Get data from DataBase
        /// </summary>
        /// <param name="ProcOrFuncName">The given name of the Procedure or Function on DataBase</param>
        /// <param name="CmdType">Type of command to execute on the DataBase</param>
        /// <param name="parametersArray">[Optional] Parameters array</param>
        /// <returns>Return a DataTable with all the information from DataBase</returns>
        public DataTable GetDbData(string ProcOrFuncName, DBCommandType CmdType, NpgsqlParameter[]? parametersArray = null)
        {
            DataTable? resultsDt = null;

            try
            {
                if (parametersArray != null)
                {
                    paramCount = parametersArray.Length;
                }

                //Build, dynamically, the command string
                commandString = CommandStringBuilder(paramCount, CmdType, ProcOrFuncName);

                //Create and define a connection to the database with a given connection string
                using (NpgsqlConnection npgConn = new NpgsqlConnection(connectionString))
                {
                    npgConn.Open();

                    //Create and define a command to call a given function
                    using (NpgsqlCommand npgCmd = new NpgsqlCommand(commandString, npgConn))
                    {
                        if (parametersArray != null)
                        {
                            //Add a parameter
                            npgCmd.Parameters.AddRange(parametersArray);
                        }

                        //Create a dataAdapater to get information from database
                        using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(npgCmd))
                        {
                            //Initialize a new dataSet to be filled by the DataAdapter
                            resultsDt = new DataTable();

                            //Try to fill the DataSet. If is empty send a outer message displaying it 
                            if (da.Fill(resultsDt) < 1)
                            {
                                returnedMsg = RuntimeMessages.EmptyDataStruct;
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return resultsDt;
        }

        /// <summary>
        /// Get data from a result set on the DataBase
        /// </summary>
        /// <param name="ProcOrFuncName">Name of the function on database</param>
        /// <param name="parametersArray">[Optional] Parameters array</param>
        /// <returns>A DataSet with all the data retuned from database</returns>
        public DataSet GetResultSetData(string ProcOrFuncName, DBCommandType CmdType, NpgsqlParameter[]? parametersArray = null)
        {
            DataSet? resultSets = null;

            try
            {
                if (parametersArray != null)
                {
                    paramCount = parametersArray.Length;
                }

                //Build, dynamically, the command string
                commandString = CommandStringBuilder(paramCount, CmdType, ProcOrFuncName);

                //Create and define a connection to the database with a given connection string
                using (NpgsqlConnection npgConn = new NpgsqlConnection(connectionString))
                {
                    npgConn.Open();

                    //Start a transaction with database
                    using (NpgsqlTransaction transaction = npgConn.BeginTransaction())
                    {
                        //Create and define a command to call a given function
                        using (NpgsqlCommand npgCmd = new NpgsqlCommand(commandString, npgConn))
                        {
                            //Add an array of parameters if not null [optional]
                            if (parametersArray != null)
                            {
                                npgCmd.Parameters.AddRange(parametersArray);
                            }

                            //Define command type as a stored procedure (functions)
                            //npgCmd.CommandType = CommandType.StoredProcedure;

                            StringBuilder fetchSql = new StringBuilder();

                            using (var reader = npgCmd.ExecuteReader(CommandBehavior.SequentialAccess))
                            {
                                while (reader.Read())
                                {
                                    //Build a string to fecth all the data from the cursors
                                    //Get's the name of the cursors from the reader
                                    fetchSql.AppendLine($"FETCH ALL IN \"{reader.GetString(0)}\";");
                                }
                            }

                            using (var cmd2 = new NpgsqlCommand())
                            {
                                cmd2.Connection = npgCmd.Connection;
                                cmd2.Transaction = npgCmd.Transaction;
                                cmd2.CommandTimeout = npgCmd.CommandTimeout;
                                cmd2.CommandText = fetchSql.ToString();
                                cmd2.CommandType = CommandType.Text;

                                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd2))
                                {
                                    //Initialize a new dataSet to be filled by the DataAdapter
                                    resultSets = new DataSet();

                                    //Try to fill the DataSet. If is empty send a outer message displaying it 
                                    if (da.Fill(resultSets) < 1)
                                    {
                                        returnedMsg = RuntimeMessages.EmptyDataStruct;
                                    }

                                    //commit transaction
                                    transaction.Commit();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (resultSets is not null)
                {
                    resultSets.Dispose();
                }
                
            }
            return resultSets;
        }


        /// <summary>
        /// Method to set data into DataBase
        /// </summary>
        /// <param name="functionName">A given name of the function on DataBase</param>
        /// <param name="parameters">An array of input parameter</param>
        /// <returns>A string containing messages from DataBase</returns>
        public bool SetDbData(string functionName, NpgsqlParameter[] parameters)
        {
            bool inserted = false;

            try
            {
                //Create and define a connection to the database with a given connection string
                using (NpgsqlConnection npgConn = new NpgsqlConnection(connectionString))
                {
                    npgConn.Open();

                    using (NpgsqlCommand npgCmd = new NpgsqlCommand(functionName, npgConn))
                    {
                        //Add the entry array of parameters
                        npgCmd.Parameters.AddRange(parameters);

                        //Define command type as a stored procedure (functions)
                        npgCmd.CommandType = CommandType.StoredProcedure;


                        inserted = Convert.ToBoolean(npgCmd.ExecuteScalar());

                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            //return any message generated in the code above
            return inserted;
        }
        

        public int SetDbDataScalar(string functionName, NpgsqlParameter[] parameters)
        {
            int inserted = 0;

            try
            {
                //Create and define a connection to the database with a given connection string
                using (NpgsqlConnection npgConn = new NpgsqlConnection(connectionString))
                {
                    npgConn.Open();

                    using (NpgsqlCommand npgCmd = new NpgsqlCommand(functionName, npgConn))
                    {
                        //Add the entry array of parameters
                        npgCmd.Parameters.AddRange(parameters);

                        //Define command type as a stored procedure (functions)
                        npgCmd.CommandType = CommandType.StoredProcedure;

                        //Exectute the non query statement and return the number of rows affected
                        //results = npgCmd.ExecuteNonQuery();


                        inserted = Convert.ToInt32(npgCmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            //return any message generated in the code above
            return inserted;
        }


        /// <summary>
        /// Create a NpgsqlParameter to be used with a NpgsqlCommand
        /// </summary>
        /// <param name="parName">Parameter name in the database</param>
        /// <param name="parType">Parameter type of the column in database</param>
        /// <param name="parValue">Parameter value to be inserted in the database</param>
        /// <param name="parDirection">Parameter direction in the database</param>
        /// <returns>NpgsqlParameter with the given values</returns>
        public NpgsqlParameter CreateNpgParameter(string parName, NpgsqlDbType parType, object parValue, ParameterDirection parDirection)
        {
            NpgsqlParameter? param = null;

            try
            {
                if (!string.IsNullOrEmpty(parName) && parValue != null)
                {
                    param = new NpgsqlParameter
                    {
                        ParameterName = parName,
                        NpgsqlDbType = parType,
                        NpgsqlValue = parValue,
                        Direction = parDirection
                    };
                }
                else
                {
                    throw new Exception(RuntimeMessages.NullParameter);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return param;
        }


        /// <summary>
        /// Build the command execution string
        /// </summary>
        /// <param name="paramCount">Number of parameters, if any</param>
        /// <param name="CmdType">Type of operation that command must execute</param>
        /// <param name="ProcOrFuncName">Name of procedure or function</param>
        /// <returns>A string builded with the operation and parameters</returns>
        private string CommandStringBuilder(int paramCount, DBCommandType CmdType, string ProcOrFuncName)
        {
            string commandString;

            if (CmdType == DBCommandType.Procedure)
            {
                commandString = $"CALL {ProcOrFuncName} (";
            }
            else
            {
                commandString = $"SELECT {ProcOrFuncName} (";
            }

            //Build the parameters body
            if (paramCount > 0)
            {
                for (int i = 1; i <= paramCount; i++)
                {
                    commandString += i.ToString();

                    if (i < paramCount)
                    {
                        commandString += ",";
                    }
                }
            }

            commandString += ")";


            return commandString;
        }

        #endregion

    }
}