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

        public string doubleQuoteEscapeSequence = "``"; // We can't enter double-quotes inline into a claims-rule, so we need to substitute. The default is two backticks.

        #region IAttributeStore Members

        public IAsyncResult BeginExecuteQuery(string query, string[] parameters, AsyncCallback callback, object state)
        {
            if (String.IsNullOrEmpty(query))
            {
                throw new AttributeStoreQueryFormatException("No query string defined in claim issuance rule. Please contact your system administrator.");
            }

            query = query.Replace(doubleQuoteEscapeSequence, "\"");

            List<string[]> claimData = new List<string[]>();

            using (PowerShell PowerShellInst = PowerShell.Create())
            {
                PowerShellInst.AddScript(query, true);
                // Recursively add arguments to the Powershell Script
                if (null != parameters)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        PowerShellInst.AddArgument(parameters[i]);
                    }
                }
                try
                {
                    Collection<PSObject> results = PowerShellInst.Invoke();

                    foreach (PSObject obj in results)
                    {
                        claimData.Add(new string[1] { obj.ToString() });
                    }
                }
                catch (Exception ex)
                {
                    throw new AttributeStoreQueryExecutionException(ex.Message);
                }
            }

            string[][] outputValues = claimData.ToArray();

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
            // Change the Double-Quote Escape Sequence if Defined
            if (config.ContainsKey("doubleQuoteEscapeSequence"))
            {
                doubleQuoteEscapeSequence = config["doubleQuoteEscapeSequence"];
            }
        }
        #endregion
    }
}