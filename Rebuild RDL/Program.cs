// Rebuild RDL Files From ReportServer Database
// Christopher Lee - 2022
//
// This program is free software : you can redistribute itand /or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.If not, see < https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RebuildRDL
{
    class Program
    {
        static void Main(string[] args)
        {
            // Connection string to underlying ReportServer database which holds SSRS data.
            // By default the connection is established as a trusted connection.   

            var connstr = @"Server = myServerAddress; Database = ReportServer; Trusted_Connection = True";

            // T-SQL code to select and transform data in the ReportServer database.
            // The structure of the output should then be in a format which can then be written to RDL files.

            var query = "SELECT C.NAME, CONVERT(NVARCHAR(MAX),CONVERT(XML,CONVERT(VARBINARY(MAX),C.CONTENT))) AS REPORTXML FROM  REPORTSERVER.DBO.CATALOG C WHERE  C.CONTENT IS NOT NULL AND C.TYPE = 2";
            var dadp = new SqlDataAdapter(query, connstr);
            var dtble = new DataTable();
            dadp.Fill(dtble);

            foreach (DataRow item in dtble.Rows)
            {
                // Path that the RDL files will be written to.
                // With the trusted connection being used, the path chosen needs to be one that the user has write permissions for.

                var path = @"C:\";

                path += item["NAME"].ToString() + ".rdl";

                // If the file at the stated path already exists then delete it.

                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                // Write data from ReportServer database to RDL files.

                using (FileStream fs = File.Create(path))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(item["REPORTXML"].ToString());

                    fs.Write(info, 0, info.Length);
                }

                Console.WriteLine("Process completed.");

            }
        }
    }
}
