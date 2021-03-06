﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Wings21D.Models;
using System.Linq;

namespace Wings21D.Controllers
{
    public class TradeCustomerBalanceController : ApiController
    {
        // GET api/values
        //public IEnumerable<string> Get()
        public HttpResponseMessage Get(string dbName)
        {
            SqlConnection con = new SqlConnection(@"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + dbName + @";Data Source=localhost\SQLEXPRESS");
            DataSet ds = new DataSet();
            List<string> mn = new List<string>();
            SqlDataAdapter da = new SqlDataAdapter();
            DataTable CustomerBalance = new DataTable();

            if (!String.IsNullOrEmpty(dbName))
            {
                try
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand("select * from Trade_CustomerBalance_Table Order By CustomerName", con);
                    da.SelectCommand = cmd;
                    CustomerBalance.TableName = "CustomerBalance";
                    da.Fill(CustomerBalance);
                    con.Close();
                }
                catch (Exception ex)
                {

                }

                var returnResponseObject = new
                {
                    CustomerBalance = CustomerBalance
                };

                var response = Request.CreateResponse(HttpStatusCode.OK, returnResponseObject, MediaTypeHeaderValue.Parse("application/json"));

                return response;
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        // GET api/values/5

        // POST api/values
        public HttpResponseMessage Post(List<TradeCustomerBalance> customerBalance)
        {
            var re = Request;
            var headers = re.Headers;
            String dbName = String.Empty;

            if (headers.Contains("dbname"))
            {
                dbName = headers.GetValues("dbname").First();
            }

            SqlConnection con = new SqlConnection(@"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=" + dbName + @";Data Source=localhost\SQLEXPRESS");
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = con;

            if (!String.IsNullOrEmpty(dbName))
            {
                con.Open();
                cmd.CommandText = "Select * From Trade_CustomerPendingBills_Table";
                SqlDataAdapter customerBalancesAdapter = new SqlDataAdapter();
                DataTable availableCustoemrBalances = new DataTable();
                customerBalancesAdapter.SelectCommand = cmd;
                customerBalancesAdapter.Fill(availableCustoemrBalances);

                con.Close();

                try
                {
                    con.Open();

                    if(availableCustoemrBalances.Rows.Count > 0)
                    {
                        cmd.CommandText = "Delete * from Trade_CustomerPendingBills_Table";
                        cmd.ExecuteNonQuery();
                    }

                    foreach (TradeCustomerBalance tcb in customerBalance)
                    {   
                        cmd.CommandText = "Insert Into Trade_CustomerPendingBills_Table Values('" + tcb.customerName + "', '"
                                          + tcb.billNumber + "', '" + tcb.billDate + "', " + tcb.pendingValue + ")";
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
                catch (Exception ex)
                {
                    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                }

                return new HttpResponseMessage(HttpStatusCode.Created);
            }
            else
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }
    }
}
