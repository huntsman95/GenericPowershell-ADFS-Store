using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityServer.ClaimsPolicy.Engine.AttributeStore;
using System.IdentityModel;
using System.Management.Automation;
using System.Collections.ObjectModel;

namespace Skryptek
{
    public class GenericPowershell : IAttributeStore
    {
        #region IAttributeStore Members

        public IAsyncResult BeginExecuteQuery(string query, string[] parameters, AsyncCallback callback, object state)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new AttributeStoreQueryFormatException("No query string.");
            }

            if (parameters == null)
            {
                throw new AttributeStoreQueryFormatException("No query parameter.");
            }

            string result = null;

            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                string queryF = string.Format(query,parameters);
                PowerShellInst.AddScript(queryF, true);
                try
                {
                    Collection<PSObject> results = PowerShellInst.Invoke();

                    // close the runspace

                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (PSObject obj in results)
                    {
                        stringBuilder.AppendLine(obj.ToString());
                    }

                    result = stringBuilder.ToString();
                }
                catch (Exception ex)
                {
                    throw new AttributeStoreQueryExecutionException(ex.Message);
                }
            }

            string[][] outputValues = new string[1][];
            outputValues[0] = new string[1];
            outputValues[0][0] = result;

            TypedAsyncResult<string[][]> asyncResult = new TypedAsyncResult<string[][]>(callback, state);
            asyncResult.Complete(outputValues, true);
            return asyncResult;
        }

        public string[][] EndExecuteQuery(IAsyncResult result)
        {
            return TypedAsyncResult<string[][]>.End(result);
        }

        public void Initialize(Dictionary<string, string> config)
        {
            // No initialization is required for this store.
        }
        #endregion
    }
}