using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;

namespace Utilities
    {
    public class MDBSQLAdapter
        {
        OleDbConnection connection;
        //-------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------
        public MDBSQLAdapter(string path)
            {
            //System.Data.OleDb.OleDbConnection connection = new System.Data.OleDb.OleDbConnection();
            connection = new System.Data.OleDb.OleDbConnection();

            //Set connection String
            connection.ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data source=" +
                path;
            }
        //-------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------
        public int execSQLNonQuery(string SQL)
            {
            connection.Open();

            System.Data.OleDb.OleDbCommand command = connection.CreateCommand();

            command.CommandText = SQL;

            int rowsAffected;
            rowsAffected = command.ExecuteNonQuery();

            connection.Close();

            return rowsAffected;
            }
        //-------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------
        public OleDbDataReader execSQLQuery(string SQL)
            {
            connection.Open();

            OleDbCommand command = connection.CreateCommand();

            command.CommandText = SQL;

            OleDbDataReader reader = command.ExecuteReader();

            //connection.Close();

            return reader;
            }
        //-------------------------------------------------------------------------------
        //
        //-------------------------------------------------------------------------------
        public void Close()
            {
            connection.Close();
            }
        }
    }
